// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: camera_reachy.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Reachy.Sdk.Camera {
  public static partial class CameraService
  {
    static readonly string __ServiceName = "reachy.sdk.camera.CameraService";

    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    static readonly grpc::Marshaller<global::Reachy.Sdk.Camera.ImageRequest> __Marshaller_reachy_sdk_camera_ImageRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Camera.ImageRequest.Parser));
    static readonly grpc::Marshaller<global::Reachy.Sdk.Camera.Image> __Marshaller_reachy_sdk_camera_Image = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Camera.Image.Parser));
    static readonly grpc::Marshaller<global::Reachy.Sdk.Camera.StreamImageRequest> __Marshaller_reachy_sdk_camera_StreamImageRequest = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Camera.StreamImageRequest.Parser));
    static readonly grpc::Marshaller<global::Reachy.Sdk.Camera.Camera> __Marshaller_reachy_sdk_camera_Camera = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Camera.Camera.Parser));
    static readonly grpc::Marshaller<global::Reachy.Sdk.Camera.ZoomLevel> __Marshaller_reachy_sdk_camera_ZoomLevel = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Camera.ZoomLevel.Parser));
    static readonly grpc::Marshaller<global::Reachy.Sdk.Camera.ZoomSpeed> __Marshaller_reachy_sdk_camera_ZoomSpeed = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Camera.ZoomSpeed.Parser));
    static readonly grpc::Marshaller<global::Reachy.Sdk.Camera.ZoomCommand> __Marshaller_reachy_sdk_camera_ZoomCommand = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Camera.ZoomCommand.Parser));
    static readonly grpc::Marshaller<global::Reachy.Sdk.Camera.ZoomCommandAck> __Marshaller_reachy_sdk_camera_ZoomCommandAck = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Reachy.Sdk.Camera.ZoomCommandAck.Parser));

    static readonly grpc::Method<global::Reachy.Sdk.Camera.ImageRequest, global::Reachy.Sdk.Camera.Image> __Method_GetImage = new grpc::Method<global::Reachy.Sdk.Camera.ImageRequest, global::Reachy.Sdk.Camera.Image>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetImage",
        __Marshaller_reachy_sdk_camera_ImageRequest,
        __Marshaller_reachy_sdk_camera_Image);

    static readonly grpc::Method<global::Reachy.Sdk.Camera.StreamImageRequest, global::Reachy.Sdk.Camera.Image> __Method_StreamImage = new grpc::Method<global::Reachy.Sdk.Camera.StreamImageRequest, global::Reachy.Sdk.Camera.Image>(
        grpc::MethodType.ServerStreaming,
        __ServiceName,
        "StreamImage",
        __Marshaller_reachy_sdk_camera_StreamImageRequest,
        __Marshaller_reachy_sdk_camera_Image);

    static readonly grpc::Method<global::Reachy.Sdk.Camera.Camera, global::Reachy.Sdk.Camera.ZoomLevel> __Method_GetZoomLevel = new grpc::Method<global::Reachy.Sdk.Camera.Camera, global::Reachy.Sdk.Camera.ZoomLevel>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetZoomLevel",
        __Marshaller_reachy_sdk_camera_Camera,
        __Marshaller_reachy_sdk_camera_ZoomLevel);

    static readonly grpc::Method<global::Reachy.Sdk.Camera.Camera, global::Reachy.Sdk.Camera.ZoomSpeed> __Method_GetZoomSpeed = new grpc::Method<global::Reachy.Sdk.Camera.Camera, global::Reachy.Sdk.Camera.ZoomSpeed>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetZoomSpeed",
        __Marshaller_reachy_sdk_camera_Camera,
        __Marshaller_reachy_sdk_camera_ZoomSpeed);

    static readonly grpc::Method<global::Reachy.Sdk.Camera.ZoomCommand, global::Reachy.Sdk.Camera.ZoomCommandAck> __Method_SendZoomCommand = new grpc::Method<global::Reachy.Sdk.Camera.ZoomCommand, global::Reachy.Sdk.Camera.ZoomCommandAck>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SendZoomCommand",
        __Marshaller_reachy_sdk_camera_ZoomCommand,
        __Marshaller_reachy_sdk_camera_ZoomCommandAck);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Reachy.Sdk.Camera.CameraReachyReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of CameraService</summary>
    [grpc::BindServiceMethod(typeof(CameraService), "BindService")]
    public abstract partial class CameraServiceBase
    {
      public virtual global::System.Threading.Tasks.Task<global::Reachy.Sdk.Camera.Image> GetImage(global::Reachy.Sdk.Camera.ImageRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task StreamImage(global::Reachy.Sdk.Camera.StreamImageRequest request, grpc::IServerStreamWriter<global::Reachy.Sdk.Camera.Image> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Reachy.Sdk.Camera.ZoomLevel> GetZoomLevel(global::Reachy.Sdk.Camera.Camera request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Reachy.Sdk.Camera.ZoomSpeed> GetZoomSpeed(global::Reachy.Sdk.Camera.Camera request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Reachy.Sdk.Camera.ZoomCommandAck> SendZoomCommand(global::Reachy.Sdk.Camera.ZoomCommand request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for CameraService</summary>
    public partial class CameraServiceClient : grpc::ClientBase<CameraServiceClient>
    {
      /// <summary>Creates a new client for CameraService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public CameraServiceClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for CameraService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public CameraServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected CameraServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected CameraServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::Reachy.Sdk.Camera.Image GetImage(global::Reachy.Sdk.Camera.ImageRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetImage(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Reachy.Sdk.Camera.Image GetImage(global::Reachy.Sdk.Camera.ImageRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetImage, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Camera.Image> GetImageAsync(global::Reachy.Sdk.Camera.ImageRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetImageAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Camera.Image> GetImageAsync(global::Reachy.Sdk.Camera.ImageRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetImage, null, options, request);
      }
      public virtual grpc::AsyncServerStreamingCall<global::Reachy.Sdk.Camera.Image> StreamImage(global::Reachy.Sdk.Camera.StreamImageRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return StreamImage(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncServerStreamingCall<global::Reachy.Sdk.Camera.Image> StreamImage(global::Reachy.Sdk.Camera.StreamImageRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncServerStreamingCall(__Method_StreamImage, null, options, request);
      }
      public virtual global::Reachy.Sdk.Camera.ZoomLevel GetZoomLevel(global::Reachy.Sdk.Camera.Camera request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetZoomLevel(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Reachy.Sdk.Camera.ZoomLevel GetZoomLevel(global::Reachy.Sdk.Camera.Camera request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetZoomLevel, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Camera.ZoomLevel> GetZoomLevelAsync(global::Reachy.Sdk.Camera.Camera request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetZoomLevelAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Camera.ZoomLevel> GetZoomLevelAsync(global::Reachy.Sdk.Camera.Camera request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetZoomLevel, null, options, request);
      }
      public virtual global::Reachy.Sdk.Camera.ZoomSpeed GetZoomSpeed(global::Reachy.Sdk.Camera.Camera request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetZoomSpeed(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Reachy.Sdk.Camera.ZoomSpeed GetZoomSpeed(global::Reachy.Sdk.Camera.Camera request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetZoomSpeed, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Camera.ZoomSpeed> GetZoomSpeedAsync(global::Reachy.Sdk.Camera.Camera request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetZoomSpeedAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Camera.ZoomSpeed> GetZoomSpeedAsync(global::Reachy.Sdk.Camera.Camera request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetZoomSpeed, null, options, request);
      }
      public virtual global::Reachy.Sdk.Camera.ZoomCommandAck SendZoomCommand(global::Reachy.Sdk.Camera.ZoomCommand request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SendZoomCommand(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Reachy.Sdk.Camera.ZoomCommandAck SendZoomCommand(global::Reachy.Sdk.Camera.ZoomCommand request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SendZoomCommand, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Camera.ZoomCommandAck> SendZoomCommandAsync(global::Reachy.Sdk.Camera.ZoomCommand request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SendZoomCommandAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Reachy.Sdk.Camera.ZoomCommandAck> SendZoomCommandAsync(global::Reachy.Sdk.Camera.ZoomCommand request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SendZoomCommand, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override CameraServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new CameraServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(CameraServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_GetImage, serviceImpl.GetImage)
          .AddMethod(__Method_StreamImage, serviceImpl.StreamImage)
          .AddMethod(__Method_GetZoomLevel, serviceImpl.GetZoomLevel)
          .AddMethod(__Method_GetZoomSpeed, serviceImpl.GetZoomSpeed)
          .AddMethod(__Method_SendZoomCommand, serviceImpl.SendZoomCommand).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static void BindService(grpc::ServiceBinderBase serviceBinder, CameraServiceBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_GetImage, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Reachy.Sdk.Camera.ImageRequest, global::Reachy.Sdk.Camera.Image>(serviceImpl.GetImage));
      serviceBinder.AddMethod(__Method_StreamImage, serviceImpl == null ? null : new grpc::ServerStreamingServerMethod<global::Reachy.Sdk.Camera.StreamImageRequest, global::Reachy.Sdk.Camera.Image>(serviceImpl.StreamImage));
      serviceBinder.AddMethod(__Method_GetZoomLevel, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Reachy.Sdk.Camera.Camera, global::Reachy.Sdk.Camera.ZoomLevel>(serviceImpl.GetZoomLevel));
      serviceBinder.AddMethod(__Method_GetZoomSpeed, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Reachy.Sdk.Camera.Camera, global::Reachy.Sdk.Camera.ZoomSpeed>(serviceImpl.GetZoomSpeed));
      serviceBinder.AddMethod(__Method_SendZoomCommand, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Reachy.Sdk.Camera.ZoomCommand, global::Reachy.Sdk.Camera.ZoomCommandAck>(serviceImpl.SendZoomCommand));
    }

  }
}
#endregion
