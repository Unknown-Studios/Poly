﻿using System.Collections;
using UnityEngine;

[System.Serializable]
public class ProceduralSphere : MonoBehaviour
{
    public int WidthTick;

    public int Width;

    public int Radius;
    public int MaxHeight;

    [HideInInspector]
    public bool done;

    public int Octaves;
    public Material TerrainMaterial;
    public string currentAction;
    public float progress;

    private int ve;
    private GameObject[] sides;

    // Use this for initialization
    public void OnBeforeSpawn(Vector3 SpawnPos)
    {
        StartCoroutine(GenerateTerrain());
    }

    private void Start()
    {
        OnBeforeSpawn(Vector3.zero);
    }

    private IEnumerator AddSide(int side)
    {
        GameObject s0 = new GameObject();
        sides[side] = s0;
        s0.name = "Side #" + side;
        s0.transform.parent = transform;

        MeshFilter mf0 = s0.AddComponent<MeshFilter>();
        s0.AddComponent<MeshRenderer>();
        s0.AddComponent<MeshCollider>();
        Mesh m0 = mf0.mesh = new Mesh();
        m0.name = "Side #" + side;

        for (int x = 0; x < Width / 16; x++)
        {
            for (int y = 0; y < Width / 16; y++)
            {
                GameObject gm = new GameObject();
                gm.transform.parent = s0.transform;
                gm.name = "Chunk (" + x + "," + y + ") side #" + side;

                LOD lod = gm.AddComponent<LOD>();
                lod.side = side;
                lod.Chunk = new Vector2(x, y);
                lod.Width = Width;
                lod.terrainMaterial = TerrainMaterial;

                lod.Radius = Radius;
                lod.MaxHeight = MaxHeight;
                lod.Octaves = Octaves;
            }
            yield return null;
        }
    }

    private IEnumerator GenerateTerrain()
    {
        sides = new GameObject[6];

        if (!PlayerPrefs.HasKey("Seed"))
        {
            PlayerPrefs.SetInt("Seed", Random.Range(0, 999999));
        }

        for (int i = 0; i < 6; i++)
        {
            StartCoroutine(AddSide(i));
            progress = 1.0f / (6 - i);
            progress = Mathf.Clamp01(progress);
            yield return null;
        }
        yield return null;
    }
}