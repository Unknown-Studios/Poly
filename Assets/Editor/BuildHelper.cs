using UnityEditor;
using UnityEngine;

public class BuildHelper : MonoBehaviour
{
    public static string[] levels = { "Assets/Scenes/Splash.unity", "Assets/Scenes/Login.unity", "Assets/Scenes/Project.unity", "Assets/Scenes/Game.unity", "Assets/Scenes/Server.unity" };

    public static void Build()
    {
        BuildWindows();
        BuildMac();
    }

    public static void BuildMac()
    {
        // Build player.
        BuildPipeline.BuildPlayer(levels, Application.dataPath + "/../Builds/Poly.app", BuildTarget.StandaloneOSXUniversal, BuildOptions.None);
    }

    public static void BuildWindows()
    {
        // Build player.
        BuildPipeline.BuildPlayer(levels, Application.dataPath + "/../Builds/Poly.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
    }
}