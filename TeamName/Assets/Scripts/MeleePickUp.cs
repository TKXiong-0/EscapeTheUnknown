using UnityEngine;

public class MeleePickUp : MonoBehaviour
{
    [SerializeField] MeleeStats melee;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
            player = other.GetComponentInParent<PlayerController>();

        if (player == null || melee == null)
            return;

        player.getMeleeStats(melee);
        Destroy(gameObject);
    }
}