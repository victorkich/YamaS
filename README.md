# YamaS Framework

## Overview
YamaS is an advanced simulation framework developed at the Intelligent Robot Laboratory, University of Tsukuba, designed to bridge the gap between virtual training and real-world application for autonomous robotic navigation. Integrating Unity3D Engine with Robotic Operating System (ROS) and ROS2, YamaS offers a versatile platform for deep reinforcement learning (Deep-RL) and natural language processing (NLP) in both single and multi-agent scenarios. It features procedural environment generation, RGB vision, dynamic obstacle navigation, and Virtual Reality (VR) integration for immersive human-robot interaction (HRI) studies.

## Key Features
- **High-Fidelity Simulation**: Supports single and multi-agent environments for detailed scenario configurations, enabling comprehensive testing and development.
- **Dynamic Environment Generation**: Leverages procedural generation and NLP for creating complex, varied environments, fostering innovative AI training and optimization.
- **Real-World Accuracy**: Validates sensor simulations and spatial reasoning against real-world robots, reducing the "reality gap."
- **VR Integration**: Augments HRI studies, offering an immersive platform for developers and researchers.
- **Collaborative Robotics**: Facilitates the development and testing of collaborative strategies and Deep-RL algorithms.

## Installation
### Prerequisites
- Unity3D
- ROS (Robot Operating System)
- Python 3

### Setup
1. Clone the YamaS repository: `git clone https://github.com/victorkich/YamaS.git`
2. Follow the setup instructions in the `INSTALL.md` file within the repository for detailed environment setup.

## Usage
### Simulation Environment Setup
1. Launch Unity3D and open the YamaS project.
2. Configure your simulation parameters through the Unity Editor interface or by editing the `config.json` file.
3. Start the simulation within Unity.

## ROS Integration
1. Ensure ROS is properly installed and set up on your system.
2. **Modify the ROSConnection.cs file**: Navigate to `Packages/com.unity.robotics.ros-tcp-connector/Runtime/TcpConnector/` and open `ROSConnection.cs`. Add the following method to the `ROSConnection` class:
   ```csharp
   public bool IsPublisherRegistered(string topicName)
   {
       RosTopicState topicState;
       if (m_Topics.TryGetValue(topicName, out topicState))
       {
           return topicState.IsPublisher;
       }
       return false;
   }

### ROS 2 Integration via Docker
To control the robots in the simulation using ROS 2, follow these steps to set up a Docker environment:

1. **Install the Docker Container**:
   Run the following command to pull the necessary Docker container:
   ```bash
   docker pull victorkich/ros2:multiagentsv2
   ```

2. **Start the Docker Container**:
   To open the Docker container and prepare it for integration with the Unity simulator, execute:
   ```bash
   docker run -it -p 10000:10000 -p 5005:5005 victorkich/ros2:multiagents bash
   ```

3. **Connect to the Unity Simulator**:
   Once inside the Docker container, initiate the connection to the Unity simulator by running:
   ```bash
   ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=0.0.0.0
   ```

4. **Custom ROS Nodes**:
   You are free to modify the setup and create custom ROS nodes. Open a new terminal tab, re-enter the container if necessary, and execute your ROS nodes to control the robots within the simulation environment.

### Virtual Reality (VR)
1. Connect your VR headset to your PC.
2. Configure the VR settings within Unity based on your headset model.
3. Engage with the simulation through VR for a more immersive HRI experience.

## Visual Demonstrations
Here are some animated demonstrations of YamaS in action:
<p float="left">
  <img src="media/yamas_interaction.gif" width="100%" />
  <img src="media/yamas_menu_demonstration.gif" width="49.7%" />
  <img src="media/yamas_moving.gif" width="49.7%" />
</p>

## Examples
Check out the `examples` directory for sample projects and scripts to get started with common tasks and simulations in YamaS.

## Contributing
We welcome contributions! Please read the `CONTRIBUTING.md` file for guidelines on how to submit pull requests, report issues, or request new features.

## License
YamaS is open-sourced under the MIT license. See the `LICENSE` file for more details.

## Contact
For any inquiries or support, please contact the primary developers:
- Victor A. Kich: victorkich98@gmail.com
- Jair A. Bottega: jairaugustobottega@gmail.com

## Acknowledgments
This work was supported by the Intelligent Robot Laboratory at the University of Tsukuba and collaborators from various institutions. We thank all contributors and researchers who have made this project possible.
