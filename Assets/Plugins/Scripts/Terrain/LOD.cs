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
    public bool isDone;
    public ProceduralSphere PS;
    public ProceduralSphere.VoronoiPoint[] points;
    private const int ChunkWidth = 16;
    private Mesh mesh;
    private int _LODLevel;
    private int LODLevel;
    private int ve;
    private RidgeNoise noise;
    private BillowNoise hills;
    private PinkNoise pink;
    private MeshRenderer mr0;
    private MeshCollider mc0;
	private MeshFilter mf0;
    private Vector3[] vert;
    private Vector3[] norm;
    private int[] tri;
    private Vector2[] uv;

    private Color[] colors;

    private bool FirstTime = false;

    private bool CallbackDone;

    public void UpdateLODSettings(Vector3 pos, int[] lodlevels)
    {
        if (mc0 == null)
        {
            mc0 = GetComponent<MeshCollider>();
        }
        Distance = Vector3.Distance(pos, mc0.bounds.center);

        //Calculate which LODLevel should be used.
        TargetLOD = 5;
        for (int x = 0; x < lodlevels.Length; x++)
        {
            if (Distance < lodlevels[x])
            {
                TargetLOD = x;
                break;
            }
        }

        SetTargetLOD(TargetLOD);
    }

    public void SetTargetLOD(int TargetLOD)
    {
        if (LODLevel == TargetLOD)
        {
            return;
        }
        LODLevel = TargetLOD;
        _LODLevel = (int)Mathf.Pow(2f, LODLevel);
        StartCoroutine(GenerateMesh(LODLevel == 0));
    }

    private void CreateTriangle(int LODW, ref int index, ref int[] triangles, int x, int y)
    {
        if (LODW != Mathf.Sqrt(triangles.Length / 6) - 1)
        {
            LODW = (int)Mathf.Sqrt(triangles.Length / 6) - 1;
        }
        int LODWidth = LODW + 1;

        triangles[index] = (y * LODWidth) + x;
        triangles[index + 1] = ((y + 1) * LODWidth) + x;
        triangles[index + 2] = (y * LODWidth) + x + 1;

        triangles[index + 3] = ((y + 1) * LODWidth) + x;
        triangles[index + 4] = ((y + 1) * LODWidth) + x + 1;
        triangles[index + 5] = (y * LODWidth) + x + 1;
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

        float plain = (pink.GetValue(x / 1.5f, y / 1.5f, z / 1.5f) + 1.0f) / 4.0f;

        float val = noise.GetValue(x - 549, y + 2585, z + 54);
        float n = val < 0 ? 0 : val;
        val = (n - 0.5f) / 2.5f;
        float mountains = val < 0 ? 0 : val;

        val = hills.GetValue(x + 549, y - 2585, z - 544);
        float hill = val < 0 ? 0 : val;
        val = (hill - 0.25f) / 7.5f;
        hill = val < 0 ? 0 : val;

        H[ve] = plain + mountains + hill;
        N[ve] = s;

        ve++;
    }

    private void Start()
    {
        _LODLevel = (int)Mathf.Pow(2f, TargetLOD);
        Random.seed = PlayerPrefs.GetInt("Seed");
        noise = new RidgeNoise(PlayerPrefs.GetInt("Seed"));
        pink = new PinkNoise(PlayerPrefs.GetInt("Seed"));
        hills = new BillowNoise(PlayerPrefs.GetInt("Seed"));

        pink.Frequency = 0.005f;
        pink.Lacunarity = 1.2f;
        pink.Persistence = 0.17f;

        noise.OctaveCount = Octaves * 2;
        noise.Frequency = 0.01f;
        noise.Gain = 1f;
        noise.Exponent = 2f;

        hills.OctaveCount = 2;
        hills.Lacunarity = 0.5f;
        hills.Frequency = 0.01f;

        mesh = GetComponent<MeshFilter>().mesh;
		mf0 = GetComponent<MeshFilter> ();
        mr0 = GetComponent<MeshRenderer>();
        mc0 = GetComponent<MeshCollider>();
        mr0.material = terrainMaterial;

        StartCoroutine(WaitForPoints());
    }

    private IEnumerator WaitForPoints()
    {
        while (points == null)
        {
            yield return null;
        }
        StartCoroutine(GenerateMesh());
    }

    private void AddToThread(ProceduralSphere.ProcCallback item)
    {
        if (thread == null)
        {
            Debug.LogError("thread doesn't exist");
        }
        thread.Add(item);
        if (!thread.Started)
        {
            StartCoroutine(thread.Start(PlayerPrefs.GetInt("Seed")));
        }
    }

    private ProceduralSphere.MeshData AddVertices(int LODW)
    {
        ProceduralSphere.MeshData data = new ProceduralSphere.MeshData();

        ProceduralSphere.V3[] NO = new ProceduralSphere.V3[(LODW + 1) * (LODW + 1)];
        float[] heightmap = new float[NO.Length];
        int[] triangles = new int[NO.Length * 6];

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
        int ti = 0;
        for (int x = 0; x < LODW; x++)
        {
            for (int y = 0; y < LODW; y++)
            {
                CreateTriangle(LODW, ref ti, ref triangles, x, y);
            }
        }

        data.v3 = NO;
        data.heightmap = heightmap;
        data.triangles = triangles;
        if (FirstTime)
        {
            Callback(data);
            return null;
        }

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

        for (int ve = 0; ve < norm.Length; ve++)
        {
            vert[ve] = norm[ve].normalized * (Radius + (MaxHeight * data.heightmap[ve]));
        }
        for (int x = 0; x < vert.Length; x++)
        {
            vert[x] = vert[x].normalized * (Radius + (MaxHeight * data.heightmap[x]));
        }
        tri = data.triangles;
        CallbackDone = true;
    }

    private IEnumerator GenerateMesh(bool forceCollider = false)
    {
		if (_LODLevel == 5) {
			mesh.Clear();
			mf0.mesh = null;
			yield break;
		}
        //Reset
        CallbackDone = false;

        //Define variables
        int LODW = ChunkWidth / _LODLevel;
        vert = new Vector3[(LODW + 1) * (LODW + 1)];
        norm = new Vector3[vert.Length];
        tri = new int[vert.Length * 6];
        ve = 0;

        //Add to queue on side-thread
        if (!FirstTime)
        {
            //Set up data needed for thread
            ProceduralSphere.ProcCallback t = new ProceduralSphere.ProcCallback();
            t.Function = () => { return AddVertices(LODW); };
            t.callback = Callback;
            t.LODW = LODW;

            AddToThread(t);
        }
        else
        {
            AddVertices(LODW);
        }
        while (!CallbackDone)
        {
            yield return null;
        }

        //Calculate UVs
        uv = new Vector2[vert.Length];
        AddUV();

        //Reset mesh and set values
        mesh.Clear();
        mesh.vertices = vert;
		mesh.normals = CalculateNormals (norm);
        mesh.triangles = tri;
        mesh.uv = uv;

        mesh.RecalculateBounds();

        //First time setup
        if (!FirstTime)
        {
            FirstTime = true;
            yield return null;
            mc0.sharedMesh = mesh;
            PS.queue.Enqueue(gameObject);
            while (!mc0.convex)
            {
                yield return null;
            }
		}
		if (mc0.sharedMesh == null && mf0.sharedMesh != null) {
			mc0.sharedMesh = mf0.sharedMesh;
			mc0.convex = true;
		}

        //If player nears a collider before it is generated force its generation
        if (forceCollider && !mc0.convex)
        {
            mc0.convex = true;
        }
    }

	Vector3[] CalculateNormals(Vector3[] normals) {
		//Replace with custom normals calculations
		mesh.normals = normals;
		mesh.RecalculateNormals ();
		return mesh.normals;
	}

    private void AddUV()
    {
        int i = 0;
        Vector2 start = new Vector2(ChunkWidth * Chunk.x, ChunkWidth * Chunk.y) / Width;

        switch (side)
        {
            case 0:
			for (int y = 0; y <= ChunkWidth; y+= _LODLevel)
                {
				for (int x = 0; x <= ChunkWidth; x+= _LODLevel, i++)
                {
            		uv[i] = start + (new Vector2(x, y) / (1.0f * Width));
                }
            }
                break;

            case 1:
			for (int x = 0; x <= ChunkWidth; x+=_LODLevel)
                {
				for (int y = 0; y <= ChunkWidth; y+=_LODLevel, i++)
				{
					uv[i] = start + (new Vector2(x,y) / (1.0f * Width));
                    }
                }
                break;

            case 2:
			for (int y = 0; y <= ChunkWidth; y+=_LODLevel)
                {
				for (int x = ChunkWidth; x >= 0; x-=_LODLevel, i++)
				{
					uv[i] = start + (new Vector2(x, y) / (1.0f * Width));
                    }
                }
                break;

            case 3:
			for (int y = 0; y <= ChunkWidth; y+=_LODLevel)
                {
				for (int x = 0; x <= ChunkWidth; x+=_LODLevel, i++)
				{
					uv[i] = start + (new Vector2(x, y) / (1.0f * Width));
                    }
                }
                break;

            case 4:
			for (int y = 0; y <= ChunkWidth; y+=_LODLevel)
                {
				for (int x = 0; x <= ChunkWidth; x+=_LODLevel, i++)
				{
					uv[i] = start + (new Vector2(x, y) / (1.0f * Width));
                    }
                }
                break;
            //Bottom
            case 5:
			for (int x = 0; x <= ChunkWidth; x += _LODLevel)
                {
				for (int y = 0; y <= ChunkWidth; y += _LODLevel, i++)
				{
					uv[i] = start + (new Vector2(x, y) / (1.0f * Width));
                    }
                }
                break;

            default:
                break;
        }
    }
}