using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry; // For TwistMsg
using RosMessageTypes.Nav; // For OdometryMsg
using Unity.Robotics.UrdfImporter.Control;
using RosMessageTypes.Std;


namespace RosSharp.Control
{
    public enum ControlMode { Keyboard, ROS};

    public class AGVController : MonoBehaviour
    {
        public GameObject wheel1;
        public GameObject wheel2;
        public ControlMode mode = ControlMode.ROS;

        private ArticulationBody wA1;
        private ArticulationBody wA2;

        public float maxLinearSpeed = 2; //  m/s
        public float maxRotationalSpeed = 1; //
        public float wheelRadius = 0.033f; //meters
        public float trackWidth = 0.288f; // meters Distance between tyres
        public float forceLimit = 10;
        public float damping = 10;

        public float ROSTimeout = 0.5f;
        private float lastCmdReceived = 0f;

        ROSConnection ros;
        private string odometryTopic;
        private RotationDirection direction;
        private float rosLinear = 0f;
        private float rosAngular = 0f;

        private const float SCALE_FACTOR_Y = 0.4f; // Fator de correção para a proporção de tamanho (1/2.5 = 0.4)
        private const float SCALE_FACTOR_X = 0.4f;

        void Start()
        {
            wA1 = wheel1.GetComponent<ArticulationBody>();
            wA2 = wheel2.GetComponent<ArticulationBody>();
            SetParameters(wA1);
            SetParameters(wA2);
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<TwistMsg>("/" + gameObject.tag + "/cmd_vel", ReceiveROSCmd);

            odometryTopic = "/" + gameObject.tag + "/odom";
            // Step 2: Initialize the publisher
            ros.RegisterPublisher<OdometryMsg>(odometryTopic);
        }

        void ReceiveROSCmd(TwistMsg cmdVel)
        {
            rosLinear = (float)cmdVel.linear.x;
            rosAngular = (float)cmdVel.angular.z;
            lastCmdReceived = Time.time;
            // Debug.Log(cmdVel);
        }

        void FixedUpdate()
        {
            if (mode == ControlMode.Keyboard)
            {
                KeyBoardUpdate();
            }
            else if (mode == ControlMode.ROS)
            {
                ROSUpdate();
            }
            PublishOdometry();
        }

        private void PublishOdometry()
        {
            // Encontrar o GameObject do base_footprint
            GameObject baseFootprint = GameObject.Find("base_link"); // Ajuste este caminho conforme necessário

            if (baseFootprint != null)
            {
                // Usar a posição e a orientação do base_footprint
                Vector3 position = baseFootprint.transform.position;
                Quaternion orientation = baseFootprint.transform.rotation;

                // Aplicar a correção de escala à posição
                Vector3 scaledPosition = new Vector3(position.x * SCALE_FACTOR_X, position.y, position.z * SCALE_FACTOR_Y);

                // Construir a mensagem de Header
                HeaderMsg header = new HeaderMsg
                {
                    frame_id = "odom"
                };

                PoseWithCovarianceMsg poseWithCovariance = new PoseWithCovarianceMsg
                {
                    pose = new PoseMsg
                    {
                        position = new PointMsg { x = scaledPosition.x, y = scaledPosition.z, z = scaledPosition.y },
                        orientation = new QuaternionMsg { x = orientation.x, y = orientation.z, z = orientation.y, w = orientation.w }
                    }
                };

                TwistWithCovarianceMsg twistWithCovariance = new TwistWithCovarianceMsg
                {
                    twist = new TwistMsg
                    {
                        linear = new Vector3Msg(),
                        angular = new Vector3Msg()
                    }
                };

                OdometryMsg odometryMsg = new OdometryMsg(header, "base_link", poseWithCovariance, twistWithCovariance);

                ros.Publish(odometryTopic, odometryMsg);
            }
            else
            {
                Debug.LogError("base_link não encontrado!");
            }
        }

        private void SetParameters(ArticulationBody joint)
        {
            ArticulationDrive drive = joint.xDrive;
            drive.forceLimit = forceLimit;
            drive.damping = damping;
            joint.xDrive = drive;
        }

        private void SetSpeed(ArticulationBody joint, float wheelSpeed = float.NaN)
        {
            ArticulationDrive drive = joint.xDrive;
            if (float.IsNaN(wheelSpeed))
            {
                drive.targetVelocity = ((2 * maxLinearSpeed) / wheelRadius) * Mathf.Rad2Deg * (int)direction;
            }
            else
            {
                drive.targetVelocity = wheelSpeed;
            }
            joint.xDrive = drive;
        }

        private void KeyBoardUpdate()
        {
            float moveDirection = Input.GetAxis("Vertical");
            float inputSpeed;
            float inputRotationSpeed;
            if (moveDirection > 0)
            {
                inputSpeed = maxLinearSpeed;
            }
            else if (moveDirection < 0)
            {
                inputSpeed = maxLinearSpeed * -1;
            }
            else
            {
                inputSpeed = 0;
            }

            float turnDirction = Input.GetAxis("Horizontal");
            if (turnDirction > 0)
            {
                inputRotationSpeed = maxRotationalSpeed;
            }
            else if (turnDirction < 0)
            {
                inputRotationSpeed = maxRotationalSpeed * -1;
            }
            else
            {
                inputRotationSpeed = 0;
            }
            RobotInput(inputSpeed, inputRotationSpeed);
        }


        private void ROSUpdate()
        {
            if (Time.time - lastCmdReceived > ROSTimeout)
            {
                rosLinear = 0f;
                rosAngular = 0f;
            }
            RobotInput(rosLinear, -rosAngular);
        }

        private void RobotInput(float speed, float rotSpeed) // m/s and rad/s
        {
            if (speed > maxLinearSpeed)
            {
                speed = maxLinearSpeed;
            }
            if (rotSpeed > maxRotationalSpeed)
            {
                rotSpeed = maxRotationalSpeed;
            }
            float wheel1Rotation = (speed / wheelRadius);
            float wheel2Rotation = wheel1Rotation;
            float wheelSpeedDiff = ((rotSpeed * trackWidth) / wheelRadius);
            if (rotSpeed != 0)
            {
                wheel1Rotation = (wheel1Rotation + (wheelSpeedDiff / 1)) * Mathf.Rad2Deg;
                wheel2Rotation = (wheel2Rotation - (wheelSpeedDiff / 1)) * Mathf.Rad2Deg;
            }
            else
            {
                wheel1Rotation *= Mathf.Rad2Deg;
                wheel2Rotation *= Mathf.Rad2Deg;
            }
            SetSpeed(wA1, wheel1Rotation);
            SetSpeed(wA2, wheel2Rotation);
        }
    }
}