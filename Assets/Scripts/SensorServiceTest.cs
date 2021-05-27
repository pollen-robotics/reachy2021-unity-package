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

class SensorServiceTest : MonoBehaviour
{
    public static ReachyController reachy;

    void Start()
    {
        reachy = GameObject.Find("Reachy").GetComponent<ReachyController>();
        gRPCServer();
    }
    
    public static void gRPCServer()
    {
        const int PortJoint = 50055;
        Server server = new Server
        {
            Services = { SensorService.BindService(new SensorServiceImpl()) },
            Ports = { new ServerPort("localhost", PortJoint, ServerCredentials.Insecure) }
        };
        server.Start();

        Channel channel = new Channel("127.0.0.1:50055", ChannelCredentials.Insecure);

        var client = new SensorService.SensorServiceClient(channel);

        channel.ShutdownAsync().Wait();

        server.ShutdownAsync().Wait();
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
            List<string> sensorRequest = new List<string>();
            // switch(stateRequest.Ids.IdCase)
            // {
            //     case SensorId.IdOneofCase.Name:
            //         //sensorRequest = name2sensor[sensor.Name];
            //     case SensorId.IdOneofCase.Uid:
            //         //s = sensors[sensor];
            //     default:
            //         //s = null;
            // }

            // var sensors = reachy.GetCurrentSensorsState(stateRequest.Ids);
            
            List<SensorState> listSensorStates = new List<SensorState>();
            List<SensorId> listSensorIds = new List<SensorId>();
            foreach (var item in sensors)
            {
                var sensorState = new SensorState();
                sensorState.ForceSensorState.Force = item.sensor_state;


                listSensorStates.Add(sensorState);
                listSensorIds.Add(new SensorId { Name = item.name });
            };

            SensorsState state = new SensorsState {
                Ids = { listSensorIds },
                States = { listSensorStates },
            };

            return Task.FromResult(state);
        }

    //     // public override async Task StreamSensorStates(StreamSensorsStateRequest stateRequest, IServerStreamWriter<SensorsState> responseStream, ServerCallContext context)
    //     // {
    //     //     var responses = 
    //     //     await responseStream.WriteAsync(response);
    //     // }
    }
}
