
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Random = UnityEngine.Random;



public class PlayerController : MonoBehaviour, IDamage, IPickup
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
    [SerializeField] List<gunStats> gunList = new List<gunStats>(); 
    [SerializeField] GameObject gunModel;

    [Header("---- Audio ----")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audJump;
    [SerializeField] float audJumpVol;
    [SerializeField] AudioClip[] audHurt;
    [SerializeField] float audHurtVol;
    [SerializeField] AudioClip[] audStep;
    [SerializeField] float audStepVol;


    int jumpCount;
    int HPorig;
    int gunListPos;

    float shootTimer;

    bool isplayingStep;
    bool isSprinting;

    Vector3 MoveDir;
    Vector3 playerVel;

    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPorig = HP;
        
        SpawnPlayer();

    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.isPaused)
        movement();
        Sprint();

    }

    IEnumerator playStep()
    {
        isplayingStep = true;
        aud.PlayOneShot(audStep[Random.Range(0, audStep.Length)], audStepVol);

        if(isSprinting)
        {
            yield return new WaitForSeconds(0.3f);

        } else
        {
            yield return new WaitForSeconds(0.5f);
        }

        isplayingStep = false;
    }


    public void SpawnPlayer()
    {
        controller.transform.position = GameManager.instance.playerSpawnPos.transform.position;
        Physics.SyncTransforms();
        HP = HPorig;
        updatePlayerUI();
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

        if(Input.GetButton("Fire1") && gunList.Count > 0 && gunList[gunListPos].ammoCur > 0 && shootTimer >= gunList[gunListPos].shootRate)
        {
            shoot();
        }

        SelectGun();

        if(MoveDir.normalized.magnitude > 0.3 && !isplayingStep)
        StartCoroutine(playStep());
    }

    void jump() 
    {
        if(Input.GetButtonDown("Jump") && jumpCount < jumpTimeMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
            aud.PlayOneShot(audJump[Random.Range(0,audJump.Length)], audJumpVol);
        }
    }


    void Sprint() 
    { 
        if(Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            isSprinting = true;

        }
        else if(Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
            isSprinting = false;
        }
    }


    void shoot()
    {
        shootTimer = 0;

        gunList[gunListPos].ammoCur--;
        aud.PlayOneShot(gunList[gunListPos].shootSound[Random.Range(0, gunList[gunListPos].shootSound.Length)], gunList[gunListPos].shootSoundVol);

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, gunList[gunListPos].shootDistance,~ignoreLayer))
        {
            Instantiate(gunList[gunListPos].hitEffect, hit.point, Quaternion.identity);

            Debug.Log(hit.collider.name);
            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(gunList[gunListPos].shootDamage);
            }
        }
        

    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();
        aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);
        StartCoroutine(flashDamage());

        if (HP<=0)
        {
            GameManager.instance.youLose();
        }
    }

    IEnumerator flashDamage()
    {
        GameManager.instance.damagePlayerFlash.SetActive(true);
        yield return new WaitForSeconds(.05f);
        GameManager.instance.damagePlayerFlash.SetActive(false);
    }


    public void updatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)HP / HPorig;
    }

    public void getGunStats(gunStats gun)
    {
        gunList.Add(gun);
        gunListPos = gunList.Count - 1;

        ChangeGun();

    }

    void ChangeGun()
    {
       

        gunModel.GetComponent<MeshFilter>().sharedMesh = gunList[gunListPos].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListPos].gunModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

    void SelectGun()
    {
        if(Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count - 1) 
        {
            gunListPos++;
            ChangeGun();
        }else if(Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPos > 0)
        {
            gunListPos--;
            ChangeGun();
        }
    }
}
