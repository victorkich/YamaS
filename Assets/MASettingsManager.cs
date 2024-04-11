using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


// Class to represent the data inside the Area Selector Prefab
[System.Serializable]
public class AreaData
{
    public int areaNumber;
    public string robotsDropdownValue;
    public string obstaclesDropdownValue;
}

// Class to represent the data inside the Door Selector Prefab
[System.Serializable]
public class DoorData
{
    public string color; // Change the data type to string
    public string type;
    public string color2;
    public string type2;
    public int areaNumber; // Reference to the area this door belongs to
}

// Class to represent the data inside the Object Selector Prefab
[System.Serializable]
public class ObjectData
{
    public string color; // Change the data type to string
    public string type;
    public int areaNumber; // Reference to the area this object belongs to
}


// Class to hold object selectors along with their area number
public class ObjectSelectorData
{
    public List<GameObject> objectSelectors;
    public int areaNumber;

    public ObjectSelectorData(int areaNumber)
    {
        this.areaNumber = areaNumber;
        objectSelectors = new List<GameObject>();
    }
}

public class MASettingsManager : MonoBehaviour
{
    // Reference to the MAData ScriptableObject instance
    public MAData maData;

    // Object Selector Variables
    public GameObject objectSelectorPrefab;
    public Transform objectSelectorsContainer;
    private int objectCounter = 0;
    private List<GameObject> objectSelectors = new List<GameObject>();

    // Area Variables
    public GameObject areaPrefab;
    public Transform areasContainer;
    private int areaCounter = 0;
    private List<GameObject> areas = new List<GameObject>();
    private Dictionary<GameObject, ObjectSelectorData> areaObjectsMap = new Dictionary<GameObject, ObjectSelectorData>();

    public GameObject doorPrefab;
    private List<GameObject> doors = new List<GameObject>();

    // // Public properties to access the data lists
    // public List<AreaData> AreaDataList { get; private set; }
    // public List<DoorData> DoorDataList { get; private set; }
    // public List<ObjectData> ObjectDataList { get; private set; }

    // Object Selector Functions
    public void AddObjectSelector()
    {
        GameObject newSelector = Instantiate(objectSelectorPrefab, objectSelectorsContainer);
        objectSelectors.Add(newSelector);

        TMP_Text objectNameTextMeshPro = newSelector.transform.Find("ObjectName").GetComponent<TMP_Text>();

        if (objectNameTextMeshPro != null)
        {
            objectCounter++;
            objectNameTextMeshPro.text = "Object " + objectCounter.ToString() + ":";
        }
        else
        {
            Debug.LogError("TextMeshPro component 'ObjectName' not found in the instantiated prefab.");
        }

        // Store the object selector under the current area with its area number
        if (areas.Count > 0)
        {
            GameObject currentArea = areas[areas.Count - 1];
            int areaNumber = areaCounter - 1; // Calculate the area number
            if (!areaObjectsMap.ContainsKey(currentArea))
            {
                areaObjectsMap[currentArea] = new ObjectSelectorData(areaNumber);
            }
            areaObjectsMap[currentArea].objectSelectors.Add(newSelector);
        }
    }

    public void DeleteLastObjectSelector()
    {
        if (objectSelectors.Count > 0)
        {
            GameObject lastObjectSelector = objectSelectors[objectSelectors.Count - 1];
            objectSelectors.RemoveAt(objectSelectors.Count - 1);
            Destroy(lastObjectSelector);
            objectCounter--;

            // Remove the object selector from the areaObjectsMap
            foreach (var entry in areaObjectsMap)
            {
                if (entry.Value.objectSelectors.Contains(lastObjectSelector))
                {
                    entry.Value.objectSelectors.Remove(lastObjectSelector);
                    break;
                }
            }
        }
        else
        {
            Debug.LogWarning("No object selector to delete.");
        }
    }

    public void AddArea()
    {
        // Check if there are existing areas
        bool hasExistingAreas = areas.Count > 0;

        // Instantiate the door prefab first if there are existing areas
        if (hasExistingAreas)
        {
            GameObject newDoor = Instantiate(doorPrefab, areasContainer);
            doors.Add(newDoor);

            TMP_Text doorNameTextMeshPro = newDoor.transform.Find("DoorsName").GetComponent<TMP_Text>();

            if (doorNameTextMeshPro != null)
            {
                doorNameTextMeshPro.text = "Connecting Area " + areaCounter.ToString() + " and Area " + (areaCounter + 1).ToString() + ":";
            }
            else
            {
                Debug.LogError("TextMeshPro component 'DoorName' not found in the instantiated door prefab.");
            }
        }

        // Instantiate the area prefab
        GameObject newArea = Instantiate(areaPrefab, areasContainer);
        areas.Add(newArea);

        TMP_Text areaNameTextMeshPro = newArea.transform.Find("AreaText").GetComponent<TMP_Text>();

        if (areaNameTextMeshPro != null)
        {
            areaCounter++;
            areaNameTextMeshPro.text = "Area " + areaCounter.ToString() + ":";
        }
        else
        {
            Debug.LogError("TextMeshPro component 'AreaName' not found in the instantiated area.");
        }

        // Create a new ObjectSelectorData instance to store the list of object selectors for this area
        ObjectSelectorData selectorData = new ObjectSelectorData(areaCounter);
        areaObjectsMap[newArea] = selectorData;

        // Update object counter if there are existing areas
        if (hasExistingAreas)
        {
            UpdateObjectCounter();
        }
    }


public void DeleteLastArea()
{
    if (areas.Count > 1)
    {
        GameObject lastArea = areas[areas.Count - 1];
        GameObject lastDoor = null;

        if (doors.Count > 0)
        {
            lastDoor = doors[doors.Count - 1];
            doors.RemoveAt(doors.Count - 1);
            Destroy(lastDoor);
        }

        areas.RemoveAt(areas.Count - 1);
        Destroy(lastArea);
        areaCounter--;

        // Destroy objects associated with the deleted area and update object counter
        if (areaObjectsMap.ContainsKey(lastArea))
        {
            ObjectSelectorData selectorData = areaObjectsMap[lastArea];
            foreach (var obj in selectorData.objectSelectors)
            {
                Destroy(obj);
                objectSelectors.Remove(obj);
            }
            areaObjectsMap.Remove(lastArea);
            UpdateObjectCounter();
        }
    }
    else
    {
        Debug.LogWarning("No area to delete.");
    }
}


