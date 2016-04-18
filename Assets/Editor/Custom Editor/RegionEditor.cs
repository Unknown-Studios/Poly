using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RegionEditor : EditorWindow
{
    private int Selected = -1;
    private ProceduralSphere PS;
    private int MH;

    private float current;

    public static void Init(int MaxHeight)
    {
        RegionEditor window = (RegionEditor)GetWindow(typeof(RegionEditor));
        window.MH = MaxHeight;
        window.maxSize = new Vector2(250f, 375f);
        window.minSize = window.maxSize;
        window.Show();
    }

    private void OnGUI()
    {
        if (PS != null)
        {
            GUILayout.Space(10f);
            if (Selected != -1)
            {
                PS.Regions[Selected].Name = EditorGUILayout.TextField("Name: ", PS.Regions[Selected].Name);
                EditorGUILayout.LabelField("Height: " + Mathf.RoundToInt(PS.Regions[Selected].height * MH) + " (" + Mathf.RoundToInt(PS.Regions[Selected].height * 100.0f) + "%)");
                PS.Regions[Selected].color = EditorGUILayout.ColorField("Color: ", PS.Regions[Selected].color);
                GUILayout.Space(10f);
            }
            EditorGUILayout.BeginHorizontal(GUILayout.Height(300f));
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < PS.Regions.Length; i++)
            {
                float height = i != 0 ? PS.Regions[i].height - PS.Regions[i - 1].height : PS.Regions[i].height;
                Texture2D tex = new Texture2D(100, (int)(300 * height));
                for (int y = 0; y < tex.height; y++)
                {
                    for (int x = 0; x < tex.width; x++)
                    {
                        tex.SetPixel(x, y, PS.Regions[i].color);
                    }
                }
                tex.Apply();
                GUIStyle myStyle = new GUIStyle(GUI.skin.label);

                RectOffset Rect0 = new RectOffset(0, 0, 0, 0);
                myStyle.margin = Rect0;
                myStyle.padding = Rect0;

                if (GUILayout.Button(tex, myStyle))
                {
                    Selected = i;
                }
            }
        }
        EditorGUILayout.EndVertical();
        if (Selected != -1)
        {
            float low = (1.0f / MH);
            float high = 1.0f - (1.0f / MH);

            if (Selected + 1 < PS.Regions.Length)
            {
                high = PS.Regions[Selected + 1].height - (1.0f / MH);
            }
            if (Selected - 1 >= 0)
            {
                low = PS.Regions[Selected - 1].height + (1.0f / MH);
            }
            if (Selected != PS.Regions.Length - 1)
            {
                PS.Regions[Selected].height = GUILayout.VerticalSlider(PS.Regions[Selected].height, low, high);
            }
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10f);
    }

    private void Update()
    {
        if (PS == null)
        {
            PS = FindObjectOfType<ProceduralSphere>();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Selected--;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Selected++;
        }
        Selected = Mathf.Clamp(Selected, 0, PS.Regions.Length - 1);
    }
}