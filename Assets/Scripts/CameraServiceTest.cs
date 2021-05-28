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
        gRPCServer();
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

        // server.ShutdownAsync().Wait();
    }
    public class CameraServiceImpl : CameraService.CameraServiceBase
    {

        private ZoomLevel zoomLevelLeft = new ZoomLevel{ Level = ZoomLevelPossibilities.Out };
        private ZoomLevel zoomLevelRight = new ZoomLevel{ Level = ZoomLevelPossibilities.Out };
        public override Task<Image> GetImage(ImageRequest request, ServerCallContext context)
        {
            var state = reachy.GetCurrentView();
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

        public override Task<ZoomCommandAck> SendZoomCommand(ZoomCommand zoomCommand, ServerCallContext context)
        {
            UnityEngine.Camera eye;
            switch(zoomCommand.CommandCase)
            {
                case ZoomCommand.CommandOneofCase.None:
                    return Task.FromResult(new ZoomCommandAck { Success = false });
                case ZoomCommand.CommandOneofCase.HomingCommand:
                    return Task.FromResult(new ZoomCommandAck { Success = false });
                case ZoomCommand.CommandOneofCase.SpeedCommand:
                    return Task.FromResult(new ZoomCommandAck { Success = false });
                case ZoomCommand.CommandOneofCase.LevelCommand:
                    if(zoomCommand.Camera.Id == CameraId.Left)
                    {          
                        foreach(UnityEngine.Camera camera in UnityEngine.Camera.allCameras)
                        {
                            if(camera.name == "Left Camera")
                            {
                                eye = camera;

                                if(zoomCommand.LevelCommand.Level == ZoomLevelPossibilities.In)
                                {
                                    eye.fieldOfView = 40.0f;
                                    this.zoomLevelLeft.Level = ZoomLevelPossibilities.In;
                                }
                                if(zoomCommand.LevelCommand.Level == ZoomLevelPossibilities.Inter)
                                {
                                    eye.fieldOfView = 70.0f;
                                    zoomLevelLeft.Level = ZoomLevelPossibilities.Inter;
                                }
                                if(zoomCommand.LevelCommand.Level == ZoomLevelPossibilities.Out)
                                {
                                    eye.fieldOfView = 100.0f;
                                    zoomLevelLeft.Level = ZoomLevelPossibilities.Out;
                                }
                            }                        
                        }
                    }
                    else
                    {
                        foreach(UnityEngine.Camera camera in UnityEngine.Camera.allCameras)
                        {
                            if(camera.name == "Right Camera")
                            {
                                eye = camera;

                                if(zoomCommand.LevelCommand.Level == ZoomLevelPossibilities.In)
                                {
                                    eye.fieldOfView = 40.0f;
                                    zoomLevelRight.Level = ZoomLevelPossibilities.In;
                                }
                                if(zoomCommand.LevelCommand.Level == ZoomLevelPossibilities.Inter)
                                {
                                    eye.fieldOfView = 70.0f;
                                    zoomLevelRight.Level = ZoomLevelPossibilities.Inter;
                                }
                                if(zoomCommand.LevelCommand.Level == ZoomLevelPossibilities.Out)
                                {
                                    eye.fieldOfView = 100.0f;
                                    zoomLevelRight.Level = ZoomLevelPossibilities.Out;
                                }
                            }
                        }
                    }
                    return Task.FromResult(new ZoomCommandAck { Success = true });
                default:
                    return Task.FromResult(new ZoomCommandAck { Success = false });
            }
        }

        public override Task<ZoomLevel> GetZoomLevel(Reachy.Sdk.Camera.Camera camera, ServerCallContext context)
        {
            if(camera.Id == CameraId.Left)
            {
                foreach(UnityEngine.Camera cam in UnityEngine.Camera.allCameras)
                {
                    if(cam.name == "Left Camera")
                    {
                        return Task.FromResult(zoomLevelLeft);
                    }
                }
                foreach(UnityEngine.Camera cam in UnityEngine.Camera.allCameras)
                {
                    if(cam.name == "Right Camera")
                    {
                        return Task.FromResult(zoomLevelRight);
                    }
                }
            }
            return Task.FromResult(new ZoomLevel { Level = ZoomLevelPossibilities.Zero });
        }

        public override Task<ZoomSpeed> GetZoomSpeed(Reachy.Sdk.Camera.Camera camera, ServerCallContext context)
        {
            return Task.FromResult(new ZoomSpeed { Speed = 0 });
        }
    }
}
