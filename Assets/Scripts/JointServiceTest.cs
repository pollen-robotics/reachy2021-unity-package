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

class JointServiceTest : MonoBehaviour
{
    public static ReachyController reachy;
    void Start()
    {
        reachy = GameObject.Find("Reachy").GetComponent<ReachyController>();
        RunHelloWorld();
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
        // Debug.Log(client.GetAllJointsId(new Google.Protobuf.WellKnownTypes.Empty()));
        // server.ShutdownAsync().Wait();
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

        // public override Task<JointsCommandAck> StreamJointsCommands(IAsyncStreamReader<JointsCommand> requestStream, ServerCallContext context)
        // {
        //     await foreach (var message in requestStream.ReadAllAsync())
        //     {
        //         Dictionary<JointId, float> commands = new Dictionary<JointId, float>();
        //         for(int i=0; i<jointsCommand.Commands.Count; i++)
        //         {
        //             float command = Mathf.Rad2Deg * (float)jointsCommand.Commands[i].GoalPosition;
        //             commands.Add(jointsCommand.Commands[i].Id, command);
        //         }
        //         reachy.HandleCommand(commands);
        //     }
        //     return (new JointsCommandAck { Success = false });
        // }

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
                jointState.PresentPosition = item.present_position;

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
            while (!context.CancellationToken.IsCancellationRequested)
            {
                Dictionary<JointId, JointField> request = new Dictionary<JointId, JointField>();
                for(int i=0; i<jointRequest.Request.Ids.Count; i++)
                {
                    request.Add(jointRequest.Request.Ids[i], JointField.PresentPosition);
                }
                var motors = reachy.GetCurrentMotorsState(request);
                
                List<JointState> listJointStates = new List<JointState>();
                List<JointId> listJointIds = new List<JointId>();
                foreach (var item in motors)
                {
                    var jointState = new JointState();
                    jointState.Name = item.name;
                    jointState.PresentPosition = item.present_position;

                    listJointStates.Add(jointState);
                    listJointIds.Add(new JointId { Name = item.name });
                };

                JointsState state = new JointsState {
                    Ids = { listJointIds },
                    States = { listJointStates },
                };
                await responseStream.WriteAsync(state);
                await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
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

        // public override async Task StreamSensorStates(StreamSensorsStateRequest stateRequest, IServerStreamWriter<SensorsState> responseStream, ServerCallContext context)
        // {
        //     var responses = 
        //     await responseStream.WriteAsync(response);
        // }
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
}
