using UnityEngine;

public class KeyBehavior : MonoBehaviour
{
    [SerializeField] Door _Door;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player")) && GameManager.instance.playerSpawnPos.transform.position != transform.position)
        {
            _Door.keyPickup();
            Destroy(gameObject);
        }
        
    }



}
