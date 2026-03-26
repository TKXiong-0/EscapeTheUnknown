using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] Animator playerAnim;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] Transform rightHandSocket;
    [SerializeField] Transform leftHandSocket;

    [Header("----- Animation Parameters -----")]
    [SerializeField] string speedFloat = "Speed";
    [SerializeField] string jumpTrigger = "JumpTrig";
    [SerializeField] string gunShootTrigger = "GunShoot";
    [SerializeField] string reloadTrigger = "Reload";
    [SerializeField] string meleeTrigger = "MeleeTrig";
    [SerializeField] string healTrigger = "HealTrig";
    [SerializeField] string sprintBool = "IsSprinting";
    [SerializeField] string equippedTypeInt = "EquipType";

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
    [SerializeField, Range(0f, 1f)] float audJumpVol = 0.5f;
    [SerializeField] AudioClip[] audHurt;
    [SerializeField, Range(0f, 1f)] float audHurtVol = 0.5f;
    [SerializeField] AudioClip[] audStep;
    [SerializeField, Range(0f, 1f)] float audStepVol = 0.5f;

    int hpOrig;
    int baseSpeed;
    int inventoryPos;
    int jumpCount;

    float useTimer;

    bool isSprinting;
    bool isReloading;
    bool isPlayingStep;

    Vector3 moveDir;
    Vector3 playerVel;

    Coroutine rechargeRoutine;
    GameObject currentEquippedItem;

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        hpOrig = HP;
        baseSpeed = speed;
        stamina = maxStamina;

        spawnPlayer();
        updatePlayerUI();
        updateStaminaUI();
        updateEquippedAnimation();
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isPaused)
            return;

        useTimer += Time.deltaTime;

        handleMovement();
        handleSprint();
        handleJump();
        applyGravity();

        selectItem();
        handleUseInput();
        handleReloadInput();

        updateAnimatorMovement();
        updateSprintAnimation();
    }

    public void spawnPlayer()
    {
        if (controller == null)
            return;

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

    void handleMovement()
    {
        moveDir = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");

        int currentSpeed = isSprinting ? baseSpeed * sprintMod : baseSpeed;
        controller.Move(moveDir * currentSpeed * Time.deltaTime);

        if (controller.isGrounded)
        {
            jumpCount = 0;

            if (playerVel.y < 0f)
                playerVel.y = -2f;
        }

        if (moveDir.magnitude > 0.1f && controller.isGrounded && !isPlayingStep)
            StartCoroutine(playStep());
    }

    void handleSprint()
    {
        bool tryingToSprint = Input.GetButton("Sprint") && moveDir.magnitude > 0.1f && stamina > 0f;

        if (tryingToSprint)
        {
            isSprinting = true;
            useStamina(runCost * Time.deltaTime);
        }
        else
        {
            isSprinting = false;
        }
    }

    void handleJump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpTimesMax && stamina >= jumpCost)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;

            useStamina(jumpCost);

            if (playerAnim != null && !string.IsNullOrEmpty(jumpTrigger))
                playerAnim.SetTrigger(jumpTrigger);

            if (aud != null && audJump != null && audJump.Length > 0)
                aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
        }
    }

    void applyGravity()
    {
        playerVel.y -= gravity * Time.deltaTime;
        controller.Move(playerVel * Time.deltaTime);
    }

    void handleUseInput()
    {
        if (inventory.Count == 0 || isReloading)
            return;

        if (!Input.GetButton("Fire1"))
            return;

        if (!canUseCurrentItem())
            return;

        useCurrentItem();
    }

    void handleReloadInput()
    {
        if (inventory.Count == 0 || isReloading)
            return;

        InventoryItem currentItem = inventory[inventoryPos];

        if (currentItem.itemType != ItemType.Gun || currentItem.gunStats == null)
            return;

        if (Input.GetKeyDown(KeyCode.R) && currentItem.gunStats.ammoCur < currentItem.gunStats.ammoMax)
            StartCoroutine(reloadGun(currentItem.gunStats));
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
                       useTimer >= currentItem.healStats.useRate;

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
                useMelee(currentItem.meleeStats);
                break;

            case ItemType.Heal:
                useHeal(currentItem.healStats);
                break;
        }
    }

    void shootGun(gunStats gun)
    {
        if (gun == null || Camera.main == null)
            return;

        useTimer = 0f;
        gun.ammoCur--;

        if (playerAnim != null && !string.IsNullOrEmpty(gunShootTrigger))
            playerAnim.SetTrigger(gunShootTrigger);

        if (aud != null && gun.shootSound != null && gun.shootSound.Length > 0)
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

    IEnumerator reloadGun(gunStats gun)
    {
        if (gun == null)
            yield break;

        isReloading = true;

        if (playerAnim != null && !string.IsNullOrEmpty(reloadTrigger))
            playerAnim.SetTrigger(reloadTrigger);

        yield return new WaitForSeconds(1f);

        gun.ammoCur = gun.ammoMax;
        isReloading = false;
    }

    void useMelee(MeleeStats melee)
    {
        if (melee == null || Camera.main == null)
            return;

        useTimer = 0f;

        if (playerAnim != null && !string.IsNullOrEmpty(meleeTrigger))
            playerAnim.SetTrigger(meleeTrigger);

        if (aud != null && melee.swingSound != null && melee.swingSound.Length > 0)
            aud.PlayOneShot(melee.swingSound[Random.Range(0, melee.swingSound.Length)], melee.swingSoundVol);

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, melee.attackDistance, ~ignoreLayer))
        {
            if (melee.hitEffect != null)
                Instantiate(melee.hitEffect, hit.point, Quaternion.identity);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takeDamage(melee.attackDamage);
        }
    }

    void useHeal(HealStats heal)
    {
        if (heal == null)
            return;

        useTimer = 0f;

        if (playerAnim != null && !string.IsNullOrEmpty(healTrigger))
            playerAnim.SetTrigger(healTrigger);

        if (aud != null && heal.healSound != null && heal.healSound.Length > 0)
            aud.PlayOneShot(heal.healSound[Random.Range(0, heal.healSound.Length)], heal.healSoundVol);

        HP += heal.healAmount;
        if (HP > hpOrig)
            HP = hpOrig;

        updatePlayerUI();

        inventory.RemoveAt(inventoryPos);

        if (inventory.Count == 0)
        {
            inventoryPos = 0;
            clearEquippedItem();
            updateEquippedAnimation();
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

        if (aud != null && audHurt != null && audHurt.Length > 0)
            aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);

        if (HP <= 0 && GameManager.instance != null)
            GameManager.instance.youLose();
    }

    void selectItem()
    {
        if (inventory.Count == 0)
            return;

        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            inventoryPos++;
            if (inventoryPos >= inventory.Count)
                inventoryPos = 0;

            changeItem();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            inventoryPos--;
            if (inventoryPos < 0)
                inventoryPos = inventory.Count - 1;

            changeItem();
        }
    }

    void changeItem()
    {
        clearEquippedItem();

        if (inventory.Count == 0)
        {
            updateEquippedAnimation();
            return;
        }

        InventoryItem currentItem = inventory[inventoryPos];
        GameObject modelToSpawn = null;
        Transform socketToUse = null;

        Vector3 holdPosition = Vector3.zero;
        Vector3 holdRotation = Vector3.zero;
        Vector3 holdScale = Vector3.one;

        switch (currentItem.itemType)
        {
            case ItemType.Gun:
                if (currentItem.gunStats != null)
                {
                    modelToSpawn = currentItem.gunStats.gunModel;
                    socketToUse = rightHandSocket;
                    holdPosition = currentItem.gunStats.holdPosition;
                    holdRotation = currentItem.gunStats.holdRotation;
                    holdScale = currentItem.gunStats.holdScale;
                }
                break;

            case ItemType.Melee:
                if (currentItem.meleeStats != null)
                {
                    modelToSpawn = currentItem.meleeStats.meleeModel;
                    socketToUse = rightHandSocket;
                    holdPosition = currentItem.meleeStats.holdPosition;
                    holdRotation = currentItem.meleeStats.holdRotation;
                    holdScale = currentItem.meleeStats.holdScale;
                }
                break;

            case ItemType.Heal:
                if (currentItem.healStats != null)
                {
                    modelToSpawn = currentItem.healStats.healModel;
                    socketToUse = leftHandSocket;
                    holdPosition = currentItem.healStats.holdPosition;
                    holdRotation = currentItem.healStats.holdRotation;
                    holdScale = currentItem.healStats.holdScale;
                }
                break;
        }

        if (modelToSpawn != null && socketToUse != null)
        {
            currentEquippedItem = Instantiate(modelToSpawn, socketToUse);
            currentEquippedItem.transform.localPosition = holdPosition;
            currentEquippedItem.transform.localRotation = Quaternion.Euler(holdRotation);
            currentEquippedItem.transform.localScale = holdScale;
        }

        updateEquippedAnimation();
    }

    void updateEquippedAnimation()
    {
        if (playerAnim == null)
            return;

        if (inventory.Count == 0)
        {
            playerAnim.SetInteger(equippedTypeInt, 0);
            return;
        }

        InventoryItem currentItem = inventory[inventoryPos];

        switch (currentItem.itemType)
        {
            case ItemType.Gun:
                playerAnim.SetInteger(equippedTypeInt, 1);
                break;

            case ItemType.Melee:
                playerAnim.SetInteger(equippedTypeInt, 2);
                break;

            case ItemType.Heal:
                playerAnim.SetInteger(equippedTypeInt, 3);
                break;

            default:
                playerAnim.SetInteger(equippedTypeInt, 0);
                break;
        }
    }

    void updateAnimatorMovement()
    {
        if (playerAnim == null)
            return;

        float currentSpeed = new Vector2(moveDir.x, moveDir.z).magnitude;
        playerAnim.SetFloat(speedFloat, currentSpeed);
    }

    void updateSprintAnimation()
    {
        if (playerAnim == null || string.IsNullOrEmpty(sprintBool))
            return;

        playerAnim.SetBool(sprintBool, isSprinting);
    }

    void clearEquippedItem()
    {
        if (currentEquippedItem != null)
        {
            Destroy(currentEquippedItem);
            currentEquippedItem = null;
        }
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

        if (aud != null && audStep != null && audStep.Length > 0)
            aud.PlayOneShot(audStep[Random.Range(0, audStep.Length)], audStepVol);

        yield return new WaitForSeconds(isSprinting ? 0.3f : 0.45f);

        isPlayingStep = false;
    }

    void updatePlayerUI()
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
        if (gun == null)
            return;

        InventoryItem item = new InventoryItem
        {
            itemType = ItemType.Gun,
            gunStats = gun
        };

        inventory.Add(item);
        inventoryPos = inventory.Count - 1;
        changeItem();
    }

    public void getMeleeStats(MeleeStats melee)
    {
        if (melee == null)
            return;

        InventoryItem item = new InventoryItem
        {
            itemType = ItemType.Melee,
            meleeStats = melee
        };

        inventory.Add(item);
        inventoryPos = inventory.Count - 1;
        changeItem();
    }

    public void getHealStats(HealStats heal)
    {
        if (heal == null)
            return;

        InventoryItem item = new InventoryItem
        {
            itemType = ItemType.Heal,
            healStats = heal
        };

        inventory.Add(item);
        inventoryPos = inventory.Count - 1;
        changeItem();
    }
}