using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;

    [SerializeField] GameObject Knife;
    [SerializeField] float swingrate;
    [SerializeField] Transform swingPos;
    [SerializeField] Transform knifePivot;

    [SerializeField] int FOV;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int roamPauseTime;
    [SerializeField] int roamDistance;

    float swingtimer;
    float roamTimer;
    float angleToPlayer;
    float stoppingDistOrig;

    bool playerInRange;

    Vector3 playerDir;
    Vector3 startingPos;
    void Start()
    {
        
        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance < 0.01f)
            roamTimer += Time.deltaTime;

        if (playerInRange && !canSeePlayer())
        {
           checkRoam();
        }
        else if(!playerInRange)
        {
            checkRoam();
        }
    }

    void checkRoam()
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
        NavMesh.SamplePosition(ranPos, out hit, roamDistance, 1);
        agent.SetDestination(hit.position);
    }

    bool canSeePlayer()
    {
        //playerDir = gameManager.instance.player.transform.position - transform.position;

        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(transform.position, playerDir);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, playerDir, out hit))
        {
            if(hit.collider.CompareTag("Player") && angleToPlayer <= FOV)
            {
                //agent.SetDestination(gamemanager.instance.player.transform.position);

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }

                swingtimer += Time.deltaTime;

                if (swingtimer >= swingrate)
                {
                    swing();
                }

                agent.stoppingDistance = stoppingDistOrig;
                return true;

            }
        }
        agent.stoppingDistance = 0;
        return false;
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
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

    void swing()
    {
        swingtimer = 0;
        Instantiate(Knife, swingPos.position, knifePivot.rotation);
    }
}
