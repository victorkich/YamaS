using UnityEngine;
using System.Collections;
using System;
using System.IO;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std; // For using the ByteMultiArray message type

public class AudioRecorder : MonoBehaviour
{
    private string savePath = "voice_commands_recording/command.wav";
    private AudioClip recordedClip;
    private int sampleRate = 44100;
    private bool isRecording = false;
    private ROSConnection ros;
    public string topicName = "audio_command";

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float32MultiArrayMsg>(topicName);
    }
    void Update()
    {
        // Start recording when the space bar is pressed down
        if (Input.GetKeyDown(KeyCode.Space) && !isRecording)
        {
            StartCoroutine(StartRecording());
        }

        // Stop recording when the space bar is released
        if (Input.GetKeyUp(KeyCode.Space) && isRecording)
        {
            StartCoroutine(StopAndSaveRecording());
        }
    }

    IEnumerator StartRecording()
    {
        isRecording = true;
        recordedClip = Microphone.Start(null, false, 10, sampleRate); // 10 seconds max, adjust as needed
        yield return null; // Wait until the end of the frame to ensure the recording has started
        Debug.Log("Recording started...");
    }
    IEnumerator StopAndSaveRecording()
    {
        // Wait a frame to ensure recording has stopped
        yield return null; 

        int lastSample = Microphone.GetPosition(null);
        Microphone.End(null); // Stop the microphone
        isRecording = false;

        // Calculate the actual length of the recording in samples, adjusting to not cut the last 0.5 seconds
        // Assuming the recording hasn't looped, and you've recorded less than the maximum length
        int adjustedSampleLength = Mathf.Max(0, lastSample - (int)(0.5f * sampleRate));

        // Create a new AudioClip to hold the trimmed recording, using the adjusted length
        AudioClip trimmedClip = AudioClip.Create("TrimmedRecording", adjustedSampleLength, recordedClip.channels, sampleRate, false);
        
        // Get data from the recorded clip and apply to the trimmed clip
        float[] data = new float[adjustedSampleLength * recordedClip.channels];
        recordedClip.GetData(data, 0);
        trimmedClip.SetData(data, 0);

        // Save the trimmed recording
        SaveRecording(trimmedClip);

        Debug.Log("Recording stopped and saved.");
    }
    void SaveRecording(AudioClip clipToSave)
    {
        float[] audioarray = SavWav.Save(savePath, clipToSave);

        Debug.Log($"Saved recorded audio to: {savePath}");

        // Convert float[] to double[]
        // double[] audioarrayDataDouble = Array.ConvertAll(audioarray, item => (double)item);

        Float32MultiArrayMsg audio = new Float32MultiArrayMsg{
                data = audioarray
                };
        
        // Publish the message
        ros.Publish(topicName, audio);
        Debug.Log("Published recorded audio.");

    }
}
