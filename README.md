# Simulator_Reachy2021

|   License     |     |
| ------------- | :-------------: |
| Title  | [Creatives Commons BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode) |
| Logo  | [![Creative Commons BY-NC-SA 4.0](https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png) ](http://creativecommons.org/licenses/by-nc-sa/4.0/)  |


## Create your own simulator

Download the Unity package available on the [release page](https://github.com/pollen-robotics/Simulator_Reachy2021/releases).
Download the [grpc_unity_package](https://packages.grpc.io/archive/2022/04/67538122780f8a081c774b66884289335c290cbe-f15a2c1c-582b-4c51-acf2-ab6d711d2c59/csharp/grpc_unity_package.2.47.0-dev202204190851.zip) from the [gRPC daily builds](https://packages.grpc.io/archive/2022/04/67538122780f8a081c774b66884289335c290cbe-f15a2c1c-582b-4c51-acf2-ab6d711d2c59/index.xml).

To start using the simulator:
1. Create a new 3D Unity project (or open an existing one)
2. Extract all from the previously downloaded grpc_unity_package, and paste the **Plugins** folder directly in the **Assets** folder of your Unity project.
3. From the menu **Assets/Import Package/Custom Package...**, import the *reachy2021-simulator.unitypackage* you previously downloaded.
4. Drag and drop Reachy and the Server from the Prefabs folder into your scene.
5. Then click Play and choose how to connect to your robot

You can create your own scene and environment for Reachy to evolve in!

## Use your simulator

The Unity simulator is only offering the gRPC services of the robot, not the below ROS2 services.  
For this reason, the simulator is compatible with:
- [Reachy 2021 Python SDK](https://docs.pollen-robotics.com/sdk/getting-started/introduction/): 
connect to the simulated robot with the usual command:

```
from reachy_sdk import ReachySDK

reachy = ReachySDK(host='localhost') # Replace with the actual IP
``` 

- VR teleoperation app
- Any gRPC client you may create, based on [reachy-sdk-api](https://github.com/pollen-robotics/reachy-sdk-api)
