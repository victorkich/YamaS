using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AreasManager : MonoBehaviour
{
    public GameObject areaPrefab; // Reference to the prefab of the area
    public Transform areasContainer; // Container where the areas will be instantiated
    private int areaCounter = 1; // Counter to keep track of the number of areas added
    private List<GameObject> areas = new List<GameObject>(); // List to keep track of the instantiated areas

    public void AddArea()
    {
        GameObject newArea = Instantiate(areaPrefab, areasContainer);
        areas.Add(newArea); // Add the new area to the list 

        // Find the "AreaName" TextMeshPro component within the instantiated area
        TMP_Text areaNameTextMeshPro = newArea.transform.Find("AreaText").GetComponent<TMP_Text>();

        // Check if the "AreaName" TextMeshPro component is found
        if (areaNameTextMeshPro != null)
        {
            // Update the area name property based on the counter value
            areaCounter++;
            areaNameTextMeshPro.text = "Area " + areaCounter.ToString() + ":";
        }
        else
        {
            Debug.LogError("TextMeshPro component 'AreaName' not found in the instantiated area.");
        }
    }

    public void DeleteLastArea()
    {
        // Check if there are any areas to delete
        if (areas.Count > 0)
        {
            // Get the last added area
            GameObject lastArea = areas[areas.Count - 1];

            // Remove it from the list
            areas.RemoveAt(areas.Count - 1);

            // Destroy the GameObject associated with it
            Destroy(lastArea);
            
            // Decrement the area counter
            areaCounter--;
        }
        else
        {
            Debug.LogWarning("No area to delete.");
        }
    }
}