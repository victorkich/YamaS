using UnityEngine;
using Unity.Robotics.ROSTCPConnector; // ROS TCP Connector namespace
using RosMessageTypes.Std; // Standard ROS Message Types

public class ClawController : MonoBehaviour
{
    public Transform leftPart;
    public Transform rightPart;
    public float speed = 10f; // Adjust the angle every frame

    private float anglel = 90f; // Current angle for the left part
    private float angler = 90f; // Current angle for the right part

    private const float OpenAnglel = 30f;
    private const float OpenAngler = 150f;
    private const float ClosedAnglel = 90f;
    private const float ClosedAngler = 90f;

    private bool isOpening = false;
    private bool isClosing = false;

    private ROSConnection ros;

    public GameObject robot;

    // Name of the ROS topic to subscribe
    [SerializeField]
    public string topicName = "Robot/claw";

    void Start()
    {
        // Get ROS Connection instance
        ros = ROSConnection.GetOrCreateInstance();
        string fullTopicName = "/" + robot.gameObject.tag + topicName;
        ros.Subscribe<BoolMsg>(fullTopicName, ClawControlCallback);
    }

    void ClawControlCallback(BoolMsg message)
    {
        // Open or close claw based on the ROS message
        if (message.data)
        {
            isOpening = true;
            isClosing = false;
        }
        else
        {
            isClosing = true;
            isOpening = false;
        }
    }

    void Update()
    {
        if (isOpening && (anglel > OpenAnglel || angler < OpenAngler))
        {
            anglel -= speed * Time.deltaTime;
            angler += speed * Time.deltaTime;

            leftPart.localEulerAngles = new Vector3(90, angler, 0);
            rightPart.localEulerAngles = new Vector3(90, anglel, 0);

            if (anglel <= OpenAnglel && angler >= OpenAngler)
            {
                isOpening = false;
            }
        }

        if (isClosing && (anglel < ClosedAnglel || angler > ClosedAngler))
        {
            anglel += speed * Time.deltaTime;
            angler -= speed * Time.deltaTime;

            leftPart.localEulerAngles = new Vector3(90, angler, 0);
            rightPart.localEulerAngles = new Vector3(90, anglel, 0);

            if (anglel >= ClosedAnglel && angler <= ClosedAngler)
            {
                isClosing = false;
            }
        }
    }
}
