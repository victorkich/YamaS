using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std; // Import ROS standard message types

public class DoorControlPublisher : MonoBehaviour
{
    private ROSConnection ros;
    public string unblockDoorTopic = "unblock_door";
    public string blockDoorTopic = "block_door";

    void Start()
    {
        Debug.Log($" to unblock {unblockDoorTopic} door");
        Debug.Log($" to block {blockDoorTopic} door");
        // Initialize ROSConnection
        ros = ROSConnection.GetOrCreateInstance();
        
        // Register publishers for both topics
        ros.RegisterPublisher<StringMsg>(unblockDoorTopic);
        ros.RegisterPublisher<StringMsg>(blockDoorTopic);
    }

    void OnTriggerEnter(Collider other)
    {
        string pressurePlateColor = gameObject.tag;
        StringMsg pressurePlateColorMsg = new StringMsg(pressurePlateColor);
        ros.Publish(unblockDoorTopic, pressurePlateColorMsg);
        Debug.Log($"Publishing to unblock {pressurePlateColor} door");
    }

    void OnTriggerExit(Collider other)
    {
        string pressurePlateColor = gameObject.tag;
        StringMsg pressurePlateColorMsg = new StringMsg(pressurePlateColor);
        ros.Publish(blockDoorTopic, pressurePlateColorMsg);
        Debug.Log($"Publishing to block {pressurePlateColor} door");
    }
}
