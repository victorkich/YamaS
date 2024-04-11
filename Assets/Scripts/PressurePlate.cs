using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public GameObject door;
    public float doorOpenSpeed = 1.0f;
    private bool doorOpened = false;
    private Vector3 initialDoorPosition;
    private Vector3 targetDoorPosition;

    void Start()
    {
        initialDoorPosition = door.transform.position;
        targetDoorPosition = new Vector3(initialDoorPosition.x, initialDoorPosition.y + door.transform.localScale.y, initialDoorPosition.z);
    }

    void Update()
    {
        if (doorOpened)
        {
            door.transform.position = Vector3.Lerp(door.transform.position, targetDoorPosition, doorOpenSpeed * Time.deltaTime);
        }
        else
        {
            door.transform.position = Vector3.Lerp(door.transform.position, initialDoorPosition, doorOpenSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("robot1") || other.gameObject.CompareTag("robot2") || other.gameObject.CompareTag("robot3") || other.gameObject.CompareTag("robot4"))
        {
            doorOpened = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("robot1") || other.gameObject.CompareTag("robot2") || other.gameObject.CompareTag("robot3") || other.gameObject.CompareTag("robot4"))
        {
            doorOpened = false;
        }
    }
}