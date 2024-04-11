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

### ROS Integration
1. Ensure ROS is properly installed and set up on your system.
2. Use the provided ROS packages in the `ros_integration` directory to connect your physical or simulated robots to the YamaS environment.
3. Utilize ROS topics to control robots within the simulation and receive sensor data.

### Virtual Reality (VR)
1. Connect your VR headset to your PC.
2. Configure the VR settings within Unity based on your headset model.
3. Engage with the simulation through VR for a more immersive HRI experience.

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
