using CoherentNoise.Generation.Fractal;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer))]
public class LOD : MonoBehaviour
{
    public int TargetLOD = 0;

    public float Distance;

    [HideInInspector]
    public Vector2 Chunk;

    [HideInInspector]
    public int side;

    public int Radius;
    public int MaxHeight;
    public int Octaves = 3;
    public float groundFrq = 0.01f;
    public int Width;

    [HideInInspector]
    public Material terrainMaterial;

    public ProceduralSphere.Region[] Regions;
    public AnimationCurve curve;
    public float scale;
    public ThreadedJob thread;
    private Mesh mesh;
    private int _LODLevel;
    private int LODLevel;
    private int ve;
    private RidgeNoise noise;
    private RidgeNoise hills;
    private PinkNoise pink;
    private MeshRenderer mr0;
    private MeshCollider mc0;
    private int ChunkWidth = 16;

    private Vector3[] vert;
    private Vector3[] norm;
    private int[] tri;
    private Vector2[] uv;

    private Color[] colors;

    private bool SplashIt;

    private bool FirstTime = false;

    private bool SplashDone = false;

    private bool CallbackDone;

    public void UpdateLODSettings(Vector3 pos, int[] lodlevels)
    {
        if (mc0 == null)
        {
            mc0 = GetComponent<MeshCollider>();
        }
        Distance = Vector3.Distance(pos, mc0.bounds.center);

        //Calculate which LODLevel should be used.
        TargetLOD = 4;
        for (int x = 0; x < lodlevels.Length; x++)
        {
            if (Distance < lodlevels[x])
            {
                TargetLOD = x;
                break;
            }
        }

        if (LODLevel != TargetLOD)
        {
            LODLevel = TargetLOD;
            _LODLevel = (int)Mathf.Pow(2f, LODLevel);
            StartCoroutine(GenerateMesh(LODLevel == 0));
        }
    }

    public void SetTargetLOD(int TargetLOD)
    {
        LODLevel = TargetLOD;
        _LODLevel = (int)Mathf.Pow(2f, LODLevel);
        StartCoroutine(GenerateMesh());
    }

    private void CreateTriangle(ref int index, int x, int y)
    {
        int LODWidth = (ChunkWidth / _LODLevel) + 1;

        if (index >= tri.Length)
        {
            Debug.LogWarning(index + "/" + tri.Length);
            Debug.LogWarning("Vert: " + index + "/" + vert.Length);
        }

        tri[index] = (y * LODWidth) + x;
        tri[index + 1] = ((y + 1) * LODWidth) + x;
        tri[index + 2] = (y * LODWidth) + x + 1;

        tri[index + 3] = ((y + 1) * LODWidth) + x;
        tri[index + 4] = ((y + 1) * LODWidth) + x + 1;
        tri[index + 5] = (y * LODWidth) + x + 1;
        index += 6;
    }

    /// <summary>
    /// Adds a vertex point (Used in mesh creation)
    /// </summary>
    /// <param name="x">X-coordinate for vertex</param>
    /// <param name="y">Y-coordinate for vertex</param>
    /// <param name="z">Z-coordinate for vertex</param>
    /// <returns></returns>
    private void AddVertex(int x, int y, int z, ref float[] H, ref ProceduralSphere.V3[] N)
    {
        ProceduralSphere.V3 v = new ProceduralSphere.V3(x, y, z) * 2f / Width - ProceduralSphere.V3.one;
        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;
        ProceduralSphere.V3 s = new ProceduralSphere.V3(x, y, z);
        s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);

        float plain = (pink.GetValue(x, y, z) + 1.0f) / 4f;

        float val = noise.GetValue(x - 549, y + 2585, z + 54);
        float n = val < 0 ? 0 : val;
        val = (n - 0.75f) * 1.25f;
        float mountains = val < 0 ? 0 : val;

        val = hills.GetValue(x + 549, y - 2585, z - 544);
        float hill = val < 0 ? 0 : val;
        val = (hill - 0.5f) / 10f;
        hill = val < 0 ? 0 : val;

        H[ve] = (mountains + plain + hill);

        N[ve] = s;

