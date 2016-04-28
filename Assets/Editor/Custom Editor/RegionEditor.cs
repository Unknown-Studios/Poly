using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RegionEditor : EditorWindow
{
    private int Selected = -1;
    private ProceduralSphere PS;
    private int MH;

    private float current;

    private bool Add;

    private bool Subtract;

    private List<ProceduralSphere.Region> rg = new List<ProceduralSphere.Region>();

    public static void Init(int MaxHeight)
    {
        RegionEditor window = (RegionEditor)GetWindow(typeof(RegionEditor));
        window.MH = MaxHeight;
        window.maxSize = new Vector2(250f, 425f);
        window.minSize = window.maxSize;
        window.Show();
    }

    private void OnGUI()
    {
        if (PS != null)
        {
            if (PS.Regions == null || PS.Regions.Length == 0)
            {
                PS.Regions = new ProceduralSphere.Region[2];
                PS.Regions[0] = new ProceduralSphere.Region();
                PS.Regions[1] = new ProceduralSphere.Region();
            }
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
                else
                {
                    GUILayout.Space(18f);
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
                    myStyle.padding = Rect0;

                    if (GUILayout.Button(tex, myStyle))
                    {
                        Selected = i;
                    }
                }
            }
            EditorGUILayout.EndVertical();
            for (int i = 0; i < PS.Regions.Length; i++)
            {
                float low = (1.0f / MH) + 0.01f;
                float high = 1.0f - (1.0f / MH);

                if (i + 1 < PS.Regions.Length)
                {
                    high = PS.Regions[i + 1].height - (1.0f / MH) - 0.01f;
                }
                if (i - 1 >= 0)
                {
                    low = PS.Regions[i - 1].height + (1.0f / MH) + 0.01f;
                }
                PS.Regions[i].height = Mathf.Clamp(PS.Regions[i].height, low, high);
            }

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
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add"))
            {
                Add = true;
            }
            if (GUILayout.Button("Subtract"))
            {
                Subtract = true;
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
            if (Add)
            {
                Add = false;
                rg.Clear();

                rg.Add(new ProceduralSphere.Region(0.1f));

                for (int i = 0; i < PS.Regions.Length; i++)
                {
                    rg.Add(PS.Regions[i]);
                }

                PS.Regions = rg.ToArray();
                Selected = 0;
                PS.Regions[0].color = Random.ColorHSV();
            }
            if (Subtract)
            {
                Subtract = false;
                rg.Clear();

                for (int i = 0; i < PS.Regions.Length; i++)
                {
                    rg.Add(PS.Regions[i]);
                }
                if (Selected == -1)
                {
                    Selected = 0;
                }
                rg.RemoveAt(Selected);

                PS.Regions = rg.ToArray();
                Selected = 0;
            }

            Selected = Mathf.Clamp(Selected, 0, PS.Regions.Length - 1);
        }
    }
}