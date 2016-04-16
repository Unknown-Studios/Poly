using UnityEngine;
using System.Collections;
using CoherentNoise.Generation.Fractal;

#if UNITY_EDITOR

using UnityEditor;

#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer))]
public class LOD : MonoBehaviour
{
    public int TargetLOD = 4;

    private Vector3 ppos;
    public float Distance;
    private Mesh mesh;
    private int _LODLevel;
    public Vector2 Chunk;
    public int side;

    public int Radius;
    public int MaxHeight;

    public int Octaves = 3;
    private int LODLevel;
    public float groundFrq = 0.25f;

    private void Update()
    {
#if UNITY_EDITOR
        if (SceneView.lastActiveSceneView != null)
        {
            ppos = SceneView.lastActiveSceneView.camera.ViewportToWorldPoint(new Vector3(1, 1, SceneView.lastActiveSceneView.camera.nearClipPlane));
        }

#endif

        if (Game.player != null)
        {
            ppos = Game.player.transform.position;
        }
        if (ppos != Vector3.zero)
        {
            Distance = Vector3.Distance(ppos, mc0.bounds.center);
        }
        //Calculate which LODLevel should be used.
        if (Distance < 100)
        {
            TargetLOD = 0;
        }
        else if (Distance < 200)
        {
            TargetLOD = 1;
        }
        else if (Distance < 400)
        {
            TargetLOD = 2;
        }
        else if (Distance < 800)
        {
            TargetLOD = 3;
        }
        else
        {
            TargetLOD = 4;
        }

        if (LODLevel != TargetLOD)
        {
            LODLevel = TargetLOD;
            _LODLevel = (int)Mathf.Pow(2f, LODLevel);
            StartCoroutine(GenerateMesh());
        }
    }

    private int ve;
    public int Width;

    private Vector3[] vertices;

    private void CreateTriangle(ref int[] triangles, ref int index, int x, int y)
    {
        int LODWidth = (16 / _LODLevel) + 1;

        triangles[index] = (y * LODWidth) + x;
        triangles[index + 1] = ((y + 1) * LODWidth) + x;
        triangles[index + 2] = (y * LODWidth) + x + 1;

        triangles[index + 3] = ((y + 1) * LODWidth) + x;
        triangles[index + 4] = ((y + 1) * LODWidth) + x + 1;
        triangles[index + 5] = (y * LODWidth) + x + 1;
        index += 6;
    }

    private int AddVertex(ref Vector3[] vert, ref Vector3[] norm, int x, int y, int z)
    {
        Vector3 v = new Vector3(x, y, z) * 2f / Width - Vector3.one;
        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;
        Vector3 s = new Vector3(x, y, z);

        s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);

        float plain = (pink.GetValue(s.x - 245, s.y, s.z + 567) + 1.0f) / 2.0f;
        float mountains = Mathf.Max(0f, noise.GetValue(s.x + 17235 - s.y, s.y - 54358, s.z + 459) - 0.75f);

        float h = Mathf.Clamp01(plain + mountains);
        vert[ve] = s * (Radius + (MaxHeight * h));
        norm[ve] = vert[ve];

        ve++;
        return ve;
    }

    private RidgeNoise noise;
    private PinkNoise pink;
    private MeshRenderer mr0;

    private void Start()
    {
        _LODLevel = (int)Mathf.Pow(2f, TargetLOD);
        Random.seed = PlayerPrefs.GetInt("Seed");
        noise = new RidgeNoise(PlayerPrefs.GetInt("Seed"));
        pink = new PinkNoise(PlayerPrefs.GetInt("Seed"));
        pink.Lacunarity = 4f;
        pink.Persistence = 0.5f;
        pink.OctaveCount = Octaves;
        pink.Frequency = groundFrq;

        noise.OctaveCount = Octaves * 2;
        noise.Exponent = 2.0f;
        noise.Gain = 1.2f;
        noise.Frequency = 0.5f;
        mesh = GetComponent<MeshFilter>().mesh;
        mc0 = GetComponent<MeshCollider>();
        mr0 = GetComponent<MeshRenderer>();
        mr0.material = terrainMaterial;

        StartCoroutine(GenerateMesh());
    }

    public Material terrainMaterial;
    private MeshCollider mc0;
    private int ChunkWidth = 16;

    private IEnumerator GenerateMesh()
    {
        int LODW = ChunkWidth / _LODLevel;

        Vector3[] v0 = new Vector3[(LODW + 1) * (LODW + 1)];
        Vector3[] n0 = new Vector3[v0.Length];
        Vector2[] u0 = new Vector2[v0.Length];
        int[] t0 = new int[v0.Length * 6];
        ve = 0;

        switch (side)
        {
            case 0:
                for (int y = 0; y <= ChunkWidth; y += _LODLevel)
                {
                    for (int x = 0; x <= ChunkWidth; x += _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, (int)(ChunkWidth * Chunk.x) + x, (int)(ChunkWidth * Chunk.y) + y, 0);
                    }
                }
                break;

            case 1:
                for (int y = 0; y <= ChunkWidth; y += _LODLevel)
                {
                    for (int z = 0; z <= ChunkWidth; z += _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, Width, (int)(ChunkWidth * Chunk.x) + y, (int)(ChunkWidth * Chunk.y) + z);
                    }
                }
                break;

            case 2:
                for (int y = 0; y <= ChunkWidth; y += _LODLevel)
                {
                    for (int x = ChunkWidth; x >= 0; x -= _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, (int)(ChunkWidth * Chunk.x) + x, (int)(ChunkWidth * Chunk.y) + y, Width);
                    }
                }
                break;

            case 3:
                for (int y = 0; y <= ChunkWidth; y += _LODLevel)
                {
                    for (int z = 0; z <= ChunkWidth; z += _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, 0, (int)(ChunkWidth * Chunk.x) + z, (int)(ChunkWidth * Chunk.y) + y);
                    }
                }
                break;

            case 4:
                for (int z = 0; z <= ChunkWidth; z += _LODLevel)
                {
                    for (int x = 0; x <= ChunkWidth; x += _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, (int)(ChunkWidth * Chunk.x) + x, Width, (int)(ChunkWidth * Chunk.y) + z);
                    }
                }
                break;

            case 5:
                for (int x = 0; x <= ChunkWidth; x += _LODLevel)
                {
                    for (int z = 0; z <= ChunkWidth; z += _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, (int)(ChunkWidth * Chunk.x) + x, 0, (int)(ChunkWidth * Chunk.y) + z);
                    }
                }
                break;

            default:
                Debug.LogError("No side defined");
                break;
        }
        int ti = 0;
        for (int x = 0; x < LODW; x++)
        {
            for (int y = 0; y < LODW; y++)
            {
                CreateTriangle(ref t0, ref ti, x, y);
            }
        }

        for (int x = 0; x < LODW; x++)
        {
            for (int y = 0; y < LODW; y++)
            {
                u0[(y * LODW) + x] = new Vector2(1.0f / (float)ChunkWidth, 1.0f / (float)ChunkWidth);
            }
        }

        mesh.Clear();
        mesh.vertices = v0;
        mesh.normals = n0;
        mesh.triangles = t0;
        mesh.uv = u0;

        vertices = v0;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mc0.sharedMesh = mesh;
        yield return null;
    }
}