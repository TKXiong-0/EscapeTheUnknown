using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class PlayerController : MonoBehaviour, IDamage
{
    [Header("---- Components ----")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("---- Stats ----")]
    [Range(1,10)][SerializeField] int HP;
    [Range(1,10)][SerializeField] int speed;
    [Range(2,6)][SerializeField] int sprintMod;
    [Range(5,25)][SerializeField] int jumpSpeed;
    [Range(1,4)][SerializeField] int jumpTimeMax;
    [Range(15,50)][SerializeField] int Gravity;

    [Header("----Guns----")]
    [SerializeField] int shootDamage;
    [SerializeField] int shootDis;
    [SerializeField] float shootRate;

    int jumpCount;
    int HPorig;

    float shootTimer;

    Vector3 MoveDir;
    Vector3 playerVel;

    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPorig = HP;
        updatePlayerUI();

    }

    // Update is called once per frame
    void Update()
    {
        movement();
        Sprint();
    }

    void movement()
    {
        //Don't forget this, you'll want to Cry
        shootTimer += Time.deltaTime;

        if(controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = 0;
        }


        MoveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;

        controller.Move(MoveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        playerVel.y -= Gravity * Time.deltaTime;

        if(Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }    

    }

    void jump() 
    {
        if(Input.GetButtonDown("Jump") && jumpCount < jumpTimeMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }


    void Sprint() 
    { 
        if(Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;

        }
        else if(Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }


    void shoot()
    {
        shootTimer = 0;
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDis,~ignoreLayer))
        {
            Debug.Log(hit.collider.name);
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }
        

    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();
        if(HP<=0)
        {
            GameManager.instance.youLose();
        }
    }


    public void updatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)HP / HPorig;
    }



}
