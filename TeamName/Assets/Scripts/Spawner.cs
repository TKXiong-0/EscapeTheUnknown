using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] int spawnAmount;
    [SerializeField] int spawnRate;
    [SerializeField] int spawnDist;

    int SpawnCount;
    float spawnTimer;

    bool startSpawning;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.instance.UpdateGameGoal(1);
    }

    // Update is called once per frame
    void Update()
    {
        if(startSpawning)
        {
            spawnTimer += Time.deltaTime;
            
            if(SpawnCount < spawnAmount && spawnTimer >= spawnRate)
            {
                //hey start spawnig
                spawn();

            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            startSpawning = true;
        }
    }

    void spawn()
    {
        spawnTimer = 0;
        SpawnCount++;

        Vector3 ranPos = Random.insideUnitSphere * spawnDist;
        ranPos += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, spawnDist, 1);
        Instantiate(objectToSpawn, hit.position, Quaternion.Euler(0, Random.Range(0, 360), 0));

    }


}