        ve++;
    }

    private void Start()
    {
        _LODLevel = (int)Mathf.Pow(2f, TargetLOD);
        Random.seed = PlayerPrefs.GetInt("Seed");
        noise = new RidgeNoise(PlayerPrefs.GetInt("Seed"));
        pink = new PinkNoise(PlayerPrefs.GetInt("Seed"));
        hills = new RidgeNoise(PlayerPrefs.GetInt("Seed"));

        pink.Frequency = 0.005f;
        pink.Lacunarity = 1.2f;
        pink.Persistence = 0.17f;

        noise.OctaveCount = Octaves * 2;
        noise.Frequency = 0.0025f;
        noise.Gain = 1f;
        noise.Exponent = 2f;

        hills.OctaveCount = 2;
        hills.Frequency = 0.01f;
        hills.Gain = 1f;
        hills.Exponent = 2f;

        mesh = GetComponent<MeshFilter>().mesh;
        mr0 = GetComponent<MeshRenderer>();
        mc0 = GetComponent<MeshCollider>();
        mr0.material = terrainMaterial;

        StartCoroutine(GenerateMesh());
    }

    private void AddToThread(ProceduralSphere.ProcCallback item)
    {
        if (thread == null)
        {
            Debug.LogWarning("thread doesn't exist");
        }
        thread.Add(item);
    }

    private ProceduralSphere.MeshData AddVertices()
    {
        int LODW = ChunkWidth / _LODLevel;

        ProceduralSphere.MeshData data = new ProceduralSphere.MeshData();

        ProceduralSphere.V3[] NO = new ProceduralSphere.V3[(LODW + 1) * (LODW + 1)];
        float[] heightmap = new float[NO.Length];

        switch (side)
        {
            case 0:
                for (int y = 0; y <= ChunkWidth; y += _LODLevel)
                {
                    for (int x = 0; x <= ChunkWidth; x += _LODLevel)
                    {
                        AddVertex((int)(ChunkWidth * Chunk.x) + x, (int)(ChunkWidth * Chunk.y) + y, 0, ref heightmap, ref NO);
                    }
                }
                break;

            case 1:
                for (int y = 0; y <= ChunkWidth; y += _LODLevel)
                {
                    for (int z = 0; z <= ChunkWidth; z += _LODLevel)
                    {
                        AddVertex(Width, (int)(ChunkWidth * Chunk.x) + y, (int)(ChunkWidth * Chunk.y) + z, ref heightmap, ref NO);
                    }
                }
                break;

            case 2:
                for (int y = 0; y <= ChunkWidth; y += _LODLevel)
                {
                    for (int x = ChunkWidth; x >= 0; x -= _LODLevel)
                    {
                        AddVertex((int)(ChunkWidth * Chunk.x) + x, (int)(ChunkWidth * Chunk.y) + y, Width, ref heightmap, ref NO);
                    }
                }
                break;

            case 3:
                for (int y = 0; y <= ChunkWidth; y += _LODLevel)
                {
                    for (int z = 0; z <= ChunkWidth; z += _LODLevel)
                    {
                        AddVertex(0, (int)(ChunkWidth * Chunk.x) + z, (int)(ChunkWidth * Chunk.y) + y, ref heightmap, ref NO);
                    }
                }
                break;

            case 4:
                for (int z = 0; z <= ChunkWidth; z += _LODLevel)
                {
                    for (int x = 0; x <= ChunkWidth; x += _LODLevel)
                    {
                        AddVertex((int)(ChunkWidth * Chunk.x) + x, Width, (int)(ChunkWidth * Chunk.y) + z, ref heightmap, ref NO);
                    }
                }
                break;

            case 5:
                for (int x = 0; x <= ChunkWidth; x += _LODLevel)
                {
                    for (int z = 0; z <= ChunkWidth; z += _LODLevel)
                    {
                        AddVertex((int)(ChunkWidth * Chunk.x) + x, 0, (int)(ChunkWidth * Chunk.y) + z, ref heightmap, ref NO);
                    }
                }
                break;

            default:
                break;
        }
        data.v3 = NO;
        data.heightmap = heightmap;
        return data;
    }

    private void Callback(ProceduralSphere.MeshData data)
    {
        Vector3[] arr = new Vector3[data.v3.Length];

        for (int i = 0; i < data.v3.Length; i++)
        {
            arr[i] = new Vector3(data.v3[i].x, data.v3[i].y, data.v3[i].z);
        }
        norm = arr;

        for (int ve = 0; ve < arr.Length; ve++)
        {
            vert[ve] = norm[ve] * (Radius + MaxHeight * data.heightmap[ve]);
        }
        CallbackDone = true;
    }

    private IEnumerator GenerateMesh(bool forceCollider = false)
    {
        CallbackDone = false;
        int LODW = ChunkWidth / _LODLevel;

        vert = new Vector3[(LODW + 1) * (LODW + 1)];
        norm = new Vector3[vert.Length];
        uv = new Vector2[vert.Length];
        tri = new int[vert.Length * 6];
        ve = 0;

        ProceduralSphere.ProcCallback t = new ProceduralSphere.ProcCallback();
        t.Function = () => { return AddVertices(); };
        t.callback = Callback;

        AddToThread(t);

        while (!CallbackDone)
        {
            yield return null;
        }

        int ti = 0;
        for (int x = 0; x < LODW; x++)
        {
            for (int y = 0; y < LODW; y++)
            {
                CreateTriangle(ref ti, x, y);
            }
        }
        AddUV(LODW);

        mesh.Clear();
        mesh.vertices = vert;
        mesh.normals = norm;
        mesh.triangles = tri;
        mesh.uv = uv;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        if (!FirstTime)
        {
            FirstTime = true;
            AddSplash(LODW);
            mc0.sharedMesh = mesh;
            while (!SplashDone)
            {
                yield return null;
            }
            SetTargetLOD(4);
        }
        if (forceCollider && !mc0.convex)
        {
            forceCollider = false;
            mc0.convex = true;
        }
    }

    private void AddUV(int LODW)
    {
        int i = 0;
        Vector2 start = new Vector2((LODW * Chunk.x), (LODW * Chunk.y));
        int UVSize = LODW * 16;
        switch (side)
        {
            case 0:
                for (int y = 0; y <= LODW; y++)
                {
                    for (int x = 0; x <= LODW; x++, i++)
                    {
                        uv[i] = new Vector2((start.y + y) / UVSize, (start.x + x) / UVSize);
                    }
                }
                break;

            case 1:
                for (int x = 0; x <= LODW; x++)
                {
                    for (int y = 0; y <= LODW; y++, i++)
                    {
                        uv[i] = new Vector2((start.y + y) / UVSize, (start.x + x) / UVSize);
                    }
                }
                break;

            case 2:
                for (int y = 0; y <= LODW; y++)
                {
                    for (int x = LODW; x >= 0; x--, i++)
                    {
                        uv[i] = new Vector2((start.y + y) / UVSize, (start.x + x) / UVSize);
                    }
                }
                break;

            case 3:
                for (int y = 0; y <= LODW; y++)
                {
                    for (int x = 0; x <= LODW; x++, i++)
                    {
                        uv[i] = new Vector2((start.y + y) / UVSize, (start.x + x) / UVSize);
                    }
                }
                break;

            case 4:
                for (int y = 0; y <= LODW; y++)
                {
                    for (int x = 0; x <= LODW; x++, i++)
                    {
                        uv[i] = new Vector2((start.y + y) / UVSize, (start.x + x) / UVSize);
                    }
                }
                break;

            case 5:
                for (int x = 0; x <= LODW; x++)
                {
                    for (int y = 0; y <= LODW; y++, i++)
                    {
                        uv[i] = new Vector2((start.y + y) / UVSize, (start.x + x) / UVSize);
                    }
                }
                break;

            default:
                break;
        }
    }

    private void AddSplash(int LODW)
    {
        Texture2D tex = (Texture2D)mr0.material.mainTexture;
        Vector2 start = new Vector2(ChunkWidth * Chunk.x, ChunkWidth * Chunk.y);
        int i = 0;

        switch (side)
        {
            case 0:
                for (int y = 0; y <= LODW; y++)
                {
                    for (int x = 0; x <= LODW; x++, i++)
                    {
                        for (int r = Regions.Length - 1; r > 0; r--)
                        {
                            if ((Vector3.Distance(vert[i], Vector3.zero) - Radius) / MaxHeight <= Regions[r].height)
                            {
                                tex.SetPixel((int)start.y + y, (int)start.x + x, Regions[r].color);
                            }
                        }
                    }
                }
                break;

            case 1:
                for (int x = 0; x <= LODW; x++)
                {
                    for (int y = 0; y <= LODW; y++, i++)
                    {
                        for (int r = Regions.Length - 1; r > 0; r--)
                        {
                            if ((Vector3.Distance(vert[i], Vector3.zero) - Radius) / MaxHeight <= Regions[r].height)
                            {
                                tex.SetPixel((int)start.y + y, (int)start.x + x, Regions[r].color);
                            }
                        }
                    }
                }
                break;

            case 2:
                for (int y = 0; y <= LODW; y++)
                {
                    for (int x = LODW; x >= 0; x--, i++)
                    {
                        for (int r = Regions.Length - 1; r > 0; r--)
                        {
                            if ((Vector3.Distance(vert[i], Vector3.zero) - Radius) / MaxHeight <= Regions[r].height)
                            {
                                tex.SetPixel((int)start.y + y, (int)start.x + x, Regions[r].color);
                            }
                        }
                    }
                }
                break;

            case 3:
                for (int y = 0; y <= LODW; y++)
                {
                    for (int x = 0; x <= LODW; x++, i++)
                    {
                        for (int r = Regions.Length - 1; r > 0; r--)
                        {
                            if ((Vector3.Distance(vert[i], Vector3.zero) - Radius) / MaxHeight <= Regions[r].height)
                            {
                                tex.SetPixel((int)start.y + y, (int)start.x + x, Regions[r].color);
                            }
                        }
                    }
                }
                break;

            case 4:
                for (int y = 0; y <= LODW; y++)
                {
                    for (int x = 0; x <= LODW; x++, i++)
                    {
                        for (int r = Regions.Length - 1; r > 0; r--)
                        {
                            if ((Vector3.Distance(vert[i], Vector3.zero) - Radius) / MaxHeight <= Regions[r].height)
                            {
                                tex.SetPixel((int)start.y + y, (int)start.x + x, Regions[r].color);
                            }
                        }
                    }
                }
                break;

            case 5:
                for (int x = 0; x <= LODW; x++)
                {
                    for (int y = 0; y <= LODW; y++, i++)
                    {
                        for (int r = Regions.Length - 1; r > 0; r--)
                        {
                            if ((Vector3.Distance(vert[i], Vector3.zero) - Radius) / MaxHeight <= Regions[r].height)
                            {
                                tex.SetPixel((int)start.y + y, (int)start.x + x, Regions[r].color);
                            }
                        }
                    }
                }
                break;

            default:
                break;
        }
        tex.Apply();
        mr0.material.mainTexture = tex;
        SplashDone = true;
    }
}