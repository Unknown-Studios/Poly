using OnePathfinding;
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
    /// Last time the layer mask was updated.
    /// </summary>
    public static long lastUpdateTick;

    /// <summary>
    /// The list of names for the layers.
    /// </summary>
    public static string[] layerNames;

    /// <summary>
    /// The number for each layer.
    /// </summary>
    public static List<int> layerNumbers;

    /// <summary>
    /// The name for each layer.
    /// </summary>
    public static List<string> layers;

    /// <summary>
    /// Bool array toggling whether to show grid settings or not.
    /// </summary>
    public bool[] current;

    /// <summary>
    /// An instance to this object.
    /// </summary>
    private ProceduralSphere PC;

    /// <summary>
    /// Used as a custom editor field for layer masks.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="selected"></param>
    /// <param name="showSpecial"></param>
    /// <returns></returns>
    public LayerMask LayerMaskField(string label, LayerMask selected, bool showSpecial)
    {
        //Unity 3.5 and up

        if (layers == null || (System.DateTime.Now.Ticks - lastUpdateTick > 10000000L && Event.current.type == EventType.Layout))
        {
            lastUpdateTick = System.DateTime.Now.Ticks;
            if (layers == null)
            {
                layers = new List<string>();
                layerNumbers = new List<int>();
                layerNames = new string[4];
            }
            else
            {
                layers.Clear();
                layerNumbers.Clear();
            }

            int emptyLayers = 0;
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);

                if (layerName != "")
                {
                    for (; emptyLayers > 0; emptyLayers--)
                        layers.Add("Layer " + (i - emptyLayers));
                    layerNumbers.Add(i);
                    layers.Add(layerName);
                }
                else
                {
                    emptyLayers++;
                }
            }

            if (layerNames.Length != layers.Count)
            {
                layerNames = new string[layers.Count];
            }
            for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];
        }

        selected.value = EditorGUILayout.MaskField(label, selected.value, layerNames);

        return selected;
    }

    /// <summary>
    /// Used to draw the inspectorGUI.
    /// </summary>
    public override void OnInspectorGUI()
    {
        PC = (ProceduralSphere)target;
        EditorUtility.SetDirty(target);
        if (PC == null)
        {
            return;
        }

        EditorGUILayout.LabelField("General: ", EditorStyles.boldLabel);
        PC.WidthTick = EditorGUILayout.IntSlider("Width Tick: ", PC.WidthTick, 3, 10);
        PC.Width = Mathf.RoundToInt(Mathf.Pow(2, PC.WidthTick));
        EditorGUILayout.LabelField("Width: " + PC.Width);
        PC.Radius = EditorGUILayout.IntField("Radius: ", PC.Radius);
        GUILayout.Space(20f);

        EditorGUILayout.LabelField("Level Of Detail: ", EditorStyles.boldLabel);
        PC.LODLevel = EditorGUILayout.IntSlider("Simplification Level: ", PC.LODLevel, 0, 4);

        GUILayout.Space(20f);

        EditorGUILayout.LabelField("Terrain Settings: ", EditorStyles.boldLabel);
        PC.Radius = EditorGUILayout.IntField("TerrainHeight: ", PC.MaxHeight);
        PC.Radius = EditorGUILayout.IntField("Octaves: ", PC.Octaves);

        GUILayout.Space(10f);
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