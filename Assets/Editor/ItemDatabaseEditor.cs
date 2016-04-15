using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseEditor : Editor
{
    private bool[] current;
    private ItemDatabase myTarget;
    private bool[] Transforms;

    public override void OnInspectorGUI()
    {
        myTarget = (ItemDatabase)target;
        List<ItemDatabase.item> items = myTarget.Item;
        if (current == null || current.Length != items.Count)
        {
            current = new bool[items.Count];
        }
        if (Transforms == null || Transforms.Length != items.Count)
        {
            Transforms = new bool[items.Count];
        }

        EditorGUILayout.LabelField("Item Database:");
        for (int i = 0; i < items.Count; i++)
        {
            current[i] = EditorGUILayout.Foldout(current[i], myTarget.Item[i].Name);
            if (current[i])
            {
                items[i].Name = EditorGUILayout.TextField("Name: ", myTarget.Item[i].Name);
                items[i].ID = i;
                items[i].texture = (Texture2D)EditorGUILayout.ObjectField("Icon: ", items[i].texture, typeof(Texture2D), true);
                items[i].Type = (Game.ItemType)EditorGUILayout.EnumPopup("Item Type:", items[i].Type);
                GUILayout.Space(10);
                if (items[i].Type == Game.ItemType.Weapon)
                {
                    items[i].WeaponType = (Game.wt)EditorGUILayout.EnumPopup("Weapon Type: ", items[i].WeaponType);
                    if (items[i].WeaponType != Game.wt.Melee && items[i].WeaponType != Game.wt.Nothing)
                    {
                        items[i].Ammotype = (ItemDatabase.AmmoType)EditorGUILayout.EnumPopup("Ammo Type: ", items[i].Ammotype);
                    }
                    items[i].Damage = EditorGUILayout.IntField("Damage: ", items[i].Damage);
                    items[i].Range = EditorGUILayout.IntField("Range: ", items[i].Range);
                    GUILayout.Space(5);
                    items[i].Gameobject = (GameObject)EditorGUILayout.ObjectField("Weapon: ", items[i].Gameobject, typeof(GameObject), true);
                    GUILayout.Space(5);
                    Transforms[i] = EditorGUILayout.Foldout(Transforms[i], "Transform:");
                    if (Transforms[i])
                    {
                        items[i].offset = EditorGUILayout.Vector3Field("Position: ", items[i].offset);
                        items[i].rotation = EditorGUILayout.Vector3Field("Rotation: ", items[i].rotation);
                        items[i].scale = EditorGUILayout.Vector3Field("Scale: ", items[i].scale);
                    }
                }
                else if (items[i].Type == Game.ItemType.Consumable)
                {
                    items[i].ConsumType = (ItemDatabase.ConsumableType)EditorGUILayout.EnumPopup("Consumable Type: ", myTarget.Item[i].ConsumType);
                    items[i].ConsumableAmount = EditorGUILayout.IntField(items[i].ConsumType.ToString() + " Amount: ", items[i].ConsumableAmount);
                }
                else if (items[i].Type == Game.ItemType.Building)
                {
                    items[i].Gameobject = (GameObject)EditorGUILayout.ObjectField("Building: ", items[i].Gameobject, typeof(GameObject), true);
                    GUILayout.Space(5);
                    items[i].Range = EditorGUILayout.IntField("Range: ", items[i].Range);

                    Transforms[i] = EditorGUILayout.Foldout(Transforms[i], "Transform:");
                    if (Transforms[i])
                    {
                        items[i].offset = EditorGUILayout.Vector3Field("Position: ", items[i].offset);
                        items[i].rotation = EditorGUILayout.Vector3Field("Rotation: ", items[i].rotation);
                        items[i].scale = EditorGUILayout.Vector3Field("Scale: ", items[i].scale);
                    }
                }
            }
            GUILayout.Space(5);
        }
        if (GUILayout.Button("New Item"))
        {
            myTarget.Item.Add(new ItemDatabase.item());
        }
    }
}