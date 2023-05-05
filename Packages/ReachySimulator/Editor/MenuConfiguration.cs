using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class MenuConfiguration : Editor
{

    private static DownloadTask _taskManager = new DownloadTask();

    [MenuItem("Pollen Robotics/Install GRPC")]
    static void InstallGRPC()
    {
        DownloadTask.RunTask();
    }
}
#endif