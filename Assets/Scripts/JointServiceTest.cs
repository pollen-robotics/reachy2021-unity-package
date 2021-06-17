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
    void Start()
    {
        reachy = GameObject.Find("Reachy").GetComponent<ReachyController>();
        RunHelloWorld();
    }

    void Update()
    {
     //   Debug.Log("update");
    }
    public static void RunHelloWorld()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        Debug.Log("==============================================================");
        Debug.Log("Starting tests");
        Debug.Log("==============================================================");

        gRPCServer();

        Debug.Log("==============================================================");
        Debug.Log("Tests finished successfully.");
        Debug.Log("==============================================================");
    }
    public static void gRPCServer()
    {
        const int PortJoint = 50055;
        Server server = new Server
        {
            Services = { 
                JointService.BindService(new JointServiceImpl()), 
                SensorService.BindService(new SensorServiceImpl()),
                FanControllerService.BindService(new FanControllerServiceImpl()) },
            Ports = { new ServerPort("localhost", PortJoint, ServerCredentials.Insecure) }
        };
        server.Start();

        // Channel channel = new Channel("127.0.0.1:50055", ChannelCredentials.Insecure);

        // var client = new JointService.JointServiceClient(channel);

        // SendJointsPositions(client);
        
        // StreamJointsRequest req = new StreamJointsRequest{
        //     Request = new JointsStateRequest { 
        //         Ids = { new JointId { Name = "l_arm_yaw"}},
        //         RequestedFields = { JointField.PresentPosition },
        //     },
        //     PublishFrequency = 1f,
        // };

        // AsyncServerStreamingCall<JointsState> call = client.StreamJointsState(req);
        // GetJointsPositions(call);

        //Debug.Log(client.GetAllJointsId(new Google.Protobuf.WellKnownTypes.Empty()));
        
        // server.ShutdownAsync().Wait();
    }

    public static async void GetJointsPositions(AsyncServerStreamingCall<JointsState> call)
    {
        
        while(await call.ResponseStream.MoveNext())
        {
            var reply = call.ResponseStream.Current;
            Debug.Log("I've got an answer");
            Debug.Log(reply.States[0].PresentPosition);
            await Task.Delay(1000);
        }
    }

    public static async void SendJointsPositions(JointService.JointServiceClient client)
    {
        while(true)
        {
            using (var call = client.StreamJointsCommands())
            {
                JointsCommand bodyCommand = new JointsCommand {
                    Commands = { 
                        new JointCommand { 
                            Id = new JointId { Name="l_arm_yaw" },
                            GoalPosition = 0.5f,
                            }
                        }
                };

                await call.RequestStream.WriteAsync(bodyCommand);
                await call.RequestStream.CompleteAsync();

                JointsCommandAck success = await call.ResponseAsync;
                Debug.Log(success.Success);
            }
        }
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
                    jointState.TorqueLimit = 0;
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
            FansId allIds = new FansId {
                Names = { },
                Uids = { },
            };

            return Task.FromResult(allIds);
        }

        public override Task<FansState> GetFansState(FansStateRequest request, ServerCallContext context)
        {
            FansState state = new FansState {
                Ids = { },
                States = { },
            };

            return Task.FromResult(state);
        }

        public override Task<FansCommandAck> SendFansCommands(FansCommand command, ServerCallContext context)
        {
            FansCommandAck ack = new FansCommandAck {
                Success = false,
            };

            return Task.FromResult(ack);
        }
    }

    public class OrbitaKinematicsImpl : OrbitaKinematics.OrbitaKinematicsBase
    {
        // public override Task<OrbitaIKSolution> ComputeOrbitaIK(OrbitaIKRequest ik_request, ServerCallContext context)
        // {
        //     FansId allIds = new FansId {
        //         Names = { },
        //         Uids = { },
        //     };

        //     return Task.FromResult(allIds);
        // }

        public override Task<Reachy.Sdk.Kinematics.Quaternion> GetQuaternionTransform(LookVector look_at_request, ServerCallContext context)
        {
            Vector3 vo = new Vector3(1, 0, 0);
            Vector3 vt = new Vector3((float)look_at_request.X, (float)look_at_request.Y, (float)look_at_request.Z);
            vt = vt.normalized;

            Vector3 v = Vector3.Cross(vo, vt);
            v = v.normalized;

            float alpha = Mthf.Acos(Vector3.Dot(vo, vt));
            if(float.IsNaN(alpha) || (alpha < 0.000001f))
            {
                Reachy.Sdk.Kinematics.Quaternion q = new Reachy.Sdk.Kinematics.Quaternion{
                    W = 0,
                    X = 1,
                    Y = 0,
                    Z = 0,
                };
                return Task.FromResult(q);
            }
            Reachy.Sdk.Kinematics.Quaternion q = new Reachy.Sdk.Kinematics.Quaternion{
                W = 0,
                X = 1,
                Y = 0,
                Z = 0,
            };
            return Task.FromResult(q);
        }
    }
}
