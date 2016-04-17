using UnityEditor;
using UnityEngine;

public class GetNormalColor : EditorWindow
{
    private bool al;
    private Color col;
    private string error = "";
    private Color Preview;
    private bool suc;
    private Texture2D text;

    public static void CloseWindow()
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(GetNormalColor));
        window.Close();
    }

    [MenuItem("Window/Get Normal Color")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(GetNormalColor));
    }

    private Color GetColor(Texture2D pic, bool Alpha)
    {
        float r = 0.0f;
        float g = 0.0f;
        float b = 0.0f;
        float a = 255.0f;
        if (Alpha)
        {
            a = 0.0f;
        }
        for (int x = 0; x < pic.width; x++)
        {
            for (int y = 0; y < pic.height; y++)
            {
                r += pic.GetPixel(x, y).r;
                g += pic.GetPixel(x, y).g;
                b += pic.GetPixel(x, y).b;
                if (Alpha)
                {
                    a += pic.GetPixel(x, y).a;
                }
            }
        }
        r /= (pic.width * pic.height);
        g /= (pic.width * pic.height);
        b /= (pic.width * pic.height);
        if (Alpha)
        {
            a /= (pic.width * pic.height);
        }
        text = null;
        return new Color(r, g, b, a);
    }

    private void OnGUI()
    {
        GUILayout.Space(20);
        text = EditorGUILayout.ObjectField("Image", text, typeof(Texture2D), false) as Texture2D;
        GUILayout.Space(20);
        GUILayout.Label("Should alpha be included?");
        al = EditorGUILayout.Toggle(al);

        if (GUILayout.Button("Process"))
        {
            error = "";
            col = GetColor(text, al);
            suc = true;
        }

        if (suc)
        {
            GUI.color = Color.green;
            EditorGUILayout.ColorField(col);
        }

        GUILayout.Space(20);
        GUI.color = Color.red;
        EditorGUILayout.LabelField(error, EditorStyles.whiteLabel, GUILayout.Width(250f));
    }
}