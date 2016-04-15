using CoherentNoise.Generation.Fractal;
using System.Collections;
using UnityEngine;

public class ProceduralSphere : MonoBehaviour
{
    public int Width;
    public int Radius;
    public int MaxHeight;

    public int Octaves = 3;
    public float groundFrq = 0.005f;
    private int[] triangles;
    private Vector3[] vertices;
    private Vector3[] normals;

    private int ve;
    private Mesh mesh;

    private Vector2[] UV;
    private RidgeNoise noise;
    private PerlinNoise perlin;

    // Use this for initialization
    private void Start()
    {
        StartCoroutine(GenerateTerrain());
    }

    private int SetQuad(ref int[] triangles, int i, int v00, int v10, int v01, int v11)
    {
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }

    private void CreateTriangles()
    {
        int xW = Width + 1;
        int[] triangles = new int[xW * xW * 6 * 6];
        int i = 0;

        int ring = (Width + Width) * 2;
        int v = 0;
        for (int y = 0; y < Width; y++, v++)
        {
            for (int x = 0; x < ring - 1; x++, v++)
            {
                i = SetQuad(ref triangles, i, v, v + 1, v + ring, v + ring + 1);
            }
            i = SetQuad(ref triangles, i, v, v - ring + 1, v + ring, v + 1);
        }
        i += (ring + 1) * 6;
        v += ring + 1;
        for (int y = 0; y < Width; y++, v++)
        {
            for (int x = 0; x < Width; x++, v++)
            {
                i = SetQuad(ref triangles, i, v - 1, v, v + Width, v + Width + 1);
            }
        }
        i += (Width + 1) * 6;
        v += Width + 1;
        for (int y = 0; y < Width; y++, v++)
        {
            for (int x = 0; x < Width; x++, v++)
            {
                i = SetQuad(ref triangles, i, v + Width, v + Width + 1, v - 1, v);
            }
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Splat();
    }

    private void Splat()
    {
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float height = (Vector3.Distance(Vector3.one, vertices[i]) - Radius) / MaxHeight;
            colors[i] = new Color(height, Mathf.Max(0f, height - 0.5f), 0);
        }
        mesh.colors = colors;
    }

    private void AddVertex(int x, int y, int z)
    {
        Vector3 v = new Vector3(x, y, z) * 2f / Width - Vector3.one;
        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;
        Vector3 s;
        s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);

        normals[ve] = s;

        float plain = perlin.FractalNoise2D(s.x - 245 + s.z, s.y + 567 - s.z, Octaves, groundFrq, 0.4f) + 0.5f / 5.0f;
        float mountains = Mathf.Max(0f, noise.GetValue(s.x + 17235, s.y - 54358, s.z + 459) - 0.75f);

        float h = Mathf.Clamp01(plain + mountains);
        vertices[ve] = s * (Radius + MaxHeight * h);

        Bounds bounds = mesh.bounds;

        ve++;
    }

    private IEnumerator GenerateTerrain()
    {
        vertices = new Vector3[(Width + 1) * (Width + 1) * 6];
        normals = new Vector3[vertices.Length];
        UV = new Vector2[vertices.Length];
        if (!PlayerPrefs.HasKey("Seed"))
        {
            PlayerPrefs.SetInt("Seed", Random.Range(0, 999999));
        }
        Random.seed = PlayerPrefs.GetInt("Seed");
        noise = new RidgeNoise(PlayerPrefs.GetInt("Seed"));
        perlin = new PerlinNoise(PlayerPrefs.GetInt("Seed"));
        noise.OctaveCount = Octaves * 2;
        noise.Frequency = 0.5f;

        mesh = GetComponent<MeshFilter>().mesh = new Mesh();

        for (int y = 0; y <= Width; y++)
        {
            for (int x = 0; x <= Width; x++)
            {
                AddVertex(x, y, 0);
            }
            for (int z = 1; z <= Width; z++)
            {
                AddVertex(Width, y, z);
            }
            for (int x = Width - 1; x >= 0; x--)
            {
                AddVertex(x, y, Width);
            }
            for (int z = Width - 1; z > 0; z--)
            {
                AddVertex(0, y, z);
            }
            yield return null;
        }
        for (int z = 0; z <= Width; z++)
        {
            for (int x = 0; x <= Width; x++)
            {
                AddVertex(x, Width, z);
            }
            yield return null;
        }
        for (int z = 0; z <= Width; z++)
        {
            for (int x = 0; x <= Width; x++)
            {
                AddVertex(x, 0, z);
            }
            yield return null;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = UV;
        CreateTriangles();
    }
}