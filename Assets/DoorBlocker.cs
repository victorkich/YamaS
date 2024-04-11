using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class DoorBlocker : MonoBehaviour
{
    private bool isRotating = false;
    private float rotationAmount = -90f; // Amount to rotate
    private ROSConnection ros;
    private GameObject door; // Reference to the door child object

    void Start()
    {
        // Get reference to the door child object
        door = transform.Find("door blocker").gameObject;

        ros = ROSConnection.GetOrCreateInstance();

        // Subscribe to ROS topics
        ros.Subscribe<StringMsg>("unblock_door", UnblockDoorCallback);
        ros.Subscribe<StringMsg>("block_door", BlockDoorCallback);
    }

    void UnblockDoorCallback(StringMsg doorColorMsg)
    {
        string receivedColor = doorColorMsg.data.ToLower();
        string doorTag = door.tag.ToLower(); // Assuming the door's tag is set to the color name

        Debug.Log($"Trying to unblock: received color = {receivedColor}, door tag = {doorTag}");

        if (receivedColor == doorTag && !isRotating)
        {
            StartCoroutine(RotateBlock(rotationAmount)); // Rotate to unblock
        }
    }

    void BlockDoorCallback(StringMsg doorColorMsg)
    {
        string receivedColor = doorColorMsg.data.ToLower();
        string doorTag = door.tag.ToLower(); // Assuming the door's tag is set to the color name

        Debug.Log($"Trying to block: received color = {receivedColor}, door tag = {doorTag}");

        if (receivedColor == doorTag && !isRotating)
        {
            StartCoroutine(RotateBlock(-rotationAmount)); // Rotate back to original position
        }
    }

    System.Collections.IEnumerator RotateBlock(float rotationAngle)
    {
        isRotating = true;
        Quaternion fromRotation = transform.rotation;
        Quaternion toRotation = transform.rotation * Quaternion.Euler(0, 0, rotationAngle); // Rotate relative to current rotation

        float elapsedTime = 0f;
        float duration = 1f; // Rotate over 1 second

        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(fromRotation, toRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = toRotation;
        isRotating = false;
    }
}
