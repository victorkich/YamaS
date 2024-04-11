using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class CollisionPub : MonoBehaviour
{
    public GameObject robot;

    private ROSConnection ros;
    public string topicName;
    private bool isCollided = false;

    // Start is called before the first frame update
    void Start()
    {
        topicName = "/" + robot.gameObject.tag + "/collision"; // Initialize topicName here
        ros = ROSConnection.GetOrCreateInstance();

        if (!ros.IsPublisherRegistered(topicName))
        {
            ros.RegisterPublisher<Int32Msg>(topicName);
        }
    }

    // Collision detector
    void OnTriggerEnter(Collider collision)
    {
        // Use the tag of the collided object to check if it's a wall or an object
        if (collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Object") 
        {
            // Set the collision flag
            isCollided = true;

            // Publish the collision message
            ros.Publish(topicName, new Int32Msg(1)); // Send a '1' to indicate collision
        }
    }

    void OnTriggerExit(Collider collision)
    {
        // Reset the collision flag when the robot exits a collision
        if (collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Object")
        {
            isCollided = false;

            // Publish the message to indicate that the robot is no longer in a collision
            ros.Publish(topicName, new Int32Msg(0)); // Send a '0' to indicate no collision
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If you want to publish the collision status continuously, you can do it here.
        // Otherwise, the collision status is only published once when entering or exiting a collision.
    }
}
