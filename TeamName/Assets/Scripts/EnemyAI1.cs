using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemyAI1 : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] int HP;

    [SerializeField] GameObject bullet;
    
    [SerializeField] Transform shootPos;
    [SerializeField] Transform GunPivot;
    [SerializeField] float shootRate;

    [SerializeField] float FOV;
    [SerializeField] int FaceTargetSpeed;
    [SerializeField] int gunRotateSpeed;
    [SerializeField] int roamPauseTime;
    [SerializeField] int roamDistance;


    float shootTimer;
    float roamTimer;
    float AngelToPlayer;
    float stoppingDistOrig;

    Color colorOrigin;

    bool playerInRange;

    Vector3 playerDir;
    Vector3 startingPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrigin = model.material.color;
        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;
    }

    // Update is called once per frame
    void Update()
    {

        if (agent.remainingDistance < 0.01f)
            roamTimer += Time.deltaTime;

        if (playerInRange && !CanSeePlayer())
        {
            CheckRoam();

        } else if (!playerInRange)
        {
            CheckRoam();
        }
    }

    void CheckRoam()
    {
        if(agent.remainingDistance < 0.01f && roamTimer >= roamPauseTime)
        {
            roam();         
        }
    }

    void roam()
    {
        
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDistance;
        ranPos += startingPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, roamDistance,1);
        agent.SetDestination(hit.position);

    }


    bool CanSeePlayer()
    {
        playerDir = GameManager.instance.player.transform.position - transform.position;
        AngelToPlayer = Vector3.Angle(playerDir,transform.forward);

        Debug.DrawRay(transform.position, playerDir);

        RaycastHit hit;
        if(Physics.Raycast(transform.position, playerDir, out hit))
        {
            if(hit.collider.CompareTag("Player") && AngelToPlayer <= FOV)
            {

                agent.SetDestination(GameManager.instance.player.transform.position);

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    GunRotate();

                    FaceTarget();
                }



                shootTimer += Time.deltaTime;
                if (shootTimer >= shootRate)
                {
                    shoot();
                }
                agent.stoppingDistance = stoppingDistOrig;
                return true;
            }
        }
        agent.stoppingDistance = 0;
        return false;

    }


    void FaceTarget()
    {

        Quaternion rot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * FaceTargetSpeed);
    }

    void GunRotate()
    {

        Quaternion rot = Quaternion.LookRotation(playerDir);
        GunPivot.rotation = Quaternion.Lerp(GunPivot.rotation, rot, Time.deltaTime * gunRotateSpeed);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;

    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            agent.stoppingDistance = 0;
        }
    }


    void shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPos.position, GunPivot.rotation);
    }

    public void takeDamage(int amount)
    {

        
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrigin;

    }

}
