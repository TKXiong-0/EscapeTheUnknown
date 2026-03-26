using UnityEngine;
using System.Collections;

public class PlayerController2 : MonoBehaviour, IDamage
{
    [Header("--------------- Components ---------------")]
    [SerializeField] CharacterController controller;
    [SerializeField] Animator characterAnimator;
    [SerializeField] Transform playerCamera;
    [SerializeField] LayerMask ignoreLayer;

    [Header("--------------- Player Stats ---------------")]
    [Range(1, 20)][SerializeField] int HP = 10;
    [Range(1, 20)][SerializeField] int speed = 5;
    [Range(1, 10)][SerializeField] int sprintMod = 2;
    [Range(1, 20)][SerializeField] int jumpSpeed = 10;
    [Range(1, 5)][SerializeField] int jumpTimeMax = 1;
    [Range(15, 60)][SerializeField] int Gravity = 20;

    [Header("--------------- Camera Settings ---------------")]
    [SerializeField] float cameraHeight = 1.4f;
    [SerializeField] float cameraNormalZ = 0.0f;
    [SerializeField] float cameraSprintZ = 0.4f;
    [SerializeField] float cameraAttackZ = 0.7f;
    [SerializeField] float cameraLerpSpeed = 5.0f;
    [SerializeField] float attackCameraLerpSpeed = 2.5f;
    [SerializeField] float attackCameraDuration = 0.45f;
    [SerializeField] Transform cameraPivot;
    [SerializeField] Transform mainCam;
    [SerializeField] float cameraCollisionRadius = 0.2f;
    [SerializeField] float cameraWallPadding = 0.05f;
    [SerializeField] LayerMask cameraCollisionMask;

    [Header("--------------- Melee Settings ---------------")]
    [SerializeField] int meleeDamage = 3;
    [SerializeField] float meleeRange = 3.0f;
    [SerializeField] float attackRate = 0.6f;

    int jumpCount;
    int HPorig;
    bool isAttacking;
    bool isAttackLocked;
    Coroutine attackCameraRoutine;
    float attackTimer;

    Vector3 moveDir;
    Vector3 playerVel;

    void Start()
    {
        HPorig = HP;

        if (cameraPivot != null)
        {
            Vector3 pivotPos = cameraPivot.localPosition;
            cameraPivot.localPosition = new Vector3(pivotPos.x, cameraHeight, pivotPos.z);
        }

        if (GameManager.instance != null && GameManager.instance.playerHPBar != null)
        {
            GameManager.instance.playerHPBar.fillAmount = 1;
        }
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isPaused)
            return;

        RotatePlayerToCamera();
        Movement();
        HandleMelee();
    }

    void Movement()
    {
        attackTimer += Time.deltaTime;

        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = -2f;
        }

        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");
        bool isSprinting = Input.GetButton("Sprint");

        float currentSpeed = speed;
        if (isSprinting && vInput > 0)
        {
            currentSpeed *= sprintMod;
        }

        moveDir = (hInput * transform.right) + (vInput * transform.forward);
        controller.Move(moveDir * currentSpeed * Time.deltaTime);

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

        UpdateCamera(vInput, isSprinting);
        UpdateAnimations(vInput, hInput, isSprinting);
    }

    void RotatePlayerToCamera()
    {
        if (cameraPivot == null) return;

        Vector3 camEuler = cameraPivot.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, camEuler.y, 0f);
    }

    void UpdateCamera(float vInput, bool isSprinting)
    {
        if (mainCam == null || cameraPivot == null) return;

        float targetZ = cameraNormalZ;
        float currentLerpSpeed = cameraLerpSpeed;

        if (isAttacking)
        {
            targetZ = cameraAttackZ;
            currentLerpSpeed = attackCameraLerpSpeed;
        }
        else if (vInput > 0f && isSprinting)
        {
            targetZ = cameraSprintZ;
        }

        Vector3 desiredLocalPos = new Vector3(0f, 0f, targetZ);

        Vector3 pivotWorldPos = cameraPivot.position;
        Vector3 desiredWorldPos = cameraPivot.TransformPoint(desiredLocalPos);

        Vector3 dir = desiredWorldPos - pivotWorldPos;
        float distance = dir.magnitude;

        if (distance > 0.01f)
        {
            dir.Normalize();

            RaycastHit hit;
            if (Physics.SphereCast(
                pivotWorldPos,
                cameraCollisionRadius,
                dir,
                out hit,
                distance,
                cameraCollisionMask,
                QueryTriggerInteraction.Ignore))
            {
                float safeDistance = Mathf.Max(hit.distance - cameraWallPadding, 0f);
                desiredWorldPos = pivotWorldPos + dir * safeDistance;
            }
        }

        Vector3 finalLocalPos = cameraPivot.InverseTransformPoint(desiredWorldPos);

        mainCam.localPosition = Vector3.Lerp(
            mainCam.localPosition,
            finalLocalPos,
            Time.deltaTime * currentLerpSpeed
        );
    }

    void UpdateAnimations(float vInput, float hInput, bool isSprinting)
    {
        if (characterAnimator == null) return;

        float moveAmount = Mathf.Abs(vInput) + Mathf.Abs(hInput);

        if (moveAmount > 0.1f)
        {
            if (vInput > 0)
                characterAnimator.SetFloat("Speed", isSprinting ? 1.0f : 0.5f, 0.1f, Time.deltaTime);
            else
                characterAnimator.SetFloat("Speed", 0.5f, 0.1f, Time.deltaTime);
        }
        else
        {
            characterAnimator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }
    }

    void HandleMelee()
    {
        if (Input.GetButtonDown("Fire1") && attackTimer >= attackRate && !isAttackLocked)
        {
            MeleeAttack();
        }
    }

    void MeleeAttack()
    {
        attackTimer = 0f;
        isAttackLocked = true;

        if (attackCameraRoutine != null)
        {
            StopCoroutine(attackCameraRoutine);
        }
        attackCameraRoutine = StartCoroutine(AttackCameraOffset());

        if (characterAnimator != null)
        {
            characterAnimator.ResetTrigger("MeleeTrigger");
            characterAnimator.SetTrigger("MeleeTrigger");
        }

        RaycastHit hit;
        if (mainCam != null && Physics.Raycast(mainCam.position, mainCam.forward, out hit, meleeRange, ~ignoreLayer))
        {
            IDamage dmg = hit.collider.GetComponentInParent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(meleeDamage);
            }
        }
    }

    IEnumerator AttackCameraOffset()
    {
        isAttacking = true;

        yield return new WaitForSeconds(attackCameraDuration);

        isAttacking = false;
        isAttackLocked = false;
        attackCameraRoutine = null;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();

        if (HP <= 0)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.youLose();
            }
        }
    }

    public void updatePlayerUI()
    {
        if (GameManager.instance != null && GameManager.instance.playerHPBar != null)
        {
            GameManager.instance.playerHPBar.fillAmount = (float)HP / HPorig;
        }
    }

    public void StartAttackCameraPush()
    {
        isAttacking = true;
    }

    public void EndAttackCameraPush()
    {
        isAttacking = false;
    }

    public void DealMeleeDamage()
    {
        RaycastHit hit;
        if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, meleeRange, ~ignoreLayer))
        {
            IDamage dmg = hit.collider.GetComponentInParent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(meleeDamage);
            }
        }
    }
}
