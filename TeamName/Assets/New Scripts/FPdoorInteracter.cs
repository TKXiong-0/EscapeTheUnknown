using UnityEngine;

public class FPdoorInteractor : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private float maxInteractDistance = 3f;
    [SerializeField] private LayerMask interactableLayers = -1;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private void Awake()
    {
        if(PlayerCamera == null)
        {
            PlayerCamera = GetComponent<Camera>();
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            InteractWithDoor();
        }
    }

    private void InteractWithDoor()
    {
        Ray ray = PlayerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if(Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance, interactableLayers))
        {
            DoorController door = hit.collider.GetComponentInParent<DoorController>();
            if (door != null)
            {
                door.ToggleDoor();
            }
        }
    }
}
