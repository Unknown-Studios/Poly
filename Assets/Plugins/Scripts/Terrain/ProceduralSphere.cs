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

    private int maxCheck = 0;

    private CullingGroup group;

    private GameObject[] TreeObjects;

    private List<TreeObject> UnusedTrees;

    private TreePosition[] cullingSpheres;

    public static Vector3 GetSpherePoint(Vector3 vertexPos, int Width)
    {
        Vector3 v = vertexPos * 2f / Width - Vector3.one;
        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;
        Vector3 s = Vector3.zero;
        s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
        return s;
    }

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

            treeMesh = new ProceduralTree.TreeMesh[trees.Length, 5];

            for (int tree = 0; tree < trees.Length; tree++)
            {
                for (int i = 0; i < treeMesh.GetLength(1); i++)
                {
                    treeMesh[tree, i] = ProceduralTree.GenerateTree(trees[tree]);
                }
            }

            group = new CullingGroup();
            group.targetCamera = Camera.main;
            group.onStateChanged = OnTreeStateChanged;

            TreeObjects = new GameObject[250 * trees.Length];
            UnusedTrees = new List<TreeObject>();

            for (int t = 0; t < 250 * trees.Length; t++)
            {
                TreeObject tj = new TreeObject();
                tj.type = Random.Range(0, treeMesh.GetLength(0));
                tj.gameObject = new GameObject();

                MeshFilter fil = tj.gameObject.AddComponent<MeshFilter>();
                MeshRenderer ren = tj.gameObject.AddComponent<MeshRenderer>();
                CapsuleCollider col = tj.gameObject.AddComponent<CapsuleCollider>();

                ProceduralTree.TreeMesh tm = treeMesh[tj.type, Random.Range(0, 5)];
                fil.mesh = tm.mesh;
                ren.material.mainTexture = tm.uvTex;
                col.height = tm.height;
                col.radius = tm.tRadius;

                UnusedTrees.Add(tj);
            }
            cullingSpheres = new TreePosition[1000];
            BoundingSphere[] spheres = new BoundingSphere[1000];
            for (int tree = 0; tree < 1000; tree++)
            {
                cullingSpheres[tree] = SpawnTree();
                spheres[tree] = cullingSpheres[tree].sphere;
            }

            group.SetBoundingSpheres(spheres);
            group.SetBoundingSphereCount(1000);

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

    private TreePosition SpawnTree()
    {
        Vector3 position = Random.onUnitSphere * Radius;
        RaycastHit rayhit;
        if (Physics.Linecast(position, Vector3.zero, out rayhit))
        {
            if (rayhit.transform.GetComponent<Renderer>() != null)
            {
                Vector3 SpawnPos = rayhit.point;

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
                            return null;
                        }
                        TreeType = curBiome.treeTypes[Random.Range(0, curBiome.treeTypes.Length)];
                        break;
                    }
                }

                ProceduralTree.TreeMesh tm = treeMesh[TreeType, Random.Range(0, treeMesh.GetLength(1))];
                TreePosition treePos = new TreePosition(SpawnPos, tm);
                return treePos;
            }
        }
        return null;
    }

    //Called on script load.
    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Game")
        {
            OnBeforeSpawn(Vector3.zero);
        }
    }

    private void OnDisabled()
    {
        group.Dispose();
        group = null;
    }

    private void OnTreeStateChanged(CullingGroupEvent evt)
    {
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

			Texture2D tex = new Texture2D(Width, Width);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;

			for (int x1 = 0; x1 < tex.width; x1++) {
				for (int y1 = 0; y1 < tex.height; y1++) {
					tex.SetPixel (x1, y1, Color.white);
				}
			}
			tex.Apply ();

            Material SideMaterial = new Material(TerrainMaterial);
            SideMaterial.mainTexture = tex;
            SideMaterial.name = s0.name;
            yield return null;
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
                current.GetComponent<LOD>().SetTargetLOD(5);
                Done.Add(current);
            }
            colCount++;
            if (queue.Count == 0 && maxCheck <= 25)
            {
                yield return new WaitForSeconds(1.0f);
                maxCheck++;
                StartCoroutine(AddColliders());
            }
        }
    }

    private Color GetRegionColor(Vector3 s)
    {
        RaycastHit Rayhit;
        Color col = Color.black;
		Vector3 start = s * (Radius + MaxHeight);
        if (Physics.Linecast(start, Vector3.zero, out Rayhit))
        {
            for (int r = Regions.Length - 1; r >= 0; r--)
            {
                if (Mathf.Clamp01((Vector3.Distance(Rayhit.point, Vector3.zero) - Radius) / MaxHeight) <= Regions[r].height)
                {
					if (Vector3.Angle(Rayhit.normal, start) > 25.0f) {
						col = Color.gray;
						Debug.DrawLine (start, Rayhit.point,Color.red,10.0f);
					} else 
					if (Regions[r].Biome)
                    {
                        VoronoiPoint closest = points[0];
                        for (int b = 1; b < points.Length; b++)
                        {
                            if (Vector3.Distance(points[b].point, s) < Vector3.Distance(points[b].point, closest.point))
                            {
                                closest = points[b];
                            }
                        }
                        col = closest.biome.biomeColor;
                    }
                    else
                    {
                        col = Regions[r].color;
                    }
                }
            }
        }
        return col;
    }

	bool SplashDone = false;
	void FixedUpdate() {
		if (isDone && !SplashDone) {
			SplashDone = true;
			for (int i = 0; i < 6; i++) {
				Texture2D texture = null;

				GameObject gm = GameObject.Find ("Side #" + i);
				texture = (Texture2D)gm.GetComponentInChildren<MeshRenderer> ().material.mainTexture;

				switch (i) {
				case 0:
					for (int y = 0; y < Width; y++) {
						for (int x = 0; x < Width; x++) {
							Vector3 vertexPos = new Vector3 (x + 0.5f, y + 0.5f, 0);
							Vector3 startPos = GetSpherePoint (vertexPos, Width);
							if (texture.GetPixel (x, y) == Color.white)
								texture.SetPixel (x, y, GetRegionColor (startPos));
						}
					}
					break;

				case 1:
					for (int y = 0; y < Width; y++) {
						for (int z = 0; z < Width; z++) {
							Vector3 vertexPos = new Vector3 (Width, y + 0.5f, z + 0.5f);
							Vector3 startPos = GetSpherePoint (vertexPos, Width);
							if (texture.GetPixel (y, z) == Color.white)
								texture.SetPixel (y, z, GetRegionColor (startPos));
						}
					}
					break;

				case 2:
					for (int y = 0; y < Width; y++) {
						for (int x = Width; x >= 0; x--) {
							Vector3 vertexPos = new Vector3 (x + 0.5f, y + 0.5f, Width);
							Vector3 startPos = GetSpherePoint (vertexPos, Width);
							if (texture.GetPixel (x, y) == Color.white)
								texture.SetPixel (x, y, GetRegionColor (startPos));
						}
					}
					break;

				case 3:
					for (int y = 0; y < Width; y++) {
						for (int z = 0; z < Width; z++) {
							Vector3 vertexPos = new Vector3 (0, y + 0.5f, z + 0.5f);
							vertexPos += Vector3.one;
							Vector3 startPos = GetSpherePoint (vertexPos, Width);
							if (texture.GetPixel (y, z) == Color.white)
								texture.SetPixel (y, z, GetRegionColor (startPos));
						}
					}
					break;

				case 4:
					for (int z = 0; z < Width; z++) {
						for (int x = 0; x < Width; x++) {
							Vector3 vertexPos = new Vector3 (x + 0.5f, Width, z + 0.5f);
							Vector3 startPos = GetSpherePoint (vertexPos, Width);
							if (texture.GetPixel (x, z) == Color.white)
								texture.SetPixel (x, z, GetRegionColor (startPos));
						}
					}
					break;

				case 5:
					for (int x = 0; x < Width; x++) {
						for (int z = 0; z < Width; z++) {
							Vector3 vertexPos = new Vector3 (x + 0.5f, 0, z + 0.5f);
							Vector3 startPos = GetSpherePoint (vertexPos, Width);
							if (texture.GetPixel (x, z) == Color.white)
								texture.SetPixel (x, z, GetRegionColor (startPos));
						}
					}
					break;

				default:
					break;
				}
				texture.Apply ();
			}
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

    public class TreeObject
    {
        public GameObject gameObject;
        public int type;
    }

    [Serializable]
    public class TreePosition
    {
        public ProceduralTree.TreeMesh mesh;
        public BoundingSphere sphere;

        public TreePosition(Vector3 Position, ProceduralTree.TreeMesh treeMesh)
        {
            mesh = treeMesh;
            sphere = new BoundingSphere(Position, treeMesh.height);
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