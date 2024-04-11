using UnityEngine;
using TMPro; // For TextMeshPro components
using UnityEngine.SceneManagement;

public class SASettingsManager : MonoBehaviour
{
    // Reference to your SAData ScriptableObject
    public SAData saData;

    // References to your menu UI elements
    public TMP_InputField xInputField;
    public TMP_InputField yInputField;
    public TMP_Dropdown greenAreasDropdown;
    public TMP_Dropdown redAreasDropdown;
    public TMP_Dropdown obstaclesDropdown;

    public void SaveSADataSettings()
    {
        if (saData == null)
        {
            Debug.LogError("SAData reference is not assigned!");
            return;
        }

        // Check if X and Y input fields contain valid integers
        if (!int.TryParse(xInputField.text, out int x) || !int.TryParse(yInputField.text, out int y))
        {
            Debug.LogError("X and Y values must be integers. Saving aborted.");
            return;
        }

        // Save data from UI elements to the SAData ScriptableObject
        saData.roomSize = new Vector2(x, y);
        saData.numberOfGreenAreas = greenAreasDropdown.value + 1; // Assuming dropdown values start from 0 but represent 1 to N
        saData.numberOfRedAreas = redAreasDropdown.value + 1; // Assuming dropdown values start from 0 but represent 1 to N
        saData.numberOfObjects = obstaclesDropdown.value + 1; // Adjust this logic based on how dropdown values correspond to actual numbers

        Debug.Log("SAData settings saved successfully.");

        // Get the index of the next scene
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
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
}
