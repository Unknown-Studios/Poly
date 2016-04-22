using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public Region[] Regions;

    public AnimationCurve curve;
    public float scale;
    private int ve;
    private GameObject[] sides;

    // Use this for initialization
    public void OnBeforeSpawn(Vector3 SpawnPos)
    {
        StartCoroutine(GenerateTerrain());
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Game")
            OnBeforeSpawn(Vector3.zero);
    }

    private IEnumerator AddSide(int side)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        GameObject s0 = new GameObject();
        sides[side] = s0;
        s0.name = "Side #" + side;
        s0.transform.parent = transform;

        Texture2D tex = new Texture2D(Width, Width);
        Material SideMaterial = new Material(TerrainMaterial);
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Width; y++)
            {
                if (x % 32 == 0 || y % 32 == 0)
                {
                    tex.SetPixel(x, y, Color.black);
                }
                else
                {
                    tex.SetPixel(x, y, Color.green);
                }
            }
        }
        tex.Apply();
        SideMaterial.mainTexture = tex;

        for (int x = 0; x < Width / 16; x++)
        {
            for (int y = 0; y < Width / 16; y++)
            {
                GameObject gm = new GameObject();
                gm.transform.parent = s0.transform;
                gm.name = "Chunk (" + x + "," + y + ") side #" + side;
                gm.layer = LayerMask.NameToLayer("LOD");

                LOD lod = gm.AddComponent<LOD>();
                lod.side = side;
                lod.Chunk = new Vector2(x, y);
                lod.Width = Width;
                lod.terrainMaterial = SideMaterial;
                lod.Regions = Regions;
                lod.curve = curve;
                lod.scale = scale;

                lod.Radius = Radius;
                lod.MaxHeight = MaxHeight;
                lod.Octaves = Octaves;
                if (y % 2 == 0)
                {
                    yield return null;
                }
            }
        }
        progress = 1.0f / (6 - side);
        progress = Mathf.Clamp01(progress);
        if (progress == 1.0f)
        {
            done = true;
        }
        watch.Stop();
        UnityEngine.Debug.Log(watch.ElapsedMilliseconds / 1000.0f);
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
            yield return null;
        }
        yield return null;
    }

    [System.Serializable]
    public class Region
    {
        public string Name;
        public Color color = new Color(1, 1, 1, 1);
        public float height;
    }
}