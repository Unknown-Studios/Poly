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
    private int ve;
    private GameObject[] sides;
    private int numSides = 0;

    private ThreadedJob thread;

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
    }

    //Redirect to GetHeight(Vector3);
    public Vector3 GetHeight(float x, float y, float z)
    {
        return GetHeight(new Vector3(x, y, z));
    }

    //Get the height of the terrain in any point
    public Vector3 GetHeight(Vector3 v3)
    {
        Vector3 startPos = new Vector3();
        RaycastHit hit;
        //If height found
        if (Physics.Raycast(startPos, Vector3.zero, out hit, (MaxHeight + Radius) * 2f))
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
            float localProgress = 0.0f;
            for (int x = 0; x < Width / 16; x++)
            {
                for (int y = 0; y < Width / 16; y++)
                {
                    GameObject gm = new GameObject();
                    gm.transform.parent = s0.transform;
                    gm.name = "Chunk (" + x + "," + y + ")";
                    gm.layer = LayerMask.NameToLayer("LOD");

                    LOD lod = gm.AddComponent<LOD>();
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
                    if (y % 2 == 0)
                    {
                        yield return null;
                    }
                }
                localProgress = (x + 1) / (Width / 16f);

                progress = Mathf.Clamp01(localProgress / (6 - side));
            }

            while (progress != 1.0f)
            {
                yield return null;
            }
            numSides++;
        }
        else
        {
            progress = 1.0f;
        }
        if (progress == 1.0f)
        {
            done = true;
        }
    }

    private IEnumerator AddColliders()
    {
        Debug.Log("AddColliders: Start");
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        while (queue.Count > 0)
        {
            GameObject current = queue.Dequeue();
            MeshCollider mc = current.GetComponent<MeshCollider>();
            if (mc.convex != true)
            {
                LOD lod = current.GetComponent<LOD>();
                mc.convex = true;
                lod.SetTargetLOD(4);
            }
            yield return null;
        }
        watch.Stop();
        Debug.Log("Time: " + watch.ElapsedMilliseconds / 1000.0f);
        if (SceneManager.GetActiveScene().name == "Test")
        {
            Game.Notice("Time: " + watch.ElapsedMilliseconds / 1000.0f);
        }
        Debug.Log("AddColliders: End");
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
        public string Name;
        public Color color = new Color(1, 1, 1, 1);
        public float height;
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
}