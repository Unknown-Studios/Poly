using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RegionEditor : EditorWindow
{
    private int Selected = -1;
    private ProceduralSphere PS;

    private float current;

    public static void Init()
    {
        RegionEditor window = (RegionEditor)GetWindow(typeof(RegionEditor));
        window.minSize = new Vector2(250, 300);
        window.Show();
    }

    private void OnGUI()
    {
        if (PS == null)
        {
            PS = FindObjectOfType<ProceduralSphere>();
        }
        else
        {
            GUILayout.Space(10f);
            if (Selected != -1)
            {
                PS.Regions[Selected].Name = EditorGUILayout.TextField("Name: ", PS.Regions[Selected].Name);
                EditorGUILayout.LabelField("Height: " + PS.Regions[Selected].height);
                GUILayout.Space(10f);
            }
            EditorGUILayout.BeginHorizontal(GUILayout.Height(300f));
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < PS.Regions.Length; i++)
            {
                float height = i != 0 ? PS.Regions[i].height - PS.Regions[i - 1].height : PS.Regions[i].height;
                Texture2D tex = new Texture2D(100, (int)(250 * height));
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
                myStyle.border = Rect0;
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
            PS.Regions[Selected].height = GUILayout.VerticalSlider(PS.Regions[Selected].height, 0.0f, 1.0f);
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10f);
    }
}