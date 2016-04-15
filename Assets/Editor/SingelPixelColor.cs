using System.IO;
using UnityEditor;
using UnityEngine;

public class SinglePixelColor : EditorWindow
{
    private Color color;
    private string error = "";

    public static void CloseWindow()
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(SinglePixelColor));
        window.Close();
    }

    [MenuItem("Window/Single Pixel Color")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SinglePixelColor));
    }

    private void CreateColorPicture(Color color)
    {
        if (!File.Exists(Application.dataPath + "/Textures/1x1 " + (color.r * 255).ToString() + "," + (color.g * 255).ToString() + "," + (color.b * 255).ToString() + ".png"))
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(1, 1, color);
            tex.Apply();
            if (!Directory.Exists(Application.dataPath + "/Textures/"))
            {
                Directory.CreateDirectory(Application.dataPath + "/Textures/");
            }
            byte[] bytes = tex.EncodeToPNG();
            DestroyImmediate(tex);
            File.WriteAllBytes(Application.dataPath + "/Textures/1x1 " + (color.r * 255).ToString() + "," + (color.g * 255).ToString() + "," + (color.b * 255).ToString() + ".png", bytes);
        }
        else
        {
            error = "The defined color already exists.";
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("There might be a small delay while the texture is getting imported.");

        color = EditorGUILayout.ColorField(color);

        if (GUILayout.Button("Process"))
        {
            error = "";
            CreateColorPicture(color);
            if (error == "")
            {
                CloseWindow();
            }
        }

        GUILayout.Space(20);
        GUI.color = Color.red;
        EditorGUILayout.LabelField(error, EditorStyles.whiteLabel, GUILayout.Width(250f));
    }
}