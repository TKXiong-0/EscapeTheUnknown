using UnityEngine;

public class gunPickup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] gunStats gun;

    private void OnTriggerEnter(Collider other)
    {
        IPickup pik = other.GetComponent<IPickup>();
        
        if(pik != null)
        {
            gun.ammoCur = gun.ammoMax;
            pik.getGunStats(gun);
            Destroy(gameObject);
        }
    }
}
