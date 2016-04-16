using CoherentNoise.Generation.Fractal;
using System.Collections;
using UnityEngine;

public class ProceduralSphere : MonoBehaviour
{
    public int WidthTick;

    public int Width;

    public int Radius;
    public int MaxHeight;

    public int Octaves = 3;
    public float groundFrq = 0.005f;

    public int LODLevel;

    private int[] triangles;
    private Vector3[] vertices;
    private Vector3[] normals;

    private int ve;
    private Mesh mesh;

    private Vector2[] UV;
    private RidgeNoise noise;
    private PerlinNoise perlin;

    private GameObject[] sides;

    private int _LODLevel;

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

    private void CreateTriangle(ref int[] triangles, ref int index, int x, int y)
    {
        int i = (y * Width) + x;
        int xw = Width + 1;

        int LODWidth = Width / _LODLevel;

        triangles[index] = (y * LODWidth) + x;
        triangles[index + 1] = ((y + 1) * LODWidth) + x;
        triangles[index + 2] = (y * LODWidth) + x + 1;

        triangles[index + 3] = ((y + 1) * LODWidth) + x;
        triangles[index + 4] = ((y + 1) * LODWidth) + x + 1;
        triangles[index + 5] = (y * LODWidth) + x + 1;
        index += 6;
    }

    private void Splat()
    {
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float height = (Vector3.Distance(Vector3.one, vertices[i]) - Radius) / MaxHeight;
            colors[i] = new Color(height, 1.0f - height, 0);
        }
        mesh.colors = colors;
    }

    private int AddVertex(ref Vector3[] vert, ref Vector3[] norm, int x, int y, int z)
    {
        Vector3 v = new Vector3(x, y, z) * 2f / Width - Vector3.one;
        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;
        Vector3 s = new Vector3(x, y, z);
        /*s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);

        norm[ve] = s;

        float plain = perlin.FractalNoise2D(s.x - 245 + s.z, s.y + 567 - s.z, Octaves, groundFrq, 0.4f) + 0.5f / 5.0f;
        float mountains = Mathf.Max(0f, noise.GetValue(s.x + 17235, s.y - 54358, s.z + 459) - 0.75f);

        float h = Mathf.Clamp01(plain + mountains);*/
        vert[ve] = s * (Radius + MaxHeight);

        ve++;
        return ve;
    }

    private void AddSide(int side)
    {
        GameObject s0 = new GameObject();
        sides[side] = s0;
        MeshFilter mf0 = s0.AddComponent<MeshFilter>();
        s0.AddComponent<MeshRenderer>();
        Mesh m0 = mf0.mesh = new Mesh();
        m0.name = "Side #" + side;
        s0.name = "Side #" + side;

        int LODW = Width / _LODLevel;

        Vector3[] v0 = new Vector3[(LODW + 1) * (LODW + 1)];
        Vector3[] n0 = new Vector3[v0.Length];
        int[] t0 = new int[v0.Length * 6];
        ve = 0;

        switch (side)
        {
            case 0:
                for (int y = 0; y < LODW + 1; y += _LODLevel)
                {
                    for (int x = 0; x < LODW + 1; x += _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, x, y, 0);
                    }
                }
                break;

            case 1:
                for (int y = 0; y <= LODW; y += _LODLevel)
                {
                    for (int z = 0; z <= LODW; z += _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, Width, y, z);
                    }
                }
                break;

            case 2:
                for (int y = 0; y <= LODW; y += _LODLevel)
                {
                    for (int x = LODW; x >= 0; x -= _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, x, y, Width);
                    }
                }
                break;

            case 3:
                for (int y = 0; y <= LODW; y += _LODLevel)
                {
                    for (int z = LODW; z > 0; z -= _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, 0, y, z);
                    }
                }
                break;

            case 4:
                for (int z = 0; z <= LODW; z += _LODLevel)
                {
                    for (int x = 0; x <= LODW; x += _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, x, Width, z);
                    }
                }
                break;

            case 5:
                for (int x = 0; x <= LODW; x += _LODLevel)
                {
                    for (int z = 0; z <= LODW; z += _LODLevel)
                    {
                        AddVertex(ref v0, ref n0, x, 0, z);
                    }
                }
                break;

            default:
                Debug.LogError("No side defined");
                break;
        }
        int ti = 0;

        for (int x = 0; x < LODW + 1; x++)
        {
            for (int y = 0; y < LODW + 1; y++)
            {
                CreateTriangle(ref t0, ref ti, x, y);
            }
        }

        m0.vertices = v0;
        m0.normals = n0;
        m0.triangles = t0;

        m0.RecalculateBounds();
        m0.RecalculateNormals();
    }

    private IEnumerator GenerateTerrain()
    {
        vertices = new Vector3[(Width + 1) * (Width + 1) * 6];
        sides = new GameObject[6];

        if (!PlayerPrefs.HasKey("Seed"))
        {
            PlayerPrefs.SetInt("Seed", Random.Range(0, 999999));
        }
        Random.seed = PlayerPrefs.GetInt("Seed");
        noise = new RidgeNoise(PlayerPrefs.GetInt("Seed"));
        perlin = new PerlinNoise(PlayerPrefs.GetInt("Seed"));
        noise.OctaveCount = Octaves * 2;
        noise.Frequency = 0.5f;

        _LODLevel = (LODLevel == 0) ? 1 : LODLevel * 2;
        for (int i = 0; i < 6; i++)
        {
            AddSide(i);
            yield return null;
        }
        yield return null;
    }
}