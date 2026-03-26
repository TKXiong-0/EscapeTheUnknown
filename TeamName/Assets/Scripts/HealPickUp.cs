using UnityEngine;

public class HealPickUp : MonoBehaviour
{
    [SerializeField] HealStats heal;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
            player = other.GetComponentInParent<PlayerController>();

        if (player == null || heal == null)
            return;

        player.getHealStats(heal);
        Destroy(gameObject);
    }
}