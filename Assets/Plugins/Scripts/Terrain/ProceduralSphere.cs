using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[Serializable]
public class ProceduralSphere : MonoBehaviour
{
    public int WidthTick;

    public int Width;

    public int Radius;
    public int MaxHeight;

    public int Seed;

    [HideInInspector]
    public bool done;

    public int Octaves;
    public Material TerrainMaterial;
    public string currentAction;
    public float progress;

    public Region[] Regions;

    public AnimationCurve curve;
    public float scale;
    public Queue<GameObject> queue;
    public List<GameObject> Done;
    public bool isDone;
    public VoronoiPoint[] points;
    public Biome[] biomes;
    private int ve;
    private GameObject[] sides;
    private int numSides = 0;

    private ThreadedJob thread;

    private int colCount;

    private bool Spawned;

    // Use this for initialization
    public void OnBeforeSpawn(Vector3 SpawnPos)
    {
        StartCoroutine(GenerateTerrain());
    }

    public void Update()
    {
        if (done == true)
        {
            done = false;
        }
        progress = colCount / (float)((Width / 16) * (Width / 16) * 6);
        if (progress == 1.0f)
        {
            isDone = true;
        }
        if (isDone && !Spawned && Game.player != null)
        {
            Spawned = true;
            Vector3 pos = Vector3.one * (Radius + MaxHeight);
            while (Mathf.Clamp01((Vector3.Distance(pos, Vector3.zero) - Radius) / MaxHeight) > Regions[0].height)
            {
                pos = GetHeight(Random.onUnitSphere);
            }
            Game.player.transform.position = pos;
        }
    }

    //Redirect to GetHeight(Vector3);
    public Vector3 GetHeight(float x, float y, float z)
    {
        return GetHeight(new Vector3(x, y, z));
    }

    //Get the height of the terrain in any point
    public Vector3 GetHeight(Vector3 v3)
    {
        Vector3 startPos = v3.normalized * (Radius + MaxHeight);
        RaycastHit hit;
        Debug.DrawLine(startPos, Vector3.zero, Color.red, 30.0f);
        //If height found
        if (Physics.Linecast(startPos, Vector3.zero, out hit))
        {
            //Add 0.5 to the height
            Vector3 normal = hit.point / (Radius + MaxHeight);
            Vector3 position = normal * (Radius + MaxHeight + 0.5f);

            return position;
        }
        else
        {
            return Vector3.zero * Mathf.Infinity;
        }
    }

    //Called on script load.
    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Game")
            OnBeforeSpawn(Vector3.zero);
    }

    private void Awake()
    {
        PlayerPrefs.SetInt("Seed", Seed);
        PlayerPrefs.Save();

        points = new VoronoiPoint[250];

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new VoronoiPoint(Random.onUnitSphere * Radius);
            points[i].biome = biomes[Random.Range(0, biomes.Length)];
        }
    }

    //Add a side of the cube/cubesphere
    private IEnumerator AddSide(int side)
    {
        if (numSides < 6)
        {
            if (thread == null)
            {
                thread = new ThreadedJob();
            }

            GameObject s0 = new GameObject();
            sides[side] = s0;
            s0.name = "Side #" + side;
            s0.transform.parent = transform;

            Texture2D tex = new Texture2D(Width, Width);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            Material SideMaterial = new Material(TerrainMaterial);
            SideMaterial.mainTexture = tex;
            int x = 0, y = 0;

            for (x = 0; x < Width / 16; x++)
            {
                for (y = 0; y < Width / 16; y++)
                {
                    GameObject gm = new GameObject();
                    gm.transform.parent = s0.transform;
                    gm.name = "Chunk (" + x + "," + y + ")";
                    gm.tag = "Chunk";
                    gm.layer = LayerMask.NameToLayer("LOD");

                    LOD lod = gm.AddComponent<LOD>();
                    lod.points = points;
                    lod.side = side;
                    lod.Chunk = new Vector2(x, y);
                    lod.Width = Width;
                    lod.terrainMaterial = SideMaterial;
                    lod.Regions = Regions;
                    lod.curve = curve;
                    lod.scale = scale;
                    lod.thread = thread;
                    lod.PS = this;

                    lod.Radius = Radius;
                    lod.MaxHeight = MaxHeight;
                    lod.Octaves = Octaves;
                }
            }
            yield return null;
            numSides++;
            done = true;
        }
    }

    private IEnumerator AddColliders()
    {
        Debug.Log("Add colliders " + queue.Count);
        //Wait for the queue to start filling
        while (queue.Count <= 0)
        {
            yield return null;
        }
        //Start adding colliders when ready
        while (queue.Count > 0)
        {
            GameObject current = queue.Dequeue();
            MeshCollider mc = current.GetComponent<MeshCollider>();
            if (!mc.convex)
            {
                mc.convex = true;
				yield return null;
                lod.SetTargetLOD(4);
                Done.Add(current);
            }
            colCount++;
            yield return null;
        }
    }

    private IEnumerator GenerateTerrain()
    {
        queue = new Queue<GameObject>();
        sides = new GameObject[6];

        if (!PlayerPrefs.HasKey("Seed"))
        {
            PlayerPrefs.SetInt("Seed", Random.Range(0, 999999));
        }

        for (int i = 0; i < 6; i++)
        {
            StartCoroutine(AddSide(i));
            yield return null;
        }
        while (!done)
        {
            yield return null;
        }
        StartCoroutine(AddColliders());
        yield return null;
    }

    public struct V3
    {
        public float x;
        public float y;
        public float z;

        public V3(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public static V3 one
        {
            get
            {
                return new V3(1, 1, 1);
            }
        }

        public static V3 operator -(V3 v1, V3 scalar)
        {
            return new V3(v1.x - scalar.x, v1.y - scalar.y, v1.z - scalar.z);
        }

        public static V3 operator *(V3 v1, float scalar)
        {
            return new V3(v1.x * scalar, v1.y * scalar, v1.z * scalar);
        }

        public static V3 operator /(V3 v1, float scalar)
        {
            return new V3(v1.x / scalar, v1.y / scalar, v1.z / scalar);
        }
    }

    [Serializable]
    public class Region
    {
        public string Name = "";
        public Color color = Color.white;
        public bool Biome = false;
        public float height = 1.0f;

        public Region()
        {
            Name = "Test";
            color = Color.white;
            height = 1.0f;
        }

        public Region(float Height)
        {
            Name = "Test";
            color = Color.white;
            height = Height;
        }
    }

    public class MeshData
    {
        public float[] heightmap;
        public V3[] v3;
        public int[] triangles;
        public int LODW;
    }

    public class ProcCallback
    {
        public int LODW;
        public Action<MeshData> callback;
        public Func<MeshData> Function;
    }

    [Serializable]
    public class Biome
    {
        public string Name;
        public Color biomeColor = Color.white;
    }

    public class VoronoiPoint
    {
        public Vector3 point;
        public Biome biome;

        public VoronoiPoint()
        {
        }

        public VoronoiPoint(Vector3 position)
        {
            point = position;
        }
    }
}