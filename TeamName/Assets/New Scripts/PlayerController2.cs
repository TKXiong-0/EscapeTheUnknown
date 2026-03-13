using UnityEngine;

public class PlayerControler2 : MonoBehaviour, IDamage
{
    [Header("--------------- Components ---------------")]
    [SerializeField] CharacterController controller;
    [SerializeField] Animator characterAnimator;
    [SerializeField] Transform playerCamera; 
    [SerializeField] LayerMask ignoreLayer;

    [Header("--------------- Movement Settings ---------------")]
    [Range(1, 20)][SerializeField] int HP = 10;
    [Range(1, 20)][SerializeField] int speed = 5;
    [Range(1, 10)][SerializeField] int sprintMod = 2;
    [Range(1, 20)][SerializeField] int jumpSpeed = 10;
    [Range(1, 5)][SerializeField] int jumpTimeMax = 1;
    [Range(15, 60)][SerializeField] int Gravity = 20;

    [Header("--------------- Camera Smoothing ---------------")]
    [SerializeField] float cameraHeight = 1.4f;     // Your eye level
    [SerializeField] float cameraNormalZ = 0.0f;    // Center of head
    [SerializeField] float cameraSprintZ = 0.4f;    // Pushed forward during lean
    [SerializeField] float cameraLerpSpeed = 5.0f;

    [Header("--------------- Gun Settings ---------------")]
    [SerializeField] int shootDamage = 1;
    [SerializeField] int shootDis = 50;
    [SerializeField] float shootRate = 0.5f;

    int jumpCount;
    int HPorigin;
    float shootTimer;
    Vector3 MoveDir;
    Vector3 playerVel;

    void Start()
    {
        HPorigin = HP;
    }

    void Update()
    {
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

        float currentSpeed = (isSprinting && vInput > 0) ? speed * sprintMod : speed;
        MoveDir = (hInput * transform.right) + (vInput * transform.forward);
        controller.Move(MoveDir * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && jumpCount < jumpTimeMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;

            if (characterAnimator != null)
                characterAnimator.SetTrigger("JumpTrigger");
        }

        playerVel.y -= Gravity * Time.deltaTime;
        controller.Move(playerVel * Time.deltaTime);

        if (playerCamera != null)
        {

            float targetZ = (vInput > 0 && isSprinting) ? cameraSprintZ : cameraNormalZ;
            float currentZ = Mathf.Lerp(playerCamera.localPosition.z, targetZ, Time.deltaTime * cameraLerpSpeed);

            playerCamera.localPosition = new Vector3(0, cameraHeight, currentZ);
        }

        if (characterAnimator != null)
        {
            float animValue = vInput;
            if (vInput > 0) animValue = isSprinting ? 1.0f : 0.5f;
            characterAnimator.SetFloat("Speed", animValue, 0.1f, Time.deltaTime);
        }

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
    }

    void shoot()
    {
        shootTimer = 0;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDis, ~ignoreLayer))
        {
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null) dmg.takedamage(shootDamage);
        }
    }

    public void takedamage(int amount)
    {
        HP -= amount;
        if (HP <= 0 && GameManager.instance != null) GameManager.instance.youLose();
    }
}
