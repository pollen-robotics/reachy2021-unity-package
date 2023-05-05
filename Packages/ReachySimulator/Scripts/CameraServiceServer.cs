using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;
using Reachy;

using Grpc.Core;
using Grpc.Core.Utils;
using Reachy.Sdk.Camera;

class CameraServiceServer : MonoBehaviour
{
    public static ReachyController reachy;
    static Server server;

    private static CancellationTokenSource askForCancellation = new CancellationTokenSource();

    void Start()
    {
        reachy = GameObject.Find("Reachy").GetComponent<ReachyController>();
        gRPCServer();
    }

    public static void gRPCServer()
    {
        const int PortJoint = 50057;
        server = new Server(new List<ChannelOption>
        {
            new ChannelOption(ChannelOptions.MaxSendMessageLength, 250000),
        })
        {
            Services = { CameraService.BindService(new CameraServiceImpl()) },
            Ports = { new ServerPort("0.0.0.0", PortJoint, ServerCredentials.Insecure) }
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

    public class CameraServiceImpl : CameraService.CameraServiceBase
    {

        private ZoomLevel zoomLevelLeft = new ZoomLevel { Level = ZoomLevelPossibilities.Out };
        private ZoomLevel zoomLevelRight = new ZoomLevel { Level = ZoomLevelPossibilities.Out };
        public override Task<Image> GetImage(ImageRequest request, ServerCallContext context)
        {
            var state = reachy.GetCurrentView();
            byte[] image;
            if (request.Camera.Id == CameraId.Left)
            {
                image = state.left_eye;
            }
            else
            {
                if (state.right_eye == null)
                {
                    reachy.EnableRightImage();
                    image = state.left_eye;
                }
                else
                    image = state.right_eye;
            }

            return Task.FromResult(new Image { Data = Google.Protobuf.ByteString.CopyFrom(image) });
        }

        public override async Task StreamImage(StreamImageRequest imageRequest, Grpc.Core.IServerStreamWriter<Image> responseStream, ServerCallContext context)
        {
            CancellationToken cancellationToken = askForCancellation.Token;

            try
            {
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    var state = reachy.GetCurrentView();
                    byte[] image;
                    if (imageRequest.Request.Camera.Id == CameraId.Left)
                    {
                        image = state.left_eye;
                    }
                    else
                    {
                        if (state.right_eye == null)
                        {
                            reachy.EnableRightImage();
                            image = state.left_eye;
                        }
                        else
                            image = state.right_eye;
                    }

                    await responseStream.WriteAsync(new Image { Data = Google.Protobuf.ByteString.CopyFrom(image) });
                    await Task.Delay(30, context.CancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException e)
            {
                Debug.LogWarning(e);
            }
        }

        public override Task<ZoomCommandAck> SendZoomCommand(ZoomCommand zoomCommand, ServerCallContext context)
        {
            switch (zoomCommand.CommandCase)
            {
                case ZoomCommand.CommandOneofCase.None:
                    return Task.FromResult(new ZoomCommandAck { Success = false });
                case ZoomCommand.CommandOneofCase.HomingCommand:
                    return Task.FromResult(new ZoomCommandAck { Success = false });
                case ZoomCommand.CommandOneofCase.SpeedCommand:
                    return Task.FromResult(new ZoomCommandAck { Success = false });
                case ZoomCommand.CommandOneofCase.LevelCommand:
                    reachy.HandleCameraZoom(zoomCommand.Camera.Id, zoomCommand.LevelCommand.Level);
                    return Task.FromResult(new ZoomCommandAck { Success = true });
                default:
                    return Task.FromResult(new ZoomCommandAck { Success = false });
            }
        }

        public override Task<ZoomLevel> GetZoomLevel(Reachy.Sdk.Camera.Camera camera, ServerCallContext context)
        {
            ZoomLevel zoomLevel = reachy.GetCameraZoom(camera.Id);
            return Task.FromResult(zoomLevel);
        }

        public override Task<ZoomSpeed> GetZoomSpeed(Reachy.Sdk.Camera.Camera camera, ServerCallContext context)
        {
            return Task.FromResult(new ZoomSpeed { Speed = 0 });
        }
    }
}
