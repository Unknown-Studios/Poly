using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[Serializable, RequireComponent(typeof(ProceduralTree))]
public class ProceduralSphere : MonoBehaviour
{
    public int WidthTick;

    public int Width;

    public int Radius = 1000;
    public int MaxHeight = 300;

    public int Seed;

    public bool done;

    public int Octaves = 2;
    public Material TerrainMaterial;
    public string currentAction;
    public float progress;

    public Region[] Regions;

    public AnimationCurve curve;
    public float scale = 1.0f;
    public Queue<GameObject> queue;
    public List<GameObject> Done;
    public bool isDone;
    public VoronoiPoint[] points;
    public Biome[] biomes;
    public ProceduralTree.TreeClass[] trees;
    public Material treeMaterial;
    private int ve;
    private GameObject[] sides;
    private int numSides = 0;

    private ThreadedJob thread;

    private int colCount;

    private bool Spawned;

    private ProceduralTree.TreeMesh[,] treeMesh;

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

            SpawnTree();
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

    private void SpawnTree()
    {
        treeMesh = new ProceduralTree.TreeMesh[trees.Length, 5];

        for (int tree = 0; tree < trees.Length; tree++)
        {
            for (int i = 0; i < treeMesh.GetLength(1); i++)
            {
                treeMesh[tree, i] = ProceduralTree.GenerateTree(trees[tree]);
            }
        }

        Vector3 position = Random.onUnitSphere * Radius;
        RaycastHit rayhit;
        if (Physics.Raycast(position, -position.normalized, out rayhit))
        {
            if (rayhit.transform.GetComponent<Renderer>() != null)
            {
                Vector3 SpawnPos = rayhit.point;
                Quaternion SpawnRot = Quaternion.Euler(SpawnPos.normalized);

                Texture2D tex = rayhit.transform.GetComponent<Renderer>().material.mainTexture as Texture2D;
                int x = Mathf.RoundToInt(rayhit.textureCoord.x * tex.width);
                int y = Mathf.RoundToInt(rayhit.textureCoord.y * tex.height);

                Color col = tex.GetPixel(x, y);

                Biome curBiome;
                int TreeType = 0;
                for (int biome = 0; biome < biomes.Length; biome++)
                {
                    if (biomes[biome].biomeColor == col)
                    {
                        curBiome = biomes[biome];
                        //Don't spawn tree if no treetypes were found.
                        if (curBiome.treeTypes.Length == 0)
                        {
                            return;
                        }
                        TreeType = curBiome.treeTypes[Random.Range(0, curBiome.treeTypes.Length)];
                        break;
                    }
                }

                ProceduralTree.TreeMesh tm = treeMesh[TreeType, Random.Range(0, treeMesh.GetLength(1))];

                GameObject finalTree = new GameObject("Tree: " + TreeType);
                finalTree.transform.position = SpawnPos;
                finalTree.transform.rotation = SpawnRot;

                MeshFilter treeFilter = finalTree.AddComponent<MeshFilter>();
                MeshRenderer treeRender = finalTree.AddComponent<MeshRenderer>();
                MeshCollider treeCol = finalTree.AddComponent<MeshCollider>();
                treeFilter.sharedMesh = tm.mesh;
                treeRender.material = treeMaterial;
                treeRender.material.mainTexture = tm.uvTex;
                if (treeCol.sharedMesh != null)
                {
                    treeCol.sharedMesh.Clear();
                }
                treeCol.sharedMesh = tm.mesh;
            }
        }
    }

    //Called on script load.
    private void Start()
    {
		if (SceneManager.GetActiveScene ().name != "Game") {
			OnBeforeSpawn (Vector3.zero);
		}
    }

