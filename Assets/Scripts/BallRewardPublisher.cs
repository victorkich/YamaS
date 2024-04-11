using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;

public class BallRewardPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "global_reward";
    private bool isCollided = false;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        string fullTopicName = topicName;

        if (!ros.IsPublisherRegistered(fullTopicName))
        {
            ros.RegisterPublisher<Int32Msg>(fullTopicName);
        }
    }

    void Update()
    {
        int data = isCollided ? 1 : 0;
        ros.Publish(topicName, new Int32Msg(data));
        // isCollided = false;
    }

    void OnTriggerEnter(Collider collision)
    {
        // Check if the collided object is the ground plate
        if(collision.gameObject.tag == "final_target") // Change this to your ground plate's name
        {
            // Set the flag
            isCollided = true;
        }
    }

    void OnTriggerExit(Collider collision)
    {
        // Check if the collided object is the ground plate
        if(collision.gameObject.tag == "final_target") // Change this to your ground plate's name
        {
            // Set the flag
            isCollided = false;
        }
    }
}