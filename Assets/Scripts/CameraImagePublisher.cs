using UnityEngine;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
//using RosMessageTypes.UnityRoboticsDemo;
using System;

public class CameraImagePublisher : MonoBehaviour
{
    public string topicName = "/camera/image_raw";
    public int publishFrequency = 10;

    private Camera _camera;
    private RenderTexture _renderTexture;
    public GameObject robot;
    private ROSConnection _ros;
    private ImageMsg imageMessage;
    private float _timeElapsed;

    void Start()
    {
        // Inicializa o objeto imageMessage
        imageMessage = new ImageMsg();
        imageMessage.header.frame_id = "camera";
        imageMessage.height = 144;
        imageMessage.width = 192;
        imageMessage.encoding = "jpg";
        // imageMessage.is_bigendian = false;
        // imageMessage.step = false;

        _camera = GetComponent<Camera>();
        _renderTexture = new RenderTexture(1280, 720, 24); // Resolução Full HD
        //_renderTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 24);
        _camera.targetTexture = _renderTexture;

        _ros = ROSConnection.GetOrCreateInstance();
        string fullTopicName = "/" + robot.gameObject.tag + topicName;

        if (!_ros.IsPublisherRegistered(fullTopicName))
        {
            _ros.RegisterPublisher<ImageMsg>(fullTopicName);
        }

        _timeElapsed = 0.0f;
    }

    void Update()
    {
        _timeElapsed += Time.deltaTime;

        if (_timeElapsed >= 1.0f / publishFrequency)
        {
            PublishImage();
            _timeElapsed = 0.0f;
        }
    }

    private void PublishImage()
    {
        Texture2D texture2D = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = _renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = null;

        byte[] imageData = texture2D.EncodeToJPG(100);
        Destroy(texture2D);

        imageMessage.data = imageData;
        _ros.Publish("/" + robot.gameObject.tag + topicName, imageMessage);
    }

    void OnGUI()
    {
        // Defina a largura e a altura para o tamanho desejado da visualização da câmera na tela
        int width = 426;
        int height = 240; // Estes valores são exemplos, ajuste conforme necessário

        // Canto superior direito
        int x = Screen.width - width;
        int y = 0;

        // Cria um retângulo onde a textura será exibida
        Rect rect = new Rect(x - 20, y + 20, width, height);

        // Desenha a RenderTexture na tela
        GUI.DrawTexture(rect, _renderTexture);
    }
}