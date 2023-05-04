using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Net;
using System.IO.Compression;

#if UNITY_EDITOR
public class DownloadTask
{
    private static string savePath;
    private static string GRPC_VERSION = "https://packages.grpc.io/archive/2022/04/67538122780f8a081c774b66884289335c290cbe-f15a2c1c-582b-4c51-acf2-ab6d711d2c59/csharp/grpc_unity_package.2.47.0-dev202204190851.zip";

    public static async void RunTask()
    {
        Debug.Log("Downloading GRPC...");
        savePath = string.Format("{0}/{1}.zip", Application.temporaryCachePath, "grpc");
        Task task = _Download();
        await task;
        Debug.Log("Installing GRPC...");
        task = _Unzip();
        await task;
        AssetDatabase.Refresh();
        Debug.Log("GRPC installed.");
    }

    private static async Task _Download()
    {
        WebClient Client = new WebClient ();
        await Task.Run(() => Client.DownloadFile(GRPC_VERSION, savePath));
    }

    private static async Task _Unzip()
    {
        await Task.Run(() => ZipFile.ExtractToDirectory(savePath, @"Assets/"));
    }
}
#endif