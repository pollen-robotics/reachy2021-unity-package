using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System;
using Reachy;

using Grpc.Core;
using Grpc.Core.Utils;
using Reachy.Sdk.Joint;
using Reachy.Sdk.Fan;
using Reachy.Sdk.Kinematics;

class JointServiceTest : MonoBehaviour
{
    public static ReachyController reachy;
    static Server server;

    void Start()
    {
        reachy = GameObject.Find("Reachy").GetComponent<ReachyController>();
        gRPCServer();
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
                },
            Ports = { new ServerPort("localhost", PortJoint, ServerCredentials.Insecure) }
        };
        server.Start();
    }

    void OnDestroy()
    {
        server.ShutdownAsync().Wait();
    }
    
    public class JointServiceImpl : JointService.JointServiceBase
    {
        public override Task<JointsCommandAck> SendJointsCommands(JointsCommand jointsCommand, ServerCallContext context)
        {
            try
            {
                Dictionary<JointId, float> commands = new Dictionary<JointId, float>();
                for(int i=0; i<jointsCommand.Commands.Count; i++)
                {
                    float command = Mathf.Rad2Deg * (float)jointsCommand.Commands[i].GoalPosition;
                    commands.Add(jointsCommand.Commands[i].Id, command);
                }
                reachy.HandleCommand(commands);
                return Task.FromResult(new JointsCommandAck { Success = true });
            }
            catch
            {
                return Task.FromResult(new JointsCommandAck { Success = false });
            }
        }

        public override async Task<JointsCommandAck> StreamJointsCommands(IAsyncStreamReader<JointsCommand> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var jointsCommand = requestStream.Current;
                await SendJointsCommands(jointsCommand, context);
            }
            return (new JointsCommandAck { Success = true });
        }


        public override Task<JointsState> GetJointsState(JointsStateRequest jointRequest, ServerCallContext context)
        {
            Dictionary<JointId, JointField> request = new Dictionary<JointId, JointField>();

            for(int i=0; i<jointRequest.Ids.Count; i++)
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
                if(jointRequest.RequestedFields.Contains(JointField.PresentPosition))
                {
                    jointState.PresentPosition = item.present_position;
                }
                if(jointRequest.RequestedFields.Contains(JointField.PresentSpeed))
                {
                    jointState.PresentSpeed = 0;
                }
                if(jointRequest.RequestedFields.Contains(JointField.PresentLoad))
                {
                    jointState.PresentLoad = 0;
                }
                if(jointRequest.RequestedFields.Contains(JointField.Temperature))
                {
                    jointState.Temperature = 0;
                }
                if(jointRequest.RequestedFields.Contains(JointField.Compliant))
                {
                    jointState.Compliant = false;
                }
                if(jointRequest.RequestedFields.Contains(JointField.GoalPosition))
                {
                    jointState.GoalPosition = item.goal_position;
                }
                if(jointRequest.RequestedFields.Contains(JointField.SpeedLimit))
                {
                    jointState.SpeedLimit = 0;
                }
                if(jointRequest.RequestedFields.Contains(JointField.TorqueLimit))
                {
                    jointState.TorqueLimit = 100;
                }
                if(jointRequest.RequestedFields.Contains(JointField.Pid))
                {
                    jointState.Pid = new PIDValue { Pid = new PIDGains { P = 0, I = 0, D = 0 }};
                }
                if(jointRequest.RequestedFields.Contains(JointField.All))
                {
                    jointState.PresentPosition = item.present_position;
                    jointState.PresentSpeed = 0;
                    jointState.PresentLoad = 0;
                    jointState.Temperature = 0;
                    jointState.Compliant = false;
                    jointState.GoalPosition = item.goal_position;
                    jointState.SpeedLimit = 0;
                    jointState.TorqueLimit = 100;
                    jointState.Pid = new PIDValue { Pid = new PIDGains { P = 0, I = 0, D = 0 }};
                }

                listJointStates.Add(jointState);
                listJointIds.Add(new JointId { Name = item.name });
            };

            JointsState state = new JointsState {
                Ids = { listJointIds },
                States = { listJointStates },
            };

            return Task.FromResult(state);
        }

        public override async Task StreamJointsState(StreamJointsRequest jointRequest, Grpc.Core.IServerStreamWriter<JointsState> responseStream, ServerCallContext context)
        {
            Dictionary<JointId, JointField> request = new Dictionary<JointId, JointField>();
            for(int i=0; i<jointRequest.Request.Ids.Count; i++)
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
                    if(jointRequest.Request.RequestedFields.Contains(JointField.PresentPosition))
                    {
                        jointState.PresentPosition = item.present_position;
                    }
                    if(jointRequest.Request.RequestedFields.Contains(JointField.PresentSpeed))
                    {
                        jointState.PresentSpeed = 0;
                    }
                    if(jointRequest.Request.RequestedFields.Contains(JointField.PresentLoad))
                    {
                        jointState.PresentLoad = 0;
                    }
                    if(jointRequest.Request.RequestedFields.Contains(JointField.Temperature))
                    {
                        jointState.Temperature = 0;
                    }
                    if(jointRequest.Request.RequestedFields.Contains(JointField.Compliant))
                    {
                        jointState.Compliant = false;
                    }
                    if(jointRequest.Request.RequestedFields.Contains(JointField.GoalPosition))
                    {
                        jointState.GoalPosition = item.goal_position;
                    }
                    if(jointRequest.Request.RequestedFields.Contains(JointField.SpeedLimit))
                    {
                        jointState.SpeedLimit = 0;
                    }
                    if(jointRequest.Request.RequestedFields.Contains(JointField.TorqueLimit))
                    {
                        jointState.TorqueLimit = 0;
                    }
                    if(jointRequest.Request.RequestedFields.Contains(JointField.Pid))
                    {
                        jointState.Pid = new PIDValue { Pid = new PIDGains { P = 0, I = 0, D = 0 }};
                    }
                    if(jointRequest.Request.RequestedFields.Contains(JointField.All))
                    {
                        jointState.PresentPosition = item.present_position;
                        jointState.PresentSpeed = 0;
                        jointState.PresentLoad = 0;
                        jointState.Temperature = 0;
                        jointState.Compliant = false;
                        jointState.GoalPosition = item.goal_position;
                        jointState.SpeedLimit = 0;
                        jointState.TorqueLimit = 0;
                        jointState.Pid = new PIDValue { Pid = new PIDGains { P = 0, I = 0, D = 0 }};
                    }

                    listJointStates.Add(jointState);
                    listJointIds.Add(new JointId { Name = item.name });
                };

                JointsState state = new JointsState {
                    Ids = { listJointIds },
                    States = { listJointStates },
                };
                await responseStream.WriteAsync(state);
                await Task.Delay(TimeSpan.FromSeconds(1/jointRequest.PublishFrequency), context.CancellationToken);
            }
        }

        public override Task<JointsId> GetAllJointsId(Google.Protobuf.WellKnownTypes.Empty empty, ServerCallContext context)
        {
            List<uint> ids = new List<uint>();
            List<string> names = new List<string>();

            for(int i = 0; i< reachy.motors.Length; i++)
            {
                ids.Add((uint)i);
                names.Add(reachy.motors[i].name);
            }

            JointsId allIds = new JointsId {
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
                Task<ArmIKSolution> leftArmTask = armKinematics.ComputeArmIK(fullBodyCartesianCommand.LeftArm, context);
                Task<ArmIKSolution> rightArmTask = armKinematics.ComputeArmIK(fullBodyCartesianCommand.RightArm, context);
                ArmIKSolution leftArmSolution = leftArmTask.Result;
                ArmIKSolution rightArmSolution = rightArmTask.Result;
                UnityEngine.Quaternion headRotation= new UnityEngine.Quaternion((float)fullBodyCartesianCommand.Neck.Q.X, 
                    (float)fullBodyCartesianCommand.Neck.Q.Y, 
                    -(float)fullBodyCartesianCommand.Neck.Q.Z, 
                    (float)fullBodyCartesianCommand.Neck.Q.W);

                JointServiceImpl jointService = new JointServiceImpl();
                List<JointCommand> jointCommandList = new List<JointCommand>();

                int i = 0;
                foreach(var item in leftArmSolution.ArmPosition.Positions.Ids)
                {
                    jointCommandList.Add(new JointCommand {
                        Id = item,
                        GoalPosition = (float?)leftArmSolution.ArmPosition.Positions.Positions[i],
                    });
                    i += 1;
                }

                JointsCommand jointsCommand = new JointsCommand { Commands = { jointCommandList } };

                jointService.SendJointsCommands(jointsCommand, context);

                reachy.HandleHeadOrientation(headRotation);
                return Task.FromResult(new FullBodyCartesianCommandAck { 
                    LeftArmCommandSuccess = false,
                    RightArmCommandSuccess = false,
                    NeckCommandSuccess = false
                });
            }
            catch
            {
                return Task.FromResult(new FullBodyCartesianCommandAck { 
                    LeftArmCommandSuccess = false,
                    RightArmCommandSuccess = false,
                    NeckCommandSuccess = false
                });
            }
        }

        public override async Task<FullBodyCartesianCommandAck> StreamFullBodyCartesianCommands(IAsyncStreamReader<FullBodyCartesianCommand> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var fullBodyCartesianCommand = requestStream.Current;
                await SendFullBodyCartesianCommands(fullBodyCartesianCommand, context);
            }
            return (new FullBodyCartesianCommandAck { 
                    LeftArmCommandSuccess = false,
                    RightArmCommandSuccess = false,
                    NeckCommandSuccess = false
                });
        }
    }

    public class ArmKinematicsImpl : ArmKinematics.ArmKinematicsBase
    {
        public override Task<ArmFKSolution> ComputeArmFK(ArmFKRequest fkRequest, ServerCallContext context)
        {
            ArmFKSolution sol = new ArmFKSolution {
                Success = false,
                EndEffector = new ArmEndEffector { 
                    Side = fkRequest.ArmPosition.Side,
                    Pose = new Reachy.Sdk.Kinematics.Matrix4x4 { Data = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0} },
                },
            };

            return Task.FromResult(sol);
        }

        public override Task<ArmIKSolution> ComputeArmIK(ArmIKRequest ikRequest, ServerCallContext context)
        {
            ArmIKSolution sol = new ArmIKSolution {
                Success = false,
                ArmPosition = new ArmJointPosition { 
                    Side = ikRequest.Target.Side,
                    Positions = new JointPosition { 
                        Ids = { new Reachy.Sdk.Joint.JointId {} },
                        Positions = {},
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

            for(int i = 0; i< reachy.sensors.Length; i++)
            {
                ids.Add((uint)i);
                names.Add(reachy.sensors[i].name);
            }

            SensorsId allIds = new SensorsId {
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
                sensorState.ForceSensorState = new ForceSensorState{ Force = item.sensor_state };
                listSensorStates.Add(sensorState);
                listSensorIds.Add(new SensorId { Name = item.name });
            };

            SensorsState state = new SensorsState {
                Ids = { listSensorIds },
                States = { listSensorStates },
            };

            return Task.FromResult(state);
        }

        public override async Task StreamSensorStates(StreamSensorsStateRequest stateRequest, IServerStreamWriter<SensorsState> responseStream, ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var sensors = reachy.GetCurrentSensorsState(stateRequest.Request.Ids);
            
                List<SensorState> listSensorStates = new List<SensorState>();
                List<SensorId> listSensorIds = new List<SensorId>();
                foreach (var item in sensors)
                {
                    var sensorState = new SensorState();
                    sensorState.ForceSensorState = new ForceSensorState{ Force = item.sensor_state };
                    listSensorStates.Add(sensorState);
                    listSensorIds.Add(new SensorId { Name = item.name });
                };

                SensorsState state = new SensorsState {
                    Ids = { listSensorIds },
                    States = { listSensorStates },
                };
                await responseStream.WriteAsync(state);
                await Task.Delay(TimeSpan.FromSeconds(1/stateRequest.PublishFrequency), context.CancellationToken);
            }
        }
    }

    public class FanControllerServiceImpl : FanControllerService.FanControllerServiceBase
    {
        public override Task<FansId> GetAllFansId(Google.Protobuf.WellKnownTypes.Empty empty, ServerCallContext context)
        {
            List<uint> ids = new List<uint>();
            List<string> names = new List<string>();

            for(int i = 0; i< reachy.fans.Length; i++)
            {
                ids.Add((uint)i);
                names.Add(reachy.fans[i].name);
            }

            FansId allIds = new FansId {
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

            FansState state = new FansState {
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
                for(int i=0; i<fansCommand.Commands.Count; i++)
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
}
