using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] int HP;

    [SerializeField] GameObject bullet;

    [SerializeField] Transform shootPos;
    [SerializeField] Transform GunPivot;
    [SerializeField] float shootRate;

    [SerializeField] int FOV;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int gunRotateSpeed;
    [SerializeField] int roamPauseTime;
    [SerializeField] int roamDistance;

    float shootTimer;
    float roamTimer;
    float angleToPlayer;
    float stoppingDistOrign;

    Color colorOrigin;

    bool playerInRange;

    Vector3 playerDir;
    Vector3 startingPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrigin = model.material.color;
        GameManager.instance.UpdateGameGoal(1);
        startingPos = transform.position;
        stoppingDistOrign = agent.stoppingDistance;
    }

    // Update is called once per frame
    void Update()
    {
        if ( agent.remainingDistance < 0.01f)
        {
            roamTimer += Time.deltaTime;
        }

        if (playerInRange && !canSeePlayer())
        {
            checkRoam();
        }
        else if (!playerInRange)
        {
            checkRoam();
        }
    }

    void checkRoam()
    {
        if (agent.remainingDistance < 0.01f && roamTimer >= roamPauseTime)
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
        NavMesh.SamplePosition(ranPos, out hit, roamDistance, 1);
        agent.SetDestination(hit.position);
    }

    bool canSeePlayer()
    {
        playerDir = GameManager.instance.player.transform.position - transform.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(transform.position, playerDir);  

        RaycastHit hit;
        if (Physics.Raycast(transform.position, playerDir, out hit))
        {
            if(hit.collider.CompareTag("Player") && angleToPlayer <= FOV)
            {
                agent.SetDestination(GameManager.instance.player.transform.position);

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }

                gunRotate();

                shootTimer += Time.deltaTime;
                if (shootTimer >= shootRate)
                {
                    shoot();
                }

                agent.stoppingDistance = stoppingDistOrign;
                return true;
            }
        }

        agent.stoppingDistance = 0;
        return false;
    }
    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime + faceTargetSpeed);
    }

    void gunRotate()
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

    public void takedamage(int amount)
    {

        HP -= amount;

        agent.SetDestination(GameManager.instance.player.transform.position);

        if (HP <= 0)
        {
            GameManager.instance.UpdateGameGoal(-1);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrigin;

    }

}
