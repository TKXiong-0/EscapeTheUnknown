using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour, IDamage, IPickup
{
    [System.Serializable]
    public class InventoryItem
    {
        public ItemType itemType;
        public gunStats gunStats;
        public MeleeStats meleeStats;
        public HealStats healStats;
    }

    public enum ItemType
    {
        Gun,
        Melee,
        Heal
    }

    [Header("----- Components -----")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] GameObject heldItemModel;
    GameObject currentHeldObject;
    [SerializeField] Animator playerAnim;

    [Header("----- Animation Parameters -----")]
    [SerializeField] string gunShootTrigger = "GunShoot";
    [SerializeField] string reloadTrigger = "Reload";
    [SerializeField] string speedFloat = "Speed";
    [SerializeField] string jumpTrigger = "JumpTrig";
    [SerializeField] string meleeTrigger = "MeleeTrig";

    [Header("----- Player Stats -----")]
    [SerializeField] int HP = 10;
    [SerializeField] int speed = 5;
    [SerializeField] int sprintMod = 2;
    [SerializeField] int jumpSpeed = 10;
    [SerializeField] int jumpTimesMax = 2;
    [SerializeField] int gravity = 20;

    [Header("----- Stamina -----")]
    [SerializeField] Image staminaBar;
    [SerializeField] float stamina = 100f;
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float jumpCost = 20f;
    [SerializeField] float runCost = 15f;
    [SerializeField] float chargeRate = 25f;
    [SerializeField] float staminaRechargeDelay = 1f;

    [Header("----- Inventory -----")]
    [SerializeField] List<InventoryItem> inventory = new List<InventoryItem>();

    [Header("----- Audio -----")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audJump;
    [SerializeField][Range(0f, 1f)] float audJumpVol = 0.5f;
    [SerializeField] AudioClip[] audHurt;
    [SerializeField][Range(0f, 1f)] float audHurtVol = 0.5f;
    [SerializeField] AudioClip[] audStep;
    [SerializeField][Range(0f, 1f)] float audStepVol = 0.5f;

    int jumpCount;
    int hpOrig;
    int baseSpeed;
    int inventoryPos;

    float useTimer;

    bool isPlayingStep;
    bool isSprinting;
    bool isReloading;

    Vector3 moveDir;
    Vector3 playerVel;

    Coroutine rechargeRoutine;
    Coroutine reloadRoutine;

    void Start()
    {
        baseSpeed = speed;
        hpOrig = HP;
        stamina = maxStamina;
        spawnPlayer();
        updateStaminaUI();

        if (inventory.Count > 0)
            changeItem();
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isPaused)
            return;

        movement();
        sprint();
        selectItem();
        handleReloadInput();
        updateAnimatorMovement();
    }

    public void spawnPlayer()
    {
        if (GameManager.instance != null && GameManager.instance.playerSpawnPos != null)
        {
            controller.transform.position = GameManager.instance.playerSpawnPos.transform.position;
            Physics.SyncTransforms();
        }

        HP = hpOrig;
        stamina = maxStamina;
        isReloading = false;

        updatePlayerUI();
        updateStaminaUI();
    }

    void movement()
    {
        useTimer += Time.deltaTime;

        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = 0;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();

        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;

        if (!isReloading && Input.GetButton("Fire1") && inventory.Count > 0 && canUseCurrentItem())
        {
            useCurrentItem();
        }

        if (moveDir.normalized.magnitude > 0.3f && controller.isGrounded && !isPlayingStep)
            StartCoroutine(playStep());
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpTimesMax && stamina >= jumpCost)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;

            useStamina(jumpCost);

            if (playerAnim != null)
                playerAnim.SetTrigger(jumpTrigger);

            if (audJump != null && audJump.Length > 0)
                aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
        }
    }

    void sprint()
    {
        bool tryingToSprint = Input.GetButton("Sprint") && moveDir.magnitude > 0.1f;

        if (tryingToSprint && stamina > 0f)
        {
            speed = baseSpeed * sprintMod;
            isSprinting = true;
            useStamina(runCost * Time.deltaTime);
        }
        else
        {
            speed = baseSpeed;
            isSprinting = false;
        }
    }

    void handleReloadInput()
    {
        if (inventory.Count == 0 || isReloading)
            return;

        InventoryItem currentItem = inventory[inventoryPos];

        if (currentItem.itemType != ItemType.Gun || currentItem.gunStats == null)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentItem.gunStats.ammoCur < currentItem.gunStats.ammoMax)
            {
                if (reloadRoutine != null)
                    StopCoroutine(reloadRoutine);

                reloadRoutine = StartCoroutine(reloadGun(currentItem.gunStats));
            }
        }
    }

    IEnumerator reloadGun(gunStats gun)
    {
        isReloading = true;

        if (playerAnim != null)
            playerAnim.SetTrigger(reloadTrigger);

        // change this if your reload animation is longer/shorter
        yield return new WaitForSeconds(1.0f);

        gun.ammoCur = gun.ammoMax;
        isReloading = false;
    }

    void useStamina(float amount)
    {
        stamina -= amount;
        if (stamina < 0f)
            stamina = 0f;

        updateStaminaUI();
        restartStaminaRecharge();
    }

    void restartStaminaRecharge()
    {
        if (rechargeRoutine != null)
            StopCoroutine(rechargeRoutine);

        rechargeRoutine = StartCoroutine(rechargeStamina());
    }

    IEnumerator rechargeStamina()
    {
        yield return new WaitForSeconds(staminaRechargeDelay);

        while (stamina < maxStamina)
        {
            if (Input.GetButton("Sprint") && moveDir.magnitude > 0.1f)
                yield break;

            stamina += chargeRate * Time.deltaTime;

            if (stamina > maxStamina)
                stamina = maxStamina;

            updateStaminaUI();
            yield return null;
        }
    }

    IEnumerator playStep()
    {
        isPlayingStep = true;

        if (audStep != null && audStep.Length > 0)
            aud.PlayOneShot(audStep[Random.Range(0, audStep.Length)], audStepVol);

        yield return new WaitForSeconds(isSprinting ? 0.3f : 0.45f);

        isPlayingStep = false;
    }

    void updateAnimatorMovement()
    {
        if (playerAnim == null)
            return;

        float currentSpeed = moveDir.magnitude;
        playerAnim.SetFloat(speedFloat, currentSpeed);
    }

    bool canUseCurrentItem()
    {
        InventoryItem currentItem = inventory[inventoryPos];

        switch (currentItem.itemType)
        {
            case ItemType.Gun:
                return currentItem.gunStats != null &&
                       currentItem.gunStats.ammoCur > 0 &&
                       useTimer >= currentItem.gunStats.shootRate;

            case ItemType.Melee:
                return currentItem.meleeStats != null &&
                       useTimer >= currentItem.meleeStats.attackRate;

            case ItemType.Heal:
                return currentItem.healStats != null &&
                       HP < hpOrig &&
                       useTimer >= 0.2f;

            default:
                return false;
        }
    }

    void useCurrentItem()
    {
        InventoryItem currentItem = inventory[inventoryPos];

        switch (currentItem.itemType)
        {
            case ItemType.Gun:
                shootGun(currentItem.gunStats);
                break;

            case ItemType.Melee:
                swingMelee(currentItem.meleeStats);
                break;

            case ItemType.Heal:
                useHeal(currentItem.healStats);
                break;
        }
    }

    void shootGun(gunStats gun)
    {
        if (gun == null || isReloading)
            return;

        useTimer = 0f;
        gun.ammoCur--;

        if (playerAnim != null)
            playerAnim.SetTrigger(gunShootTrigger);

        if (gun.shootSound != null && gun.shootSound.Length > 0)
            aud.PlayOneShot(gun.shootSound[Random.Range(0, gun.shootSound.Length)], gun.shootSoundVol);

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, gun.shootDistance, ~ignoreLayer))
        {
            if (gun.hitEffect != null)
                Instantiate(gun.hitEffect, hit.point, Quaternion.identity);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage(gun.shootDamage);
        }
    }

    void swingMelee(MeleeStats melee)
    {
        if (melee == null)
            return;

        useTimer = 0f;

        if (playerAnim != null)
            playerAnim.SetTrigger(meleeTrigger);

        if (melee.swingSound != null && melee.swingSound.Length > 0)
            aud.PlayOneShot(melee.swingSound[Random.Range(0, melee.swingSound.Length)], melee.swingSoundVol);

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, melee.meleeRange, ~ignoreLayer))
        {
            if (melee.hitEffect != null)
                Instantiate(melee.hitEffect, hit.point, Quaternion.identity);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage(melee.meleeDamage);
        }
    }

    void useHeal(HealStats heal)
    {
        if (heal == null)
            return;

        useTimer = 0f;

        HP += heal.healAmount;
        if (HP > hpOrig)
            HP = hpOrig;

        updatePlayerUI();

        if (heal.healSound != null && heal.healSound.Length > 0)
            aud.PlayOneShot(heal.healSound[Random.Range(0, heal.healSound.Length)], heal.healSoundVol);

        inventory.RemoveAt(inventoryPos);

        if (inventory.Count == 0)
        {
            inventoryPos = 0;
            clearHeldItem();
        }
        else
        {
            if (inventoryPos >= inventory.Count)
                inventoryPos = inventory.Count - 1;

            changeItem();
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (HP < 0)
            HP = 0;

        updatePlayerUI();

        if (audHurt != null && audHurt.Length > 0)
            aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);

        StartCoroutine(flashDamage());

        if (HP <= 0 && GameManager.instance != null)
            GameManager.instance.youLose();
    }

    IEnumerator flashDamage()
    {
        if (GameManager.instance != null && GameManager.instance.damagePlayerFlash != null)
        {
            GameManager.instance.damagePlayerFlash.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            GameManager.instance.damagePlayerFlash.SetActive(false);
        }
    }

    public void updatePlayerUI()
    {
        if (GameManager.instance != null && GameManager.instance.playerHPBar != null)
            GameManager.instance.playerHPBar.fillAmount = (float)HP / hpOrig;
    }

    void updateStaminaUI()
    {
        if (staminaBar != null)
            staminaBar.fillAmount = stamina / maxStamina;
    }

    public void getGunStats(gunStats gun)
    {
        InventoryItem newItem = new InventoryItem();
        newItem.itemType = ItemType.Gun;
        newItem.gunStats = gun;

        inventory.Add(newItem);
        inventoryPos = inventory.Count - 1;
        changeItem();
    }

    public void getMeleeStats(MeleeStats melee)
    {
        InventoryItem newItem = new InventoryItem();
        newItem.itemType = ItemType.Melee;
        newItem.meleeStats = melee;

        inventory.Add(newItem);
        inventoryPos = inventory.Count - 1;
        changeItem();
    }

    public void getHealStats(HealStats heal)
    {
        InventoryItem newItem = new InventoryItem();
        newItem.itemType = ItemType.Heal;
        newItem.healStats = heal;

        inventory.Add(newItem);
        inventoryPos = inventory.Count - 1;
        changeItem();
    }

    void selectItem()
    {
        if (inventory.Count == 0)
            return;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f && inventoryPos < inventory.Count - 1)
        {
            inventoryPos++;
            changeItem();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f && inventoryPos > 0)
        {
            inventoryPos--;
            changeItem();
        }
    }

    void changeItem()
    {
        if (heldItemModel == null)
            return;

        if (currentHeldObject != null)
            Destroy(currentHeldObject);

        if (inventory.Count == 0)
            return;

        GameObject modelToUse = null;
        InventoryItem currentItem = inventory[inventoryPos];

        switch (currentItem.itemType)
        {
            case ItemType.Gun:
                if (currentItem.gunStats != null)
                    modelToUse = currentItem.gunStats.gunModel;
                break;

            case ItemType.Melee:
                if (currentItem.meleeStats != null)
                    modelToUse = currentItem.meleeStats.meleeModel;
                break;

            case ItemType.Heal:
                if (currentItem.healStats != null)
                    modelToUse = currentItem.healStats.healModel;
                break;
        }

        if (modelToUse == null)
            return;

        currentHeldObject = Instantiate(modelToUse, heldItemModel.transform);
        currentHeldObject.transform.localPosition = Vector3.zero;
        currentHeldObject.transform.localRotation = Quaternion.identity;
        currentHeldObject.transform.localScale = Vector3.one;
    }

    void clearHeldItem()
    {
        if (currentHeldObject != null)
            Destroy(currentHeldObject);
    }
}