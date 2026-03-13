using UnityEngine;

public class PlayerControler2 : MonoBehaviour, IDamage
{
    [Header("--------------- Components ---------------")]
    [SerializeField] CharacterController controller;
    [SerializeField] Animator characterAnimator; // Drag Dummy here
    [SerializeField] LayerMask ignoreLayer;

    [Header("--------------- Controller ---------------")]
    [Range(1, 20)][SerializeField] int HP = 10;
    [Range(1, 20)][SerializeField] int speed = 5;
    [Range(1, 10)][SerializeField] int sprintMod = 2;
    [Range(1, 20)][SerializeField] int jumpSpeed = 10;
    [Range(1, 5)][SerializeField] int jumpTimeMax = 1;
    [Range(15, 60)][SerializeField] int Gravity = 20;

    [Header("--------------- Guns ---------------")]
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
        movement();
        Sprint();
    }

    void movement()
    {
        shootTimer += Time.deltaTime;

        // 1. Reset gravity when on floor
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = -2f; // Helps 'isGrounded' stay true
        }

        // 2. Your Original Physics Math
        float vInput = Input.GetAxis("Vertical");
        float hInput = Input.GetAxis("Horizontal");

        MoveDir = hInput * transform.right + vInput * transform.forward;
        controller.Move(MoveDir * speed * Time.deltaTime);

        jump();

        // 3. Apply Gravity
        playerVel.y -= Gravity * Time.deltaTime;
        controller.Move(playerVel * Time.deltaTime);

        // 4. THE ONLY NEW PART: Update Animations
        if (characterAnimator != null)
        {
            float animValue = vInput;

            // If moving forward and Shift is held
            if (vInput > 0 && Input.GetButton("Sprint")) animValue = 1.0f;
            // If moving forward normally
            else if (vInput > 0) animValue = 0.5f;

            characterAnimator.SetFloat("Speed", animValue, 0.1f, Time.deltaTime);
        }

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpTimeMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;

            if (characterAnimator != null)
                characterAnimator.SetTrigger("JumpTrigger");
        }
    }

    void Sprint()
    {
        if (Input.GetButtonDown("Sprint")) speed *= sprintMod;
        else if (Input.GetButtonUp("Sprint")) speed /= sprintMod;
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
