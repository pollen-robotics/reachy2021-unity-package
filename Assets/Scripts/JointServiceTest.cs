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

        var reply = gRPCServer();
        Debug.Log(reply.Success);

        Debug.Log("==============================================================");
        Debug.Log("Tests finished successfully.");
        Debug.Log("==============================================================");
    }
    public static JointsCommandAck gRPCServer()
    {
        const int PortJoint = 50055;
        Server server = new Server
        {
            Services = { JointService.BindService(new JointServiceImpl()) },
            Ports = { new ServerPort("localhost", PortJoint, ServerCredentials.Insecure) }
        };
        server.Start();

        Channel channel = new Channel("127.0.0.1:50055", ChannelCredentials.Insecure);

        var client = new JointService.JointServiceClient(channel);

        var reply = client.SendJointsCommands(new JointsCommand {
            Commands = {
                new JointCommand { Id=new JointId { Name = "l_shoulder_pitch" }, GoalPosition=Mathf.Deg2Rad*(70) },
                new JointCommand { Id=new JointId { Name = "l_elbow_pitch" }, GoalPosition=Mathf.Deg2Rad*(-90) },
                }
        });

        Debug.Log(client.GetAllJointsId(new Google.Protobuf.WellKnownTypes.Empty()));

        Debug.Log(client.GetJointsState(new JointsStateRequest {
            Ids = { 
                new JointId { Name = "l_shoulder_pitch" },
                new JointId { Name = "l_elbow_pitch" },
                }
        }));

        channel.ShutdownAsync().Wait();

        server.ShutdownAsync().Wait();

        return reply;
    }
    
    public class JointServiceImpl : JointService.JointServiceBase
    {
        public override Task<JointsCommandAck> SendJointsCommands(JointsCommand jointsCommand, ServerCallContext context)
        {
            try
            {
                Dictionary<string, float> commands = new Dictionary<string, float>();
                for(int i=0; i<jointsCommand.Commands.Count; i++)
                {
                    float command = Mathf.Rad2Deg * (float)jointsCommand.Commands[i].GoalPosition;
                    commands.Add(jointsCommand.Commands[i].Id.Name, command);
                }
                reachy.HandleCommand(commands);
                return Task.FromResult(new JointsCommandAck { Success = true });
            }
            catch
            {
                return Task.FromResult(new JointsCommandAck { Success = false });
            }
        }

        public override Task<JointsState> GetJointsState(JointsStateRequest jointRequest, ServerCallContext context)
        {
            Dictionary<string, JointField> request = new Dictionary<string, JointField>();
            for(int i=0; i<jointRequest.Ids.Count; i++)
            {
                request.Add(jointRequest.Ids[i].Name, JointField.PresentPosition);
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

            return Task.FromResult(state);
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
}
