using CoherentNoise.Generation.Fractal;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    #region Fields

    public Texture2D map;
    public float[,] original;
    public Material WaterMaterial;

    #endregion Fields

    #region Methods

    private void CreateMesh()
    {
        int hw = Terrain.activeTerrain.terrainData.heightmapWidth;
        TerrainData td = Terrain.activeTerrain.terrainData;
        float[,] heightmap = td.GetHeights(0, 0, hw, hw);

        var river = new RidgeNoise(Random.seed);
        river.OctaveCount = 2;
        river.Frequency = 0.01f;

        float WaterLevel = 10f;

        int ChunkWidth = hw / 8;
        int ChunkCount = hw / ChunkWidth;

        float scale = td.size.x / hw;

        Vector2 uvScale = new Vector2(1.0f / hw, 1.0f / hw);

        for (int cx = 0; cx < ChunkCount; cx++)
        {
            for (int cy = 0; cy < ChunkCount; cy++)
            {
                int CW = hw / ChunkCount;
                GameObject water = new GameObject();
                Vector3 p = new Vector3(cx * ChunkWidth, 0f, cy * ChunkWidth);

                water.transform.localScale = new Vector3(scale, 0.95f, scale);
                water.transform.position = new Vector3(cx * (ChunkWidth * scale), -2f, cy * (ChunkWidth * scale));
                water.name = "Water";
                water.transform.parent = Terrain.activeTerrain.transform;

                MeshRenderer mr = water.AddComponent<MeshRenderer>();
                Mesh w = new Mesh();
                MeshFilter mf = water.AddComponent<MeshFilter>();
                mr.material = WaterMaterial;

                Vector3[] Vertices = new Vector3[(CW + 1) * (CW + 1)];
                Vector2[] UV = new Vector2[Vertices.Length];
                int[] triangles = new int[Vertices.Length * 6];
                Vector3[] vertices = new Vector3[triangles.Length];

                for (int y = 0; y < CW; y++)
                {
                    for (int x = 0; x < CW; x++)
                    {
                        Vector2 curPos = new Vector2((p.x + x), (p.z + y));
                        float height = heightmap[(int)curPos.y, (int)curPos.x] * Terrain.activeTerrain.terrainData.size.y;
                        if (height < WaterLevel)
                        {
                            height = WaterLevel;
                        }

                        float v = Mathf.Max(0f, river.GetValue(curPos.x, curPos.y, 0f) - 0.875f) / 25.0f;
                        Vertices[y * CW + x] = new Vector3(x, height + transform.position.y, y);
                        UV[y * CW + x] = Vector2.Scale(new Vector2(x, y), uvScale);
                        heightmap[(int)curPos.y, (int)curPos.x] -= v;
                    }
                }

                int index = 0;
                for (int y = 0; y < CW - 1; y++)
                {
                    for (int x = 0; x < CW - 1; x++)
                    {
                        triangles[index] = (y * CW) + x;
                        triangles[index + 1] = ((y + 1) * CW) + x;
                        triangles[index + 2] = ((y + 1) * CW) + x + 1;

                        triangles[index + 3] = (y * CW) + x;
                        triangles[index + 4] = ((y + 1) * CW) + x + 1;
                        triangles[index + 5] = (y * CW) + x + 1;
                        index += 6;
                    }
                }
                for (int i = 0; i < triangles.Length; i++)
                {
                    vertices[i] = Vertices[triangles[i]];
                    triangles[i] = i;
                }
                w.vertices = vertices;
                w.triangles = triangles;
                w.RecalculateBounds();
                w.RecalculateNormals();

                mf.mesh = w;
            }
        }
        td.SetHeights(0, 0, heightmap);
    }

    private void Start()
    {
        map = new Texture2D(Terrain.activeTerrain.terrainData.heightmapWidth, Terrain.activeTerrain.terrainData.heightmapWidth);
        CreateMesh();
        map.Apply();
    }

    #endregion Methods
}