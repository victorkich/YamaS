using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Unity.Robotics.ROSTCPConnector; // Required for ROS communication
using RosMessageTypes.Std; // For using the standard String message type

public class SceneLoader : MonoBehaviour
{
    // Reference to the GenerateMAEnvironment script
    public GenerateMAEnvironment environmentGenerator;

    // Name of the scene to load
    public string sceneName;
    public MAData maData; // Reference to the MAData ScriptableObject

    private ROSConnection ros;
    private string topicName = "areas_information";

    void Start()
    {
        // Initialize ROSConnection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>(topicName);

        // Execute the LogDataLists function before loading the scene
        environmentGenerator.LoadAndLogDataLists();
        string areaInformationJson = PrintAreaInformation();

        // Load the scene asynchronously
        // SceneManager.LoadSceneAsync(sceneName);
        
        // Create a ROS String message
        StringMsg message = new StringMsg(areaInformationJson);

        // Publish the message
        ros.Publish(topicName, message);

        Debug.Log("Area information published successfully: " + areaInformationJson);
    }

    // Call this method to get formatted information for all areas and publish it
    public string PrintAreaInformation()
    {
        string areaInfoConcat = "";
        foreach (var area in maData.areaDataList)
        {
            string areaInfo = FormatAreaInformation(area.areaNumber);
            Debug.Log(areaInfo);
            areaInfoConcat = $"{areaInfoConcat}{areaInfo};";
        }

        // Convert the list to JSON string
        return areaInfoConcat;

    }

    private string FormatAreaInformation(int areaNumber)
    {
        var objectsInArea = maData.objectDataList.Where(obj => obj.areaNumber == areaNumber);
        var doorsInArea = maData.doorDataList.Where(door => door.areaNumber == areaNumber || door.areaNumber + 1 == areaNumber);

        string objectInfo = string.Join(", ", objectsInArea.Select(obj => $"1 {obj.color} {obj.type}"));
        string doorInfo = string.Join(", ", doorsInArea.Select(door => $"{FormatDoorInformation(door, areaNumber)}"));

        var area = maData.areaDataList.FirstOrDefault(a => a.areaNumber == areaNumber);
        string obstacleInfo = $"{area.obstaclesDropdownValue} obstacles";

        return $"Area {areaNumber} has {objectInfo}, {obstacleInfo}, {doorInfo}";
    }

    private string FormatDoorInformation(DoorData door, int areaNumber)
    {
        if (door.type2 == "None")
        {
            return $"1 {door.color} {door.type}";
        }
        return $"1 {door.color} {door.type}, 1 {door.color2} {door.type2}";
    }
}
