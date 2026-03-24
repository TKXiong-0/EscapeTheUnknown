using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterControl : MonoBehaviour, IDamage, IPickUp
{
    [System.Serializable]
    public class InventoryItem
    {
        public ItemType itemType;

        public GunStats gunStats;
        public MeleeStats meleeStats;
        public HealStats healStats;
    }

    public enum ItemType
    {
        Gun,
        Melee,
        Heal
    }

    [Header("--------------- Components ---------------")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("--------------- Controller ---------------")]
    [Range(1, 10)][SerializeField] int HP;
    [Range(1, 10)][SerializeField] int speed;
    [Range(1, 10)][SerializeField] int sprintMod;
    [Range(1, 20)][SerializeField] int jumpSpeed;
    [Range(1, 5)][SerializeField] int jumpTimeMax;
    [Range(15, 56)][SerializeField] int Gravity;

    [Header("--------------- Inventory ---------------")]
    [SerializeField] List<InventoryItem> inventory = new List<InventoryItem>();
    [SerializeField] GameObject heldItemModel;

    [Header("--------------- Audio ---------------")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audJump;
    [SerializeField] float audJumpVol;
    [SerializeField] AudioClip[] audHurt;
    [SerializeField] float audHurtVol;
    [SerializeField] AudioClip[] audStep;
    [SerializeField] float audStepVol;

    int jumpCount;
    int HPorigin;
    int inventoryPos;

    float useTimer;

    bool isPlayingStep;
    bool isSprinting;

    Vector3 MoveDir;
    Vector3 playerVel;

    void Start()
    {
        HPorigin = HP;
        spawnPlayer();

        if (inventory.Count > 0)
            changeItem();
    }

    void Update()
    {
        if (!GameManager.instance.isPaused)
        {
            movement();
            Sprint();
        }
    }

    IEnumerator playStep()
    {
        isPlayingStep = true;

        if (audStep.Length > 0)
            aud.PlayOneShot(audStep[Random.Range(0, audStep.Length)], audStepVol);

        if (isSprinting)
            yield return new WaitForSeconds(0.5f);
        else
            yield return new WaitForSeconds(0.3f);

        isPlayingStep = false;
    }

    public void spawnPlayer()
    {
        controller.transform.position = GameManager.instance.playerSpawnPos.transform.position;
        Physics.SyncTransforms();
        HP = HPorigin;
        updatePlayerUI();
    }

    void movement()
    {
        useTimer += Time.deltaTime;

        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = 0;
        }

        MoveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(MoveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        playerVel.y -= Gravity * Time.deltaTime;

        if (Input.GetButton("Fire1") && inventory.Count > 0 && canUseCurrentItem())
        {
            useCurrentItem();
        }

        selectItem();

        if (MoveDir.normalized.magnitude > 0.3f && !isPlayingStep)
            StartCoroutine(playStep());
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpTimeMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;

            if (audJump.Length > 0)
                aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
        }
    }

    void Sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
            isSprinting = false;
        }
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
                       useTimer >= 0.2f &&
                       HP < HPorigin;

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

    void shootGun(GunStats gun)
    {
        if (gun == null) return;

        useTimer = 0;
        gun.ammoCur--;

        if (gun.shootSound.Length > 0)
            aud.PlayOneShot(gun.shootSound[Random.Range(0, gun.shootSound.Length)], gun.shootSoundVel);

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, gun.shootDistance, ~ignoreLayer))
        {
            if (gun.hitEffect != null)
                Instantiate(gun.hitEffect, hit.point, Quaternion.identity);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takedamage(gun.shootDamage);
        }
    }

    void swingMelee(MeleeStats melee)
    {
        if (melee == null) return;

        useTimer = 0;

        if (melee.swingSound.Length > 0)
            aud.PlayOneShot(melee.swingSound[Random.Range(0, melee.swingSound.Length)], melee.swingSoundVol);

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, melee.meleeRange, ~ignoreLayer))
        {
            if (melee.hitEffect != null)
                Instantiate(melee.hitEffect, hit.point, Quaternion.identity);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
                dmg.takedamage(melee.meleeDamage);
        }
    }

    void useHeal(HealStats heal)
    {
        if (heal == null) return;

        useTimer = 0;

        HP += heal.healAmount;
        if (HP > HPorigin)
            HP = HPorigin;

        updatePlayerUI();

        if (heal.healSound.Length > 0)
            aud.PlayOneShot(heal.healSound[Random.Range(0, heal.healSound.Length)], heal.healSoundVol);

        // one-time use heal item
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

    public void takedamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();

        if (audHurt.Length > 0)
            aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);

        StartCoroutine(flashDamage());

        if (HP <= 0)
            GameManager.instance.youLose();
    }

    IEnumerator flashDamage()
    {
        GameManager.instance.DamagePlayerFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.DamagePlayerFlash.SetActive(false);
    }

    public void updatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)HP / HPorigin;
    }

    public void getGunStats(GunStats gun)
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

    void changeItem()
    {
        if (inventory.Count == 0 || heldItemModel == null)
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

        if (modelToUse == null) return;

        MeshFilter heldFilter = heldItemModel.GetComponent<MeshFilter>();
        MeshRenderer heldRenderer = heldItemModel.GetComponent<MeshRenderer>();

        MeshFilter newFilter = modelToUse.GetComponent<MeshFilter>();
        MeshRenderer newRenderer = modelToUse.GetComponent<MeshRenderer>();

        if (heldFilter != null && newFilter != null)
            heldFilter.sharedMesh = newFilter.sharedMesh;

        if (heldRenderer != null && newRenderer != null)
            heldRenderer.sharedMaterial = newRenderer.sharedMaterial;
    }

    void clearHeldItem()
    {
        MeshFilter heldFilter = heldItemModel.GetComponent<MeshFilter>();
        MeshRenderer heldRenderer = heldItemModel.GetComponent<MeshRenderer>();

        if (heldFilter != null)
            heldFilter.sharedMesh = null;

        if (heldRenderer != null)
            heldRenderer.sharedMaterial = null;
    }

    void selectItem()
    {
        if (inventory.Count == 0) return;

        if (Input.GetAxis("Mouse ScrollWheel") > 0 && inventoryPos < inventory.Count - 1)
        {
            inventoryPos++;
            changeItem();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && inventoryPos > 0)
        {
            inventoryPos--;
            changeItem();
        }
    }
}