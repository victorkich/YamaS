using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std; // Import the standard message type for String
using System;

public class LLMSettingsManager : MonoBehaviour
{
    public LLMSettings llmSettings;

    public Toggle enableLLMToggle;
    public TMP_Dropdown apiDropdown;
    public TMP_InputField apiKeyInputField;
    public TMP_InputField apiModelInputField;
    public TMP_InputField availableFunctionsInputField;

    private ROSConnection ros;
    private string topicName = "llm_settings";

    void Start()
    {
        // Initialize ROSConnection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>(topicName);
    }

    public void SaveLLMSettings()
    {
        if (llmSettings == null)
        {
            Debug.LogError("LLMSettings reference is not assigned!");
            return;
        }

        llmSettings.EnableLLM = enableLLMToggle.isOn;
        llmSettings.API = apiDropdown.options[apiDropdown.value].text;
        llmSettings.APIKey = apiKeyInputField.text;
        llmSettings.APIModel = apiModelInputField.text;
        llmSettings.AvailableFunctions = availableFunctionsInputField.text;

        // Serialize LLMSettings to JSON
        string llmSettingsJson = JsonUtility.ToJson(llmSettings);

        // Create a ROS String message
        StringMsg message = new StringMsg(llmSettingsJson);

        // Publish the message
        ros.Publish(topicName, message);

        Debug.Log("LLM Settings saved and published successfully: " + llmSettingsJson);
    }
}
