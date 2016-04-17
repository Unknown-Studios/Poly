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
        PC.curve = EditorGUILayout.CurveField("Height curve: ", PC.curve);

        serializedObject.Update();
        SerializedProperty tps = serializedObject.FindProperty("Regions");
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(tps, true);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

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

    /// <summary>
    /// Update the inspector.
    /// </summary>
    public void OnInspectorUpdate()
    {
        // This will only get called 10 times per second.
        Repaint();
    }
}