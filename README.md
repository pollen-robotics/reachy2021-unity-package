# reachy2021-unity-package

|   License     |     |
| ------------- | :-------------: |
| Title  | [Creatives Commons BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode) |
| Logo  | [![Creative Commons BY-NC-SA 4.0](https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png) ](http://creativecommons.org/licenses/by-nc-sa/4.0/)  |


## Install the simulator

1. Download the Unity package available on the [release page](https://github.com/pollen-robotics/Simulator_Reachy2021/releases), or add
```
https://github.com/pollen-robotics/reachy2021-unity-package.git?path=/Packages/ReachySimulator#master
```

to the Package Manager (add package from git URL).

2. Download the [grpc_unity_package](https://packages.grpc.io/archive/2022/04/67538122780f8a081c774b66884289335c290cbe-f15a2c1c-582b-4c51-acf2-ab6d711d2c59/csharp/grpc_unity_package.2.47.0-dev202204190851.zip) from the [gRPC daily builds](https://packages.grpc.io/archive/2022/04/67538122780f8a081c774b66884289335c290cbe-f15a2c1c-582b-4c51-acf2-ab6d711d2c59/index.xml). Unzip it in the **Assets** folder. It can be done automatically from the menu "Pollen Robotics/Install GRPC". You may want to restart Unity if the menu is not visible after installing the package.

## Create your own simulator

1. Create a new 3D Unity project (or open an existing one)
2. Drag and drop Reachy and the Server from the Prefabs folder into your scene.
3. Then click Play and start controlling the robot.

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
