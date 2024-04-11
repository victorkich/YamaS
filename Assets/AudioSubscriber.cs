using UnityEngine;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;

[RequireComponent(typeof(AudioSource))]
public class AudioSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "tts_output";
    private AudioSource audioSource;

    void Start()
    {
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();

        // Start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Float32MultiArrayMsg>(topicName, PlayAudioMessage);
    }

    void PlayAudioMessage(Float32MultiArrayMsg audioMessage)
    {
        Debug.Log($"Received audio message with {audioMessage.data.Length} samples.");

        // Convert the received data back into an AudioClip
        float[] audioData = audioMessage.data;
        int sampleRate = 44100; // Adjust sample rate as needed
        int channelCount = 1; // Adjust based on the audio data

        AudioClip clip = AudioClip.Create("TTSOutput", audioData.Length, channelCount, sampleRate, false);
        clip.SetData(audioData, 0);
        
        Debug.Log($"Playing samples.");

        // Play the audio clip
        audioSource.clip = clip;
        audioSource.Play();
    }
}
