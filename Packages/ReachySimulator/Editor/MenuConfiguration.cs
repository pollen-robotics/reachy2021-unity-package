using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuConfiguration : Editor
{

    private static DownloadTask _taskManager = new DownloadTask();

    [MenuItem("Reachy/Install GRPC")]
    static void InstallGRPC()
    {
        DownloadTask.RunTask();
    }
}