    private void Awake()
    {
        PlayerPrefs.SetInt("Seed", Seed);
        PlayerPrefs.Save();

        points = new VoronoiPoint[25];

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

            tex = new Texture2D(Width, Width);
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
                current.GetComponent<LOD>().SetTargetLOD(4);
                Done.Add(current);
            }
            colCount++;
            yield return null;
			if (queue.Count == 0 && maxCheck <= 10) {
				yield return new WaitForSeconds(1.0f);
				maxCheck++;
				StartCoroutine (AddColliders());
			}
        }
    }

	private Texture2D tex;

	private IEnumerator AddSplashmap(int side) {
		while (!isDone) {
			yield return null;
		}

		int q = 0;
		switch (side)
        {
            case 0:
				for (int y = 0; y < Width; y++)
				{
					for (int x = 0; x < Width; x++)
					{
						RaycastHit Rayhit0;
						Vector3 vertexPos = new Vector3(x,y,0);
					if (Physics.Linecast(vertexPos.normalized * (Radius + MaxHeight),Vector3.zero, out Rayhit0)) {
							for (int r = Regions.Length - 1; r >= 0; r--) {
								if (q == 25) {
									Debug.Log ((Vector3.Distance(Rayhit0.point, Vector3.zero)));
									Debug.DrawLine (Rayhit0.point, Vector3.zero, Color.red, 20.0f);									
								}
							if (Mathf.Clamp01((Vector3.Distance(Rayhit0.point, Vector3.zero) - Radius) / MaxHeight) <= Regions[r].height)
								{
									if (Regions[r].Biome)
									{
										ProceduralSphere.VoronoiPoint closest = points[0];
										for (int b = 1; b < points.Length; b++)
										{
											if (Vector3.Distance(points[b].point, vertexPos) < Vector3.Distance(points[b].point, closest.point))
											{
												closest = points[b];
											}
										}
										tex.SetPixel((int)Rayhit0.textureCoord.x, (int)Rayhit0.textureCoord.y, closest.biome.biomeColor);
									}
									else
									{
										tex.SetPixel((int)Rayhit0.textureCoord.x, (int)Rayhit0.textureCoord.y, Regions[r].color);
									}
							}
							q++;
							if (q % 5 == 0)
							{
								yield return null;
							}
							}
						}
                    }
                }
                break;

            case 1:
			for (int y = 0; y < Width; y++)
			{
				for (int z = 0; z < Width; z++)
				{
                        for (int r = Regions.Length - 1; r >= 0; r--)
                        {
						RaycastHit Rayhit1;
						Vector3 vertexPos = new Vector3(Width,y,z);
						if (Physics.Raycast(vertexPos.normalized * Radius,-vertexPos.normalized, out Rayhit1)) {
							if (Mathf.Clamp01((Vector3.Distance(Rayhit1.point, Vector3.zero) - Radius) / MaxHeight) <= Regions[r].height)
							{
								if (Regions[r].Biome)
								{
									ProceduralSphere.VoronoiPoint closest = points[0];
									for (int b = 1; b < points.Length; b++)
									{
										if (Vector3.Distance(points[b].point, vertexPos) < Vector3.Distance(points[b].point, closest.point))
										{
											closest = points[b];
										}
									}
									tex.SetPixel((int)Rayhit1.textureCoord.x, (int)Rayhit1.textureCoord.y, closest.biome.biomeColor);
									q++;
									if (q % 5 == 0)
									{
										yield return null;
									}
								}
								else
								{
									tex.SetPixel((int)Rayhit1.textureCoord.x, (int)Rayhit1.textureCoord.y, Regions[r].color);
								}
							}
						}
                        }
                    }
                }
                break;

            case 2:
			for (int y = 0; y < Width; y++)
			{
				for (int x = Width; x >= 0; x--)
				{
                        for (int r = Regions.Length - 1; r >= 0; r--)
                        {
						RaycastHit Rayhit2;
						Vector3 vertexPos = new Vector3(x,y,Width);
						if (Physics.Raycast(vertexPos.normalized * Radius,-vertexPos.normalized, out Rayhit2)) {
							if (Mathf.Clamp01((Vector3.Distance(Rayhit2.point, Vector3.zero) - Radius) / MaxHeight) <= Regions[r].height)
							{
								if (Regions[r].Biome)
								{
									ProceduralSphere.VoronoiPoint closest = points[0];
									for (int b = 1; b < points.Length; b++)
									{
										if (Vector3.Distance(points[b].point, vertexPos) < Vector3.Distance(points[b].point, closest.point))
										{
											closest = points[b];
										}
									}
									tex.SetPixel((int)Rayhit2.textureCoord.x, (int)Rayhit2.textureCoord.y, closest.biome.biomeColor);
									q++;
									if (q % 5 == 0)
									{
										yield return null;
									}
								}
								else
								{
									tex.SetPixel((int)Rayhit2.textureCoord.x, (int)Rayhit2.textureCoord.y, Regions[r].color);
								}
							}
						}
						}
                    }
                }
                break;

            case 3:
			for (int y = 0; y < Width; y++)
			{
				for (int z = 0; z < Width; z++)
				{
                        for (int r = Regions.Length - 1; r >= 0; r--)
                        {
						RaycastHit Rayhit3;
						Vector3 vertexPos = new Vector3(0,y,z);
						if (Physics.Raycast(vertexPos.normalized * Radius,-vertexPos.normalized, out Rayhit3)) {
							if (Mathf.Clamp01((Vector3.Distance(Rayhit3.point, Vector3.zero) - Radius) / MaxHeight) <= Regions[r].height)
							{
								if (Regions[r].Biome)
								{
									ProceduralSphere.VoronoiPoint closest = points[0];
									for (int b = 1; b < points.Length; b++)
									{
										if (Vector3.Distance(points[b].point, vertexPos) < Vector3.Distance(points[b].point, closest.point))
										{
											closest = points[b];
										}
									}
									tex.SetPixel((int)Rayhit3.textureCoord.x, (int)Rayhit3.textureCoord.y, closest.biome.biomeColor);
									q++;
									if (q % 5 == 0)
									{
										yield return null;
									}
								}
								else
								{
									tex.SetPixel((int)Rayhit3.textureCoord.x, (int)Rayhit3.textureCoord.y, Regions[r].color);
								}
							}
						}
                        }
                    }
                }
                break;

            case 4:
			for (int z = 0; z < Width; z++)
			{
				for (int x = 0; x < Width; x++)
				{
                        for (int r = Regions.Length - 1; r >= 0; r--)
                        {
						RaycastHit Rayhit4;
						Vector3 vertexPos = new Vector3(x,Width,z);
						if (Physics.Raycast(vertexPos.normalized * Radius,-vertexPos.normalized, out Rayhit4)) {
							if (Mathf.Clamp01((Vector3.Distance(Rayhit4.point, Vector3.zero) - Radius) / MaxHeight) <= Regions[r].height)
							{
								if (Regions[r].Biome)
								{
									ProceduralSphere.VoronoiPoint closest = points[0];
									for (int b = 1; b < points.Length; b++)
									{
										if (Vector3.Distance(points[b].point, vertexPos) < Vector3.Distance(points[b].point, closest.point))
										{
											closest = points[b];
										}
									}
									tex.SetPixel((int)Rayhit4.textureCoord.x, (int)Rayhit4.textureCoord.y, closest.biome.biomeColor);
									q++;
									if (q % 5 == 0)
									{
										yield return null;
									}
								}
								else
								{
									tex.SetPixel((int)Rayhit4.textureCoord.x, (int)Rayhit4.textureCoord.y, Regions[r].color);
								}
							}
						}
                        }
                    }
                }
                break;

            case 5:
			for (int x = 0; x < Width; x++)
			{
				for (int z = 0; z < Width; z++)
				{
                        for (int r = Regions.Length - 1; r >= 0; r--)
                        {
							RaycastHit Rayhit5;
						Vector3 vertexPos = new Vector3(x,0,z);
							if (Physics.Raycast(vertexPos.normalized * Radius,-vertexPos.normalized, out Rayhit5)) {
								if (Mathf.Clamp01((Vector3.Distance(Rayhit5.point, Vector3.zero) - Radius) / MaxHeight) <= Regions[r].height)
								{
									if (Regions[r].Biome)
									{
										ProceduralSphere.VoronoiPoint closest = points[0];
										for (int b = 1; b < points.Length; b++)
										{
											if (Vector3.Distance(points[b].point, vertexPos) < Vector3.Distance(points[b].point, closest.point))
											{
												closest = points[b];
											}
										}
									tex.SetPixel((int)Rayhit5.textureCoord.x, (int)Rayhit5.textureCoord.y, closest.biome.biomeColor);
										q++;
										if (q % 5 == 0)
										{
											yield return null;
										}
									}
									else
									{
									tex.SetPixel((int)Rayhit5.textureCoord.x, (int)Rayhit5.textureCoord.y, Regions[r].color);
									}
								}
							}
                        }
                    }
                }
                break;

            default:
                break;
        }
		tex.Apply ();
	}

	private int maxCheck = 0;

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
		for (int i = 0; i < 6; i++) {
			StartCoroutine (AddSplashmap (i));
		}
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
        public int[] treeTypes;
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