using UnityEngine;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reachy;

using Grpc.Core;
using Grpc.Core.Utils;
using Reachy.Sdk.Joint;
using Reachy.Sdk.Fan;
using Reachy.Sdk.Kinematics;
using Reachy.Sdk.Mobility;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

class JointServiceServer : MonoBehaviour
{
    public static ReachyController reachy;
    static Server server;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("Arm_kinematics.dll", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
    [DllImport("libArm_kinematics.so", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    [DllImport("libArm_kinematics.dylib", CallingConvention = CallingConvention.Cdecl)]
#endif
    private static extern void setup();

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("Arm_kinematics.dll", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
    [DllImport("libArm_kinematics.so", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    [DllImport("libArm_kinematics.dylib", CallingConvention = CallingConvention.Cdecl)]
#endif
    private static extern void forward(ArmSide side, double[] q, int n, double[] M);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("Arm_kinematics.dll", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
    [DllImport("libArm_kinematics.so", CallingConvention = CallingConvention.Cdecl)]
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
    [DllImport("libArm_kinematics.dylib", CallingConvention = CallingConvention.Cdecl)]
#endif
    private static extern void inverse(ArmSide side, double[] M, double[] q);

    private static CancellationTokenSource askForCancellation = new CancellationTokenSource();


    void Start()
    {
        reachy = GameObject.Find("Reachy").GetComponent<ReachyController>();
        gRPCServer();
        setup(); // Setup Arm_kinematics
    }

    public static void gRPCServer()
    {
        const int PortJoint = 50055;
        server = new Server
        {
            Services = {
                JointService.BindService(new JointServiceImpl()),
                FullBodyCartesianCommandService.BindService(new FullBodyCartesianCommandServiceImpl()),
                SensorService.BindService(new SensorServiceImpl()),
                FanControllerService.BindService(new FanControllerServiceImpl()),
                ArmKinematics.BindService(new ArmKinematicsImpl()),
                MobileBasePresenceService.BindService(new MobileBasePresenceServiceImpl()),
                },
            Ports = { new ServerPort("0.0.0.0", PortJoint, ServerCredentials.Insecure) },
        };
        server.Start();
    }

    void OnDestroy()
    {
        server.ShutdownAsync().Wait();
    }

    void OnApplicationQuit()
    {
        askForCancellation.Cancel();
        askForCancellation.Dispose();
    }

    public class JointServiceImpl : JointService.JointServiceBase
    {
        public override Task<JointsCommandAck> SendJointsCommands(JointsCommand jointsCommand, ServerCallContext context)
        {
            try
            {
                Dictionary<JointId, float> commands = new Dictionary<JointId, float>();
                Dictionary<JointId, bool> compliancy = new Dictionary<JointId, bool>();
                for (int i = 0; i < jointsCommand.Commands.Count; i++)
                {
                    if (jointsCommand.Commands[i].GoalPosition != null)
                    {
                        float command = Mathf.Rad2Deg * (float)jointsCommand.Commands[i].GoalPosition;
                        commands.Add(jointsCommand.Commands[i].Id, command);
                    }

                    if (jointsCommand.Commands[i].Compliant != null)
                    {
                        bool isCompliant = (bool)jointsCommand.Commands[i].Compliant;
                        compliancy.Add(jointsCommand.Commands[i].Id, isCompliant);
                    }
                }
                reachy.HandleCommand(commands);
                reachy.HandleCompliancy(compliancy);
                return Task.FromResult(new JointsCommandAck { Success = true });
            }
            catch
            {
                return Task.FromResult(new JointsCommandAck { Success = false });
            }
        }

        public override async Task<JointsCommandAck> StreamJointsCommands(IAsyncStreamReader<JointsCommand> requestStream, ServerCallContext context)
        {
            CancellationToken cancellationToken = askForCancellation.Token;
            try
            {
                while (await requestStream.MoveNext(cancellationToken))
                {
                    var jointsCommand = requestStream.Current;
                    await SendJointsCommands(jointsCommand, context);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning(e);
            }
            return (new JointsCommandAck { Success = true });
        }


        public override Task<JointsState> GetJointsState(JointsStateRequest jointRequest, ServerCallContext context)
        {
            Dictionary<JointId, JointField> request = new Dictionary<JointId, JointField>();

            for (int i = 0; i < jointRequest.Ids.Count; i++)
            {
                request.Add(jointRequest.Ids[i], JointField.PresentPosition);
            }
            var motors = reachy.GetCurrentMotorsState(request);

            List<JointState> listJointStates = new List<JointState>();
            List<JointId> listJointIds = new List<JointId>();
            foreach (var item in motors)
            {
                var jointState = new JointState();
                jointState.Name = item.name;
                jointState.Uid = (uint?)item.uid;
                if (jointRequest.RequestedFields.Contains(JointField.PresentPosition))
                {
                    jointState.PresentPosition = item.present_position;
                }
                if (jointRequest.RequestedFields.Contains(JointField.PresentSpeed))
                {
                    jointState.PresentSpeed = 0;
                }
                if (jointRequest.RequestedFields.Contains(JointField.PresentLoad))
                {
                    jointState.PresentLoad = 0;
                }
                if (jointRequest.RequestedFields.Contains(JointField.Temperature))
                {
                    jointState.Temperature = 0;
                }
                if (jointRequest.RequestedFields.Contains(JointField.Compliant))
                {
                    jointState.Compliant = item.isCompliant;
                }
                if (jointRequest.RequestedFields.Contains(JointField.GoalPosition))
                {
                    jointState.GoalPosition = item.goal_position;
                }
                if (jointRequest.RequestedFields.Contains(JointField.SpeedLimit))
                {
                    jointState.SpeedLimit = 0;
                }
                if (jointRequest.RequestedFields.Contains(JointField.TorqueLimit))
                {
                    jointState.TorqueLimit = 100;
                }
                if (jointRequest.RequestedFields.Contains(JointField.Pid))
                {
                    jointState.Pid = new PIDValue { Pid = new PIDGains { P = 0, I = 0, D = 0 } };
                }
                if (jointRequest.RequestedFields.Contains(JointField.All))
                {
                    jointState.PresentPosition = item.present_position;
                    jointState.PresentSpeed = 0;
                    jointState.PresentLoad = 0;
                    jointState.Temperature = 0;
                    jointState.Compliant = false;
                    jointState.GoalPosition = item.goal_position;
                    jointState.SpeedLimit = 0;
                    jointState.TorqueLimit = 100;
                    jointState.Pid = new PIDValue { Pid = new PIDGains { P = 0, I = 0, D = 0 } };
                }

                listJointStates.Add(jointState);
                listJointIds.Add(new JointId { Name = item.name });
            };

            JointsState state = new JointsState
            {
                Ids = { listJointIds },
                States = { listJointStates },
            };

            return Task.FromResult(state);
        }

        public override async Task StreamJointsState(StreamJointsRequest jointRequest, Grpc.Core.IServerStreamWriter<JointsState> responseStream, ServerCallContext context)
        {
            CancellationToken cancellationToken = askForCancellation.Token;

            try
            {
                Dictionary<JointId, JointField> request = new Dictionary<JointId, JointField>();
                for (int i = 0; i < jointRequest.Request.Ids.Count; i++)
                {
                    request.Add(jointRequest.Request.Ids[i], JointField.PresentPosition);
                }

                while (!context.CancellationToken.IsCancellationRequested)
                {
                    var motors = reachy.GetCurrentMotorsState(request);

                    List<JointState> listJointStates = new List<JointState>();
                    List<JointId> listJointIds = new List<JointId>();

                    foreach (var item in motors)
                    {
                        var jointState = new JointState();
                        jointState.Name = item.name;
                        jointState.Uid = (uint?)item.uid;
                        if (jointRequest.Request.RequestedFields.Contains(JointField.PresentPosition))
                        {
                            jointState.PresentPosition = item.present_position;
                        }
                        if (jointRequest.Request.RequestedFields.Contains(JointField.PresentSpeed))
                        {
                            jointState.PresentSpeed = 0;
                        }
                        if (jointRequest.Request.RequestedFields.Contains(JointField.PresentLoad))
                        {
                            jointState.PresentLoad = 0;
                        }
                        if (jointRequest.Request.RequestedFields.Contains(JointField.Temperature))
                        {
                            jointState.Temperature = 0;
                        }
                        if (jointRequest.Request.RequestedFields.Contains(JointField.Compliant))
                        {
                            jointState.Compliant = false;
                        }
                        if (jointRequest.Request.RequestedFields.Contains(JointField.GoalPosition))
                        {
                            jointState.GoalPosition = item.goal_position;
                        }
                        if (jointRequest.Request.RequestedFields.Contains(JointField.SpeedLimit))
                        {
                            jointState.SpeedLimit = 0;
                        }
                        if (jointRequest.Request.RequestedFields.Contains(JointField.TorqueLimit))
                        {
                            jointState.TorqueLimit = 0;
                        }
                        if (jointRequest.Request.RequestedFields.Contains(JointField.Pid))
                        {
                            jointState.Pid = new PIDValue { Pid = new PIDGains { P = 0, I = 0, D = 0 } };
                        }
                        if (jointRequest.Request.RequestedFields.Contains(JointField.All))
                        {
                            jointState.PresentPosition = item.present_position;
                            jointState.PresentSpeed = 0;
                            jointState.PresentLoad = 0;
                            jointState.Temperature = 0;
                            jointState.Compliant = false;
                            jointState.GoalPosition = item.goal_position;
                            jointState.SpeedLimit = 0;
                            jointState.TorqueLimit = 0;
                            jointState.Pid = new PIDValue { Pid = new PIDGains { P = 0, I = 0, D = 0 } };
                        }

                        listJointStates.Add(jointState);
                        listJointIds.Add(new JointId { Name = item.name });
                    };

                    JointsState state = new JointsState
                    {
                        Ids = { listJointIds },
                        States = { listJointStates },
                    };
                    await responseStream.WriteAsync(state);
                    await Task.Delay(TimeSpan.FromSeconds(1 / jointRequest.PublishFrequency), cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning(e);
            }
        }

        public override Task<JointsId> GetAllJointsId(Google.Protobuf.WellKnownTypes.Empty empty, ServerCallContext context)
        {
            List<uint> ids = new List<uint>();
            List<string> names = new List<string>();

            for (int i = 0; i < reachy.motors.Length; i++)
            {
                ids.Add((uint)i);
                names.Add(reachy.motors[i].name);
            }

            JointsId allIds = new JointsId
            {
                Names = { names },
                Uids = { ids },
            };

            return Task.FromResult(allIds);
        }
    }

    public class FullBodyCartesianCommandServiceImpl : FullBodyCartesianCommandService.FullBodyCartesianCommandServiceBase
    {
        public override Task<FullBodyCartesianCommandAck> SendFullBodyCartesianCommands(FullBodyCartesianCommand fullBodyCartesianCommand, ServerCallContext context)
        {
            try
            {
                ArmKinematicsImpl armKinematics = new ArmKinematicsImpl();

                JointServiceImpl jointService = new JointServiceImpl();
                List<JointCommand> jointCommandList = new List<JointCommand>();

                if (fullBodyCartesianCommand.LeftArm != null)
                {
                    Task<ArmIKSolution> leftArmTask = armKinematics.ComputeArmIK(fullBodyCartesianCommand.LeftArm, context);
                    ArmIKSolution leftArmSolution = leftArmTask.Result;

                    int iter = 0;
                    foreach (var l_id in leftArmSolution.ArmPosition.Positions.Ids)
                    {
                        jointCommandList.Add(new JointCommand
                        {
                            Id = l_id,
                            GoalPosition = (float?)leftArmSolution.ArmPosition.Positions.Positions[iter],
                        });
                        iter += 1;
                    }
                }
                if (fullBodyCartesianCommand.RightArm != null)
                {
                    Task<ArmIKSolution> rightArmTask = armKinematics.ComputeArmIK(fullBodyCartesianCommand.RightArm, context);
                    ArmIKSolution rightArmSolution = rightArmTask.Result;

                    int iter = 0;
                    foreach (var l_id in rightArmSolution.ArmPosition.Positions.Ids)
                    {
                        jointCommandList.Add(new JointCommand
                        {
                            Id = l_id,
                            GoalPosition = (float?)rightArmSolution.ArmPosition.Positions.Positions[iter],
                        });
                        iter += 1;
                    }
                }

                if (fullBodyCartesianCommand.Neck != null)
                {
                    UnityEngine.Quaternion headRotation = new UnityEngine.Quaternion((float)fullBodyCartesianCommand.Neck.Q.Y,
                        -(float)fullBodyCartesianCommand.Neck.Q.Z,
                        -(float)fullBodyCartesianCommand.Neck.Q.X,
                        (float)fullBodyCartesianCommand.Neck.Q.W);

                    Vector3 neck_commands = Mathf.Deg2Rad * headRotation.eulerAngles;

                    jointCommandList.Add(new JointCommand
                    {
                        Id = new JointId { Name = "neck_roll" },
                        GoalPosition = (float?)ChangeAngleRange(neck_commands[2]),
                    });
                    jointCommandList.Add(new JointCommand
                    {
                        Id = new JointId { Name = "neck_pitch" },
                        GoalPosition = (float?)ChangeAngleRange(neck_commands[0]),
                    });
                    jointCommandList.Add(new JointCommand
                    {
                        Id = new JointId { Name = "neck_yaw" },
                        GoalPosition = -(float?)ChangeAngleRange(neck_commands[1]),
                    });
                }

                JointsCommand jointsCommand = new JointsCommand { Commands = { jointCommandList } };
                jointService.SendJointsCommands(jointsCommand, context);

                return Task.FromResult(new FullBodyCartesianCommandAck
                {
                    LeftArmCommandSuccess = false,
                    RightArmCommandSuccess = false,
                    NeckCommandSuccess = false
                });
            }
            catch
            {
                return Task.FromResult(new FullBodyCartesianCommandAck
                {
                    LeftArmCommandSuccess = false,
                    RightArmCommandSuccess = false,
                    NeckCommandSuccess = false
                });
            }
        }

        public override async Task<FullBodyCartesianCommandAck> StreamFullBodyCartesianCommands(IAsyncStreamReader<FullBodyCartesianCommand> requestStream, ServerCallContext context)
        {
            CancellationToken cancellationToken = askForCancellation.Token;

            try
            {
                while (await requestStream.MoveNext(cancellationToken))
                {
                    var fullBodyCartesianCommand = requestStream.Current;
                    await SendFullBodyCartesianCommands(fullBodyCartesianCommand, context);

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning(e);
            }

            return (new FullBodyCartesianCommandAck
            {
                LeftArmCommandSuccess = true,
                RightArmCommandSuccess = true,
                NeckCommandSuccess = true
            });
        }

        private float ChangeAngleRange(float orbita_angle)
        {
            float modified_angle = orbita_angle % (2.0f * (float)Math.PI);
            modified_angle = modified_angle > (float)Math.PI ? modified_angle - (2.0f * (float)Math.PI) : modified_angle;
            return modified_angle;
        }
    }

    public class ArmKinematicsImpl : ArmKinematics.ArmKinematicsBase
    {
        public override Task<ArmFKSolution> ComputeArmFK(ArmFKRequest fkRequest, ServerCallContext context)
        {
            ArmFKSolution sol;
            if (fkRequest.ArmPosition.Positions.Positions.Count != 7)
            {
                sol = new ArmFKSolution
                {
                    Success = false,
                    EndEffector = new ArmEndEffector
                    {
                        Side = fkRequest.ArmPosition.Side,
                        Pose = new Reachy.Sdk.Kinematics.Matrix4x4 { Data = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } },
                    },
                };
                return Task.FromResult(sol);
            }

            double[] q = new double[7];
            for (int i = 0; i < 7; i++)
            {
                q[i] = fkRequest.ArmPosition.Positions.Positions[i];
            }
            double[] M = new double[16];

            forward(fkRequest.ArmPosition.Side, q, 7, M);

            List<double> listM = new List<double>(M);

            sol = new ArmFKSolution
            {
                Success = true,
                EndEffector = new ArmEndEffector
                {
                    Side = fkRequest.ArmPosition.Side,
                    Pose = new Reachy.Sdk.Kinematics.Matrix4x4 { Data = { listM } },
                },
            };

            return Task.FromResult(sol);
        }

        public override Task<ArmIKSolution> ComputeArmIK(ArmIKRequest ikRequest, ServerCallContext context)
        {
            ArmIKSolution sol;

            double[] M = new double[16];
            if (ikRequest.Target.Pose.Data.Count != 16)
            {
                sol = new ArmIKSolution
                {
                    Success = false,
                    ArmPosition = new ArmJointPosition
                    {
                        Side = ikRequest.Target.Side,
                        Positions = new JointPosition
                        {
                            Ids = { new Reachy.Sdk.Joint.JointId { } },
                            Positions = { },
                        },
                    },
                };

                return Task.FromResult(sol);
            }

            for (int i = 0; i < 16; i++)
            {
                M[i] = ikRequest.Target.Pose.Data[i];
            }
            double[] q = new double[7];
            inverse(ikRequest.Target.Side, M, q);

            List<double> listq = new List<double>(q);

            List<JointId> listJointIds = new List<JointId>();
            if (ikRequest.Target.Side == ArmSide.Right)
            {
                listJointIds.Add(new JointId { Name = "r_shoulder_pitch" });
                listJointIds.Add(new JointId { Name = "r_shoulder_roll" });
                listJointIds.Add(new JointId { Name = "r_arm_yaw" });
                listJointIds.Add(new JointId { Name = "r_elbow_pitch" });
                listJointIds.Add(new JointId { Name = "r_forearm_yaw" });
                listJointIds.Add(new JointId { Name = "r_wrist_pitch" });
                listJointIds.Add(new JointId { Name = "r_wrist_roll" });
            }
            else
            {
                listJointIds.Add(new JointId { Name = "l_shoulder_pitch" });
                listJointIds.Add(new JointId { Name = "l_shoulder_roll" });
                listJointIds.Add(new JointId { Name = "l_arm_yaw" });
                listJointIds.Add(new JointId { Name = "l_elbow_pitch" });
                listJointIds.Add(new JointId { Name = "l_forearm_yaw" });
                listJointIds.Add(new JointId { Name = "l_wrist_pitch" });
                listJointIds.Add(new JointId { Name = "l_wrist_roll" });
            }

            sol = new ArmIKSolution
            {
                Success = true,
                ArmPosition = new ArmJointPosition
                {
                    Side = ikRequest.Target.Side,
                    Positions = new JointPosition
                    {
                        Ids = { listJointIds },
                        Positions = { listq },
                    },
                },
            };

            return Task.FromResult(sol);
        }
    }

    public class SensorServiceImpl : SensorService.SensorServiceBase
    {
        public override Task<SensorsId> GetAllForceSensorsId(Google.Protobuf.WellKnownTypes.Empty empty, ServerCallContext context)
        {
            List<uint> ids = new List<uint>();
            List<string> names = new List<string>();

            for (int i = 0; i < reachy.sensors.Length; i++)
            {
                ids.Add((uint)i);
                names.Add(reachy.sensors[i].name);
            }

            SensorsId allIds = new SensorsId
            {
                Names = { names },
                Uids = { ids },
            };

            return Task.FromResult(allIds);
        }

        public override Task<SensorsState> GetSensorsState(SensorsStateRequest stateRequest, ServerCallContext context)
        {
            var sensors = reachy.GetCurrentSensorsState(stateRequest.Ids);

            List<SensorState> listSensorStates = new List<SensorState>();
            List<SensorId> listSensorIds = new List<SensorId>();
            foreach (var item in sensors)
            {
                var sensorState = new SensorState();
                sensorState.ForceSensorState = new ForceSensorState { Force = item.sensor_state };
                listSensorStates.Add(sensorState);
                listSensorIds.Add(new SensorId { Name = item.name });
            };

            SensorsState state = new SensorsState
            {
                Ids = { listSensorIds },
                States = { listSensorStates },
            };

            return Task.FromResult(state);
        }

        public override async Task StreamSensorStates(StreamSensorsStateRequest stateRequest, IServerStreamWriter<SensorsState> responseStream, ServerCallContext context)
        {
            CancellationToken cancellationToken = askForCancellation.Token;

            try
            {
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    var sensors = reachy.GetCurrentSensorsState(stateRequest.Request.Ids);

                    List<SensorState> listSensorStates = new List<SensorState>();
                    List<SensorId> listSensorIds = new List<SensorId>();
                    foreach (var item in sensors)
                    {
                        var sensorState = new SensorState();
                        sensorState.ForceSensorState = new ForceSensorState { Force = item.sensor_state };
                        listSensorStates.Add(sensorState);
                        listSensorIds.Add(new SensorId { Name = item.name });
                    };

                    SensorsState state = new SensorsState
                    {
                        Ids = { listSensorIds },
                        States = { listSensorStates },
                    };
                    await responseStream.WriteAsync(state);
                    await Task.Delay(TimeSpan.FromSeconds(1 / stateRequest.PublishFrequency), cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning(e);
            }
        }
    }

    public class FanControllerServiceImpl : FanControllerService.FanControllerServiceBase
    {
        public override Task<FansId> GetAllFansId(Google.Protobuf.WellKnownTypes.Empty empty, ServerCallContext context)
        {
            List<uint> ids = new List<uint>();
            List<string> names = new List<string>();

            for (int i = 0; i < reachy.fans.Length; i++)
            {
                ids.Add((uint)i);
                names.Add(reachy.fans[i].name);
            }

            FansId allIds = new FansId
            {
                Names = { names },
                Uids = { ids },
            };

            return Task.FromResult(allIds);
        }

        public override Task<FansState> GetFansState(FansStateRequest fanRequest, ServerCallContext context)
        {
            var fans = reachy.GetCurrentFansState(fanRequest.Ids);

            List<FanState> listFanStates = new List<FanState>();
            List<FanId> listFanIds = new List<FanId>();
            foreach (var item in fans)
            {
                var fanState = new FanState();
                fanState.On = item.fan_state;
                listFanStates.Add(fanState);
                listFanIds.Add(new FanId { Name = item.name });
            };

            FansState state = new FansState
            {
                Ids = { listFanIds },
                States = { listFanStates },
            };

            return Task.FromResult(state);
        }

        public override Task<FansCommandAck> SendFansCommands(FansCommand fansCommand, ServerCallContext context)
        {
            try
            {
                Dictionary<FanId, bool> commands = new Dictionary<FanId, bool>();
                for (int i = 0; i < fansCommand.Commands.Count; i++)
                {
                    commands.Add(fansCommand.Commands[i].Id, fansCommand.Commands[i].On);
                }
                reachy.HandleFanCommand(commands);
                return Task.FromResult(new FansCommandAck { Success = true });
            }
            catch
            {
                return Task.FromResult(new FansCommandAck { Success = false });
            }
        }
    }

    public class MobileBasePresenceServiceImpl : MobileBasePresenceService.MobileBasePresenceServiceBase
    {
        public override Task<MobileBasePresence> GetMobileBasePresence(Google.Protobuf.WellKnownTypes.Empty empty, ServerCallContext context)
        {
            MobileBasePresence mobility = new MobileBasePresence
            {
                Presence = false,
            };

            return Task.FromResult(mobility);
        }
    }
}
