using UnityEngine;

public class limitCamera : MonoBehaviour
{
    private void LateUpdate()
    {        
        transform.position = new Vector3(GameManager.instance.player.transform.position.x, 40 , GameManager.instance.player.transform.position.z);    
    }
}