    // Function to gather data from instantiated prefabs
    public void GatherData()
    {   
        if (maData == null)
        {
            Debug.LogError("MAData reference is not assigned!");
            return;
        }
        // Gather data from Area Selector Prefabs
        List<AreaData> areaDataList = new List<AreaData>();
        int areaNumber = 0;
        foreach (GameObject area in areas)
        {
            areaNumber++;
            TMP_Dropdown robotsDropdown = area.transform.Find("RobotsDropdown").GetComponent<TMP_Dropdown>();
            TMP_Dropdown obstaclesDropdown = area.transform.Find("ObstaclesDropdown").GetComponent<TMP_Dropdown>();

            AreaData areaData = new AreaData();
            areaData.areaNumber = areaNumber;
            areaData.robotsDropdownValue = robotsDropdown.options[robotsDropdown.value].text;
            areaData.obstaclesDropdownValue = obstaclesDropdown.options[obstaclesDropdown.value].text;

            areaDataList.Add(areaData);
        }

        // Gather data from Door Selector Prefabs
        List<DoorData> doorDataList = new List<DoorData>();
        for (int i = 0; i < doors.Count; i++)
        {
            GameObject door = doors[i];
            TMP_Dropdown colorDropdown = door.transform.Find("Color").GetComponent<TMP_Dropdown>();
            TMP_Dropdown typeDropdown = door.transform.Find("Type").GetComponent<TMP_Dropdown>();
            TMP_Dropdown color2Dropdown = door.transform.Find("Color 2").GetComponent<TMP_Dropdown>();
            TMP_Dropdown type2Dropdown = door.transform.Find("Type 2").GetComponent<TMP_Dropdown>();

            DoorData doorData = new DoorData();
            doorData.color = colorDropdown.options[colorDropdown.value].text;
            doorData.type = typeDropdown.options[typeDropdown.value].text;
            doorData.color2 = color2Dropdown.options[color2Dropdown.value].text;
            doorData.type2 = type2Dropdown.options[type2Dropdown.value].text;
            doorData.areaNumber = i + 1; // Assign the respective area number

            doorDataList.Add(doorData);
        }

        // Gather data from Object Selector Prefabs
        List<ObjectData> objectDataList = new List<ObjectData>();
        for (int i = 0; i < objectSelectors.Count; i++)
        {
            GameObject objectSelector = objectSelectors[i];
            TMP_Dropdown colorDropdown = objectSelector.transform.Find("Color").GetComponent<TMP_Dropdown>();
            TMP_Dropdown typeDropdown = objectSelector.transform.Find("Type").GetComponent<TMP_Dropdown>();

            ObjectData objectData = new ObjectData();
            objectData.color = colorDropdown.options[colorDropdown.value].text;
            objectData.type = typeDropdown.options[typeDropdown.value].text;

            // Get the area associated with the current object selector from the areaObjectsMap
            int areaN = GetAreaForObjectSelector(objectSelector);

            objectData.areaNumber = areaN; // Assign the respective areaNumber

            objectDataList.Add(objectData);
        }

        // Populate the data lists in the MAData instance
        maData.areaDataList = areaDataList;
        maData.doorDataList = doorDataList;
        maData.objectDataList = objectDataList;



        // Get the index of the next scene
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 2;
        
        // Load the next scene if it exists
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No next scene available.");
        }
    }

    public void DeleteAllPrefabs()
    {
        // Delete all areas
        foreach (GameObject area in areas)
        {
            Destroy(area);
        }
        areas.Clear(); // Clear the list of areas

        // Delete all doors
        foreach (GameObject door in doors)
        {
            Destroy(door);
        }
        doors.Clear(); // Clear the list of doors

        // Delete all object selectors
        foreach (GameObject objectSelector in objectSelectors)
        {
            Destroy(objectSelector);
        }
        objectSelectors.Clear(); // Clear the list of object selectors

        // Reset counters
        areaCounter = 0;
        objectCounter = 0;

    }


    public int GetAreaForObjectSelector(GameObject objectSelector)
    {
        foreach (var entry in areaObjectsMap)
        {
            if (entry.Value.objectSelectors.Contains(objectSelector))
            {
                return entry.Value.areaNumber;
            }
        }
        return -1; // Return -1 if the object selector is not found in any area
    }

    // Method to update the object counter based on the number of object selectors
    private void UpdateObjectCounter()
    {
        objectCounter = objectSelectors.Count;
    }
}
