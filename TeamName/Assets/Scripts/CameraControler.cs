using UnityEngine;

public class CameraControler : MonoBehaviour
{

    [SerializeField] int sens;
    [SerializeField] int lockVerMin, lockVertMax;
    [SerializeField] bool invertY;
    [SerializeField] Transform player;

    float camRotX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sens * Time.deltaTime;

        if(invertY)
        {
            camRotX += mouseY;

        }
        else
        {
            camRotX -= mouseY;

        }

        camRotX = Mathf.Clamp(camRotX, lockVerMin, lockVertMax);
        transform.localRotation = Quaternion.Euler(camRotX, 0, 0);
        
        player.transform.Rotate(Vector3.up * mouseX);
    }
}
