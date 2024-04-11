using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
//using RosMessageTypes.UnityRoboticsDemo;
using System.Runtime.Serialization;
using RosMessageTypes.Sensor;

namespace RosSharp.Control
{
    public enum DrawRay { ON, OFF};

    public class LaserScanPublisher : MonoBehaviour
    {
        ROSConnection ros;

        [SerializeField]
        public string topicName = "/scan";
        
        [SerializeField]
        public DrawRay DrawRays = DrawRay.ON;

        public int numberOfIncrements = 360;
        public float maxRange = 100f;

        public float ROSTimeout = 0.5f;

        [HideInInspector]
        public float timeElapsed;

        [HideInInspector]
        public static float[] distances;

        [SerializeField]
        private float publishRate = 0.1f;

        private float nextPublishTime;

        private LaserScanMsg laserScan;
        
        public GameObject robot;

        private static bool isPublisherRegistered = false;

        // Use this for initialization
        void Start () {
            ros = ROSConnection.GetOrCreateInstance();
            string fullTopicName = "/" + robot.gameObject.tag + topicName;

            // Verifica se o tópico já foi registrado antes de tentar registrar novamente
            if (!ros.IsPublisherRegistered(fullTopicName))
            {
                ros.RegisterPublisher<LaserScanMsg>(fullTopicName);
            }
            
            distances = new float[numberOfIncrements];

            // Inicializa o objeto LaserScan
            laserScan = new LaserScanMsg();
            // Configura o frame_id para o LaserScan
            laserScan.header.frame_id = "laser";


            // Configura os ângulos mínimo e máximo do campo de visão do LaserScan
            laserScan.angle_min = -Mathf.PI / 2;
            laserScan.angle_max = Mathf.PI / 2;

            // Configura a resolução do LaserScan
            laserScan.angle_increment = Mathf.PI / 180;

            // Configura a distância mínima e máxima detectada pelo LaserScan
            laserScan.range_min = 0.1f;
            laserScan.range_max = 10.0f;

            // distances = new float[numberOfIncrements];
            // ros = ROSConnection.GetOrCreateInstance();
            // ros.Subscribe<ScanMsg>("scan", ReceiveROSCmd);
        }

        // Update is called once per frame
        void FixedUpdate () {

            Vector3 fwd = new Vector3(0, 0, 1);
            Vector3 dir;
            RaycastHit hit;
            int indx = 0;
            laserScan.ranges = new float[numberOfIncrements];

            for (int incr = 0; incr < numberOfIncrements; incr++)
            {
                
                indx = incr;
                dir = transform.rotation * Quaternion.Euler(0, incr, 0)*fwd;
            
                
                if (Physics.Raycast(transform.position, dir, out hit, maxRange))
                {
                    distances[indx] = (float)hit.distance;
                }
                else
                {
                    distances[indx] = maxRange;
                }
                if (DrawRays == DrawRay.ON){
                    Debug.DrawRay(transform.position, dir * distances[indx], Color.red);
                }
                laserScan.ranges[indx] = distances[indx];
            }

            timeElapsed += Time.deltaTime;

            if (timeElapsed > publishRate)
            {
                ros.Publish("/" + robot.gameObject.tag + topicName, laserScan);
                timeElapsed = 0;
            }
        }
    }
}
