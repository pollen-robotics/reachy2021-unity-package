using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grpc.Core;
using Reachy.Sdk.Joint;
using Reachy.Sdk.Kinematics;

public class ResetRobot : MonoBehaviour
{

    private Channel channel;
    private JointService.JointServiceClient client;
    private FullBodyCartesianCommandService.FullBodyCartesianCommandServiceClient clientCartesian;

    // Start is called before the first frame update
    void Start()
    {
        channel = new Channel("localhost:50055", ChannelCredentials.Insecure);
        client = new JointService.JointServiceClient(channel);
        clientCartesian = new FullBodyCartesianCommandService.FullBodyCartesianCommandServiceClient(channel);
    }

    public void ResetAll()
    {        
        OrbitaIKRequest headTarget = new OrbitaIKRequest {
            Q = new Reachy.Sdk.Kinematics.Quaternion {
                W = 1.0,
                X = 0.0,
                Y = 0.0,
                Z = 0.0,
            }
        };

        FullBodyCartesianCommand neckCommand = new FullBodyCartesianCommand {
            Neck = headTarget,
        };

        SendBodyCommand(neckCommand);

        JointsCommand jointCommands = new JointsCommand {
            Commands = {
                new JointCommand { Id = new JointId { Name = "r_shoulder_pitch" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "r_shoulder_roll" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "r_arm_yaw" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "r_elbow_pitch" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "r_forearm_yaw" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "r_wrist_pitch" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "r_wrist_roll" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "r_gripper" }, GoalPosition = 0.0f, },

                new JointCommand { Id = new JointId { Name = "l_shoulder_pitch" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "l_shoulder_roll" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "l_arm_yaw" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "l_elbow_pitch" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "l_forearm_yaw" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "l_wrist_pitch" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "l_wrist_roll" }, GoalPosition = 0.0f, },
                new JointCommand { Id = new JointId { Name = "l_gripper" }, GoalPosition = 0.0f, },
            },
        };

        SendJointsCommand(jointCommands);

    }

    public async void SendJointsCommand(JointsCommand jointsCommand)
    {
        try
        {
            await client.SendJointsCommandsAsync(jointsCommand);
        }
        catch (RpcException e)
        {
            Debug.Log("Communication RPC failed: in SendJointsPositions():" + e);
        }
    }

    public async void SendBodyCommand(FullBodyCartesianCommand command)
    {
        try
        {
            await clientCartesian.SendFullBodyCartesianCommandsAsync(command);
        }
        catch (RpcException e)
        {
            Debug.Log("Communication RPC failed: in SendBodyCommand():" + e);
        }
    }
}
