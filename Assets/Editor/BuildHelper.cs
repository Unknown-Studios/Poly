using UnityEditor;
using UnityEngine;

public class BuildHelper : MonoBehaviour
{
    public static string[] levels = { "Assets/Scenes/Splash.unity", "Assets/Scenes/Login.unity", "Assets/Scenes/Project.unity", "Assets/Scenes/Game.unity", "Assets/Scenes/Server.unity" };

    public static void Build()
    {
        string[] strings = System.Environment.GetCommandLineArgs();
        string path = "";

        for (int i = 0; i < strings.Length; i++)
        {
            if (strings[i].Contains("BuildHelper.Build"))
            {
                path = strings[Mathf.Clamp(i + 1, 0, strings.Length)];
            }
        }

        if (path == "")
        {
            path = Application.dataPath + "/../Builds/";
        }

        BuildWindows(path);
        BuildMac(path);
    }

    public static void BuildMac(string Path)
    {
        // Build player.
        BuildPipeline.BuildPlayer(levels, Path + "Poly.app", BuildTarget.StandaloneOSXUniversal, BuildOptions.None);
    }

    public static void BuildWindows(string Path)
    {
        // Build player.
        BuildPipeline.BuildPlayer(levels, Path + "Poly.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
    }
}