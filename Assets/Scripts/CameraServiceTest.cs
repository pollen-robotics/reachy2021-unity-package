using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System;
using Reachy;

using Grpc.Core;
using Grpc.Core.Utils;
using Reachy.Sdk.Camera;

class CameraServiceTest : MonoBehaviour
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
        const int PortJoint = 50057;
        Server server = new Server
        {
            Services = { CameraService.BindService(new CameraServiceImpl()) },
            Ports = { new ServerPort("localhost", PortJoint, ServerCredentials.Insecure) }
        };
        server.Start();

        Channel channel = new Channel("127.0.0.1:50055", ChannelCredentials.Insecure);

        var client = new CameraService.CameraServiceClient(channel);


        channel.ShutdownAsync().Wait();

        server.ShutdownAsync().Wait();
    }
    public class CameraServiceImpl : CameraService.CameraServiceBase
    {
        public override Task<Image> GetImage(ImageRequest request, ServerCallContext context)
        {
            var state = reachy.GetCurrentState();
            string image;
            if(request.Camera.Id == CameraId.Left)
            {
                image = state.left_eye;
            }
            else
            {
                image = state.right_eye;
            }

            return Task.FromResult(new Image { Data = Google.Protobuf.ByteString.FromBase64(image) });
        }

        public override Task<ZoomCommadAck> SendZoomCommand(ZoomCommand zoomCommand, ServerCallContext context)
        {
            Camera eye;
            if(zoomCommand.Camera.Id == CameraId.Left)
            {          
                foreach(Camera camera in Camera.allCameras)
                {
                    if(camera.name == "CameraLeft")
                    {
                        eye = camera;
                    }
                }
            }
            else
            {
                foreach(Camera camera in Camera.allCameras)
                {
                    if(camera.name == "CameraRight")
                    {
                        eye = camera;
                    }
                }
            }

            if(zoomCommand.Command.GetCaseFieldDescriptor() == ZoomLevelPossibilities)
            {
                if(zoomCommand.level_command == ZoomLevelPossibilities.In)
                {
                    eye.fieldOfView = 40.0f;
                }
                if(zoomCommand.level_command == ZoomLevelPossibilities.Inter)
                {
                    eye.fieldOfView = 60.0f;
                }
                if(zoomCommand.level_command == ZoomLevelPossibilities.Out)
                {
                    eye.fieldOfView = 80.0f;
                }
            }
            

            

            return Task.FromResult(new Image { Data = Google.Protobuf.ByteString.FromBase64(image) });
        }
    }
}
