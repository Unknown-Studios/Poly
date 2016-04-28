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
        window.maxSize = new Vector2(250f, 400f);
        window.minSize = window.maxSize;
        window.Show();
    }

    private void OnGUI()
    {
        if (PS != null)
        {
            if (PS.Regions[PS.Regions.Length - 1].height != 1.0f)
            {
                PS.Regions[PS.Regions.Length - 1].height = 1.0f;
            }

            GUILayout.Space(10f);
            if (Selected != -1)
            {
                PS.Regions[Selected].Name = EditorGUILayout.TextField("Name: ", PS.Regions[Selected].Name);
                EditorGUILayout.LabelField("Height: " + Mathf.RoundToInt(PS.Regions[Selected].height * MH) + " (" + Mathf.RoundToInt(PS.Regions[Selected].height * 100.0f) + "%)");
                PS.Regions[Selected].Biome = EditorGUILayout.Toggle("Biome: ", PS.Regions[Selected].Biome);
                if (!PS.Regions[Selected].Biome)
                {
                    PS.Regions[Selected].color = EditorGUILayout.ColorField("Color: ", PS.Regions[Selected].color);
                }
                GUILayout.Space(10f);
            }
            EditorGUILayout.BeginHorizontal(GUILayout.Height(300f));
            EditorGUILayout.BeginVertical();
            for (int i = PS.Regions.Length - 1; i >= 0; i--)
            {
                float height = i != 0 ? PS.Regions[i].height - PS.Regions[i - 1].height : PS.Regions[i].height;
                if (height > 0.0f)
                {
                    Texture2D tex = new Texture2D(100, (int)(300 * height));
                    for (int y = 0; y < tex.height; y++)
                    {
                        for (int x = 0; x < tex.width; x++)
                        {
                            if (PS.Regions[i].Biome)
                            {
                                tex.SetPixel(x, y, Random.ColorHSV());
                            }
                            else
                            {
                                tex.SetPixel(x, y, PS.Regions[i].color);
                            }
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
                if (Selected != PS.Regions.Length - 1)
                {
                    float low = (1.0f / MH) + 0.01f;
                    float high = 1.0f - (1.0f / MH);

                    if (Selected + 1 < PS.Regions.Length)
                    {
                        high = PS.Regions[Selected + 1].height - (1.0f / MH) - 0.01f;
                    }
                    if (Selected - 1 >= 0)
                    {
                        low = PS.Regions[Selected - 1].height + (1.0f / MH) + 0.01f;
                    }
                    PS.Regions[Selected].height = GUILayout.VerticalSlider(PS.Regions[Selected].height, high, low);
                }
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10f);
        }
        else
        {
            EditorGUILayout.LabelField("ProceduralSphere Component not found");
        }
    }

    private void Update()
    {
        if (PS == null)
        {
            PS = FindObjectOfType<ProceduralSphere>();
        }
        if (PS != null)
        {
            if (PS.Regions == null || PS.Regions.Length <= 0)
            {
                PS.Regions = new ProceduralSphere.Region[1];
                PS.Regions[0] = new ProceduralSphere.Region();
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
}