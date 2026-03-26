using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections;

public class Damage : MonoBehaviour
{
    enum damagetype {bullet, stationary, DOT }
    [SerializeField] damagetype type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [SerializeField] ParticleSystem hitEffect;

    bool isDamage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(type == damagetype.bullet)
        { 
            rb.linearVelocity = transform.forward * speed;
            Destroy(gameObject, destroyTime);

        }

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        return;
            
        

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg!=null && type != damagetype.DOT)
        {
            dmg.takeDamage(damageAmount);
        }
        if (type == damagetype.bullet)
        {
            if(hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
             }
            Destroy(gameObject);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
        return;
            
        

        IDamage dmg = other.GetComponent<IDamage>();
        if (dmg != null && type != damagetype.DOT && !isDamage)
        {
            StartCoroutine(damageOther(dmg));
        }
    }

    IEnumerator damageOther (IDamage d)
    {
        isDamage = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        isDamage = false;
    }

  
}
