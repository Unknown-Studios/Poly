using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralTerrain))]
public class PTEditor : Editor
{
    #region Fields

    public static long lastUpdateTick;
    public static string[] layerNames;
    public static List<int> layerNumbers;
    public static List<string> layers;
    public bool[] current;
    private ProceduralTerrain PT;

    #endregion Fields

    #region Methods

    private bool Biomes;
    private bool Fold;

    private bool Grass;

    private bool Settings;
    private bool Trees;

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

    public override void OnInspectorGUI()
    {
        PT = (ProceduralTerrain)target;
        Settings = EditorGUILayout.Foldout(Settings, "Settings: ");
        if (Settings)
        {
            PT.groundFrq = EditorGUILayout.FloatField(PT.groundFrq, "Normal Frequency:");
            PT.mountainFrq = EditorGUILayout.FloatField(PT.mountainFrq, "Mountain Frequency:");
            PT.riverFrq = EditorGUILayout.FloatField(PT.riverFrq, "River Frequency:");
        }

        Fold = EditorGUILayout.Foldout(Fold, "Variables: ");
        if (Fold)
        {
            Biomes = EditorGUILayout.Foldout(Biomes, "Biomes:");
            if (Biomes)
            {
                for (int i = 0; i < PT._Biomes.Length; i++)
                {
                }
            }
            Trees = EditorGUILayout.Foldout(Trees, "Tree:");
            if (Trees)
            {
            }
        }
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    #endregion Methods
}