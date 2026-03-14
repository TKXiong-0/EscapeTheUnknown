using UnityEngine;
using System.Collections;

public class PlayerControler2 : MonoBehaviour, IDamage
{
    [Header("--------------- Components ---------------")]
    [SerializeField] CharacterController controller;
    [SerializeField] Animator characterAnimator;
    [SerializeField] Transform playerCamera; // This should be your CameraPivot
    [SerializeField] LayerMask ignoreLayer;

    [Header("--------------- Movement Settings ---------------")]
    [Range(1, 20)][SerializeField] int HP = 10;
    [Range(1, 20)][SerializeField] int speed = 5;
    [Range(1, 10)][SerializeField] int sprintMod = 2;
    [Range(1, 20)][SerializeField] int jumpSpeed = 10;
    [Range(1, 5)][SerializeField] int jumpTimeMax = 1;
    [Range(15, 60)][SerializeField] int Gravity = 20;

    [Header("--------------- Camera Smoothing ---------------")]
    [SerializeField] float cameraHeight = 1.4f;
    [SerializeField] float cameraNormalZ = 0.0f;
    [SerializeField] float cameraSprintZ = 0.4f;
    [SerializeField] float cameraLerpSpeed = 5.0f;

    [Header("--------------- Melee Settings ---------------")]
    [SerializeField] int meleeDamage = 3;
    [SerializeField] float meleeRange = 3.0f;
    [SerializeField] float attackRate = 0.6f;

    int jumpCount;
    int HPorigin;
    float shootTimer;
    Vector3 MoveDir;
    Vector3 playerVel;

    void Start()
    {
        HPorigin = HP;

        // Ensure the HP Bar is full at the start
        if (GameManager.instance != null && GameManager.instance.playerHPBar != null)
        {
            GameManager.instance.playerHPBar.fillAmount = 1;
        }
    }

    void Update()
    {
        // Don't move or attack if the game is paused
        if (GameManager.instance != null && GameManager.instance.isPaused) return;

        MovementLogic();
    }

    void MovementLogic()
    {
        shootTimer += Time.deltaTime;

        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = -2f;
        }

        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");
        bool isSprinting = Input.GetButton("Sprint");

        // Movement
        float currentSpeed = (isSprinting && vInput > 0) ? speed * sprintMod : speed;
        MoveDir = (hInput * transform.right) + (vInput * transform.forward);
        controller.Move(MoveDir * currentSpeed * Time.deltaTime);

        // Jump Logic (Fixed for single animation)
        if (Input.GetButtonDown("Jump") && jumpCount < jumpTimeMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;

            if (characterAnimator != null)
            {
                characterAnimator.ResetTrigger("JumpTrigger");
                characterAnimator.SetTrigger("JumpTrigger");
            }
        }

        playerVel.y -= Gravity * Time.deltaTime;
        controller.Move(playerVel * Time.deltaTime);

        // Camera Lean Logic
        if (playerCamera != null)
        {
            float targetZ = (vInput > 0 && isSprinting) ? cameraSprintZ : cameraNormalZ;
            float currentZ = Mathf.Lerp(playerCamera.localPosition.z, targetZ, Time.deltaTime * cameraLerpSpeed);

            // X is 0 for center, Y is 1.4 for eye level
            playerCamera.localPosition = new Vector3(0, cameraHeight, currentZ);
        }

        // Animator Speed Update
        if (characterAnimator != null)
        {
            float animValue = vInput;
            if (vInput > 0) animValue = isSprinting ? 1.0f : 0.5f;
            characterAnimator.SetFloat("Speed", animValue, 0.1f, Time.deltaTime);
        }

        // Melee Attack (Fixed for single animation)
        if (Input.GetButtonDown("Fire1") && shootTimer >= attackRate)
        {
            Melee();
        }
    }

    void Melee()
    {
        shootTimer = 0;

        if (characterAnimator != null)
        {
            characterAnimator.ResetTrigger("MeleeTrigger");
            characterAnimator.SetTrigger("MeleeTrigger");
        }

        RaycastHit hit;
        // Raycast ignores the Player layer so you don't hit yourself
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, meleeRange, ~ignoreLayer))
        {
            // Look for IDamage on the hit object or its parent (useful for doors/enemies)
            IDamage dmg = hit.collider.GetComponentInParent<IDamage>();
            if (dmg != null)
            {
                dmg.takedamage(meleeDamage);
            }
        }
    }

    public void takedamage(int amount)
    {
        HP -= amount;

        // Update the UI HP Bar
        if (GameManager.instance != null && GameManager.instance.playerHPBar != null)
        {
            GameManager.instance.playerHPBar.fillAmount = (float)HP / HPorigin;
        }

        if (HP <= 0 && GameManager.instance != null)
        {
            GameManager.instance.youLose();
        }
    }
}
