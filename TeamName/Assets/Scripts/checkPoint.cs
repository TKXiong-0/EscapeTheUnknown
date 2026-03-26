using UnityEngine;
using System.Collections;

public class checkPoint : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && GameManager.instance.playerSpawnPos.transform.position != transform.position)
        {
            GameManager.instance.playerSpawnPos.transform.position = transform.position;
            StartCoroutine(showPopup());
        }
    }

    IEnumerator showPopup()
    {
        GameManager.instance.checkpointPopup.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        GameManager.instance.checkpointPopup.SetActive(false);
    }
}
