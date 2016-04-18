using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A custom editor for the GridManager
/// </summary>
[CustomEditor(typeof(ProceduralSphere))]
public class PCEditor : Editor
{
    /// <summary>
    /// An instance to this object.
    /// </summary>
    private ProceduralSphere PC;

    private ProceduralSphere.Region Selected;

    /// <summary>
    /// Used to draw the inspectorGUI.
    /// </summary>
    public override void OnInspectorGUI()
    {
        PC = (ProceduralSphere)target;
        if (PC == null)
        {
            return;
        }

        EditorGUILayout.LabelField("General: ", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Progress: " + PC.progress * 100.0f + "%");
        PC.WidthTick = EditorGUILayout.IntSlider("Width Tick: ", PC.WidthTick, 4, 10);
        PC.Width = Mathf.RoundToInt(Mathf.Pow(2, PC.WidthTick));
        EditorGUILayout.LabelField("Width: " + PC.Width);
        PC.TerrainMaterial = (Material)EditorGUILayout.ObjectField(PC.TerrainMaterial, typeof(Material), true);

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("curve"), new GUIContent("Height Curve: "));
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10f);
        EditorGUILayout.LabelField("Region: ", EditorStyles.boldLabel);
        if (PC.Regions == null)
        {
            PC.Regions = new ProceduralSphere.Region[1];
        }
        if (Selected == null)
        {
            Selected = PC.Regions[0];
        }
        Texture2D tex = new Texture2D(50, 100);

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                Color col = PC.Regions[0].color;
                for (int i = 0; i < PC.Regions.Length; i++)
                {
                    float height = (float)y / tex.height;
                    if (height <= PC.Regions[i].height)
                    {
                        col = PC.Regions[i].color;
                        break;
                    }
                }
                tex.SetPixel(x, y, col);
            }
        }
        tex.Apply();
        GUI.backgroundColor = Color.clear;
        if (GUILayout.Button(tex))
        {
            RegionEditor.Init(PC.MaxHeight);
        }

        GUILayout.Space(10f);

        EditorGUILayout.LabelField("Terrain Settings: ", EditorStyles.boldLabel);
        PC.Radius = EditorGUILayout.IntField("Radius: ", PC.Radius);
        PC.MaxHeight = EditorGUILayout.IntField("TerrainHeight: ", PC.MaxHeight);
        PC.Octaves = EditorGUILayout.IntField("Octaves: ", PC.Octaves);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}