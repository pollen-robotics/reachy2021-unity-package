# Simulator_Reachy2021

|   License     |     |
| ------------- | :-------------: |
| Title  | [Creatives Commons BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode) |
| Logo  | [![Creative Commons BY-NC-SA 4.0](https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png) ](http://creativecommons.org/licenses/by-nc-sa/4.0/)  |


## Create your own simulator

To use the simulator, download the Unity package.

To start using the simulator:
1. Create a new 3D Unity project (or open an existing one)
2. From **Assets/Import Package/Custom Package...**, import the package you previously downloaded.
3. Drag and drop Reachy and the Server from the Prefabs folder into your scene.
4. Then click Play and choose how to connect to your robot

You can create your own scene and environment for Reachy to evolve in!

## Use your simulator

The Unity simulator is only offering the gRPC services of the robot, not the below ROS2 services.  
For this reason, the simulator is compatible with:
- [Reachy 2021 Python SDK](https://docs.pollen-robotics.com/sdk/getting-started/introduction/): 
connect to the simulated robot with the usual [Reachy 2021 Python SDK](https://docs.pollen-robotics.com/sdk/getting-started/introduction/) command:

```
from reachy_sdk import ReachySDK

reachy = ReachySDK(host='localhost') # Replace with the actual IP
``` 

- VR teleoperation app
- Any gRPC client you may create, based on [reachy-sdk-api](https://github.com/pollen-robotics/reachy-sdk-api)
