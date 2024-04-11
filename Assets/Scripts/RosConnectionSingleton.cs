using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Sensor;
using System.Runtime.Serialization;


public class RosConnectionSingleton : MonoBehaviour
{
    private static ROSConnection _instance;

    // Create a static instance of RosConnector
    public static ROSConnection Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ROSConnection>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    // This is called when the script instance is being loaded.
    void Awake()
    {
        // Ensure that there's only one instance of RosConnector
        if (_instance == null)
        {
            _instance = FindObjectOfType<ROSConnection>();
            DontDestroyOnLoad(this.gameObject);
        }
        else if (_instance != this)
        {
            Destroy(this.gameObject);
        }
    }
}