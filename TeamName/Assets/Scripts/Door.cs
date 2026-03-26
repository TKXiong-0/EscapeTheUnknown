using UnityEngine;
using UnityEngine.InputSystem;



public class Door : MonoBehaviour
{
    [SerializeField] GameObject model;
    [SerializeField] GameObject button;
    [SerializeField] bool canOpen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Interact") && canOpen)
        {
            model.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            button.SetActive(true);
        }
    }



    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            model.SetActive(true);
            button.SetActive(false);
        }
    }

    public void keyPickup()
    {
        canOpen = true;
    }
}