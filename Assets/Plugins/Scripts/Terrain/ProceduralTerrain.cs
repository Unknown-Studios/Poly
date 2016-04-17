using CoherentNoise.Generation.Fractal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class ProceduralTerrain : NetworkBehaviour
{
    #region Fields

    public static ProceduralTerrain instance;

    public Biome[] _Biomes;

    public GameObject[] caves;

    [HideInInspector]
    public int Caves;

    [HideInInspector]
    public Terrain Chunk;

    [HideInInspector]
    public string currentAction = "Connecting to server";

    public Texture2D[] detail;

    [HideInInspector]
    public int GeneratedTiles = 0;

    public Color Grass;
    public float groundFrq;
    public int MaxCaves = 100;
    public float mountainFrq;
    public int Octaves;

    [HideInInspector]
    public float progress = 0.0f;

    [HideInInspector]
    public bool Respawn;

    public float riverFrq;

    [SyncVar]
    public int Seed;

    public bool SpawnAll;

    [HideInInspector]
    public Vector3 SpawnPos;

    public Texture2D[] splat;
    public GameObject[] stones;
    public int terrainHeight;
    public int terrainSize;
    public int terrainWidth;

    [HideInInspector]
    public int Trees;

    public GameObject Water;
    public int WaterLevel;
    private int Ah = 0;
    private BiomeMap Biomes;
    private int CurrentX = -1;
    private int CurrentY = -1;

    private DetailPrototype[] detailProtoTypes;

    private float[,] heightmap;

    private float[,,] map;

    private bool Spawned;

    private SplatPrototype[] splatPrototypes;

    private bool starto = false;

    private TreePrototype[] treePrototypes;

    #endregion Fields

    #region Properties

    public static int GT
    {
        get
        {
            if (instance == null)
            {
                return 1;
            }
            return instance.GeneratedTiles;
        }
    }

    #endregion Properties

    #region Methods

    public bool _LoadOnPlay = false;

    public GameObject[] trees;

    /*public static void OnBeforeSpawn(Vector3 sp)
    {
        instance.StartCoroutine(instance.BeforeSpawn(sp));
    }*/

    public void CreateProtoTypes()
    {
        splatPrototypes = new SplatPrototype[splat.Length];

        for (int i = 0; i < splat.Length; i++)
        {
            splatPrototypes[i] = new SplatPrototype();
            splatPrototypes[i].texture = splat[i];
            splatPrototypes[i].tileSize = new Vector2(15, 15);
        }

        detailProtoTypes = new DetailPrototype[detail.Length];

        for (int d = 0; d < detail.Length; d++)
        {
            detailProtoTypes[d] = new DetailPrototype();
            detailProtoTypes[d].renderMode = DetailRenderMode.Grass;
            detailProtoTypes[d].prototypeTexture = detail[d];
            detailProtoTypes[d].minHeight = 1f;
            detailProtoTypes[d].maxHeight = 2f;
            detailProtoTypes[d].healthyColor = Grass;
            detailProtoTypes[d].dryColor = Grass;
        }
    }

    public void OnKilled()
    {
        GeneratedTiles = 0;
        progress = 0.0f;
        starto = false;
    }

    public void ServerSpawn()
    {
        OnSeedReceived();
        Debug.Log("Creating Terrain..");
    }

    public IEnumerator UpdateTile()
    {
        if (isServer)
        {
            Seed = PlayerPrefs.GetInt("Seed");
        }
        Random.seed = Seed;
        if (GameObject.Find("Chunk"))
        {
            Chunk = GameObject.Find("Chunk").GetComponent<Terrain>();
        }
        else
        {
            progress = 0.0f;

            CreateProtoTypes();

            TerrainData td = new TerrainData();
            td.heightmapResolution = Ah;
            td.SetDetailResolution(td.heightmapResolution, 8);
            td.alphamapResolution = terrainSize;
            td.size = new Vector3(terrainWidth, terrainHeight, terrainWidth);

            GameObject tmp = Terrain.CreateTerrainGameObject(td);
            tmp.gameObject.layer = 11;

            Chunk = tmp.GetComponent<Terrain>();
            Chunk.tag = "Chunk";
            Chunk.name = "Chunk";

            Biomes = new BiomeMap();
            Biomes.Generate(_Biomes, Ah);

            Chunk.terrainData.splatPrototypes = splatPrototypes;
            Chunk.terrainData.detailPrototypes = detailProtoTypes;
            Chunk.terrainData.RefreshPrototypes();

            Chunk.castShadows = true;
            int hw = Chunk.terrainData.heightmapWidth;
            int aw = Chunk.terrainData.alphamapWidth;

            PinkNoise perlin = new PinkNoise(Seed);
            var mountain = new RidgeNoise(Seed);
            mountain.OctaveCount = Octaves * 2;
            mountain.Frequency = 0.0005f;

            heightmap = new float[hw, hw];

            int threshold = 150;
            currentAction = "Creating height-map..";

            map = new float[aw, aw, splatPrototypes.Length];
            for (int x = 0; x < aw; x++)
            {
                int worldPosX = Mathf.RoundToInt((x * 1f / hw * 1f) * (terrainWidth * 1f));
                for (int z = 0; z < aw; z++)
                {
                    int worldPosZ = Mathf.RoundToInt((z * 1f / hw * 1f) * (terrainWidth * 1f));

                    float plain = perlin.GetValue(worldPosX - 245, worldPosZ + 567, 0) + 0.5f / 2.0f;
                    float mountains = Mathf.Max(0f, mountain.GetValue(worldPosX + 17235, worldPosZ - 54358, 0f) - 0.75f) / 2.0f;

                    float h = mountains + plain;
                    if (x < threshold)
                    {
                        h *= (x * 1f) / threshold;
                    }
                    if (x > hw - threshold)
                    {
                        h *= (hw - (x * 1f)) / threshold;
                    }
                    if (z < threshold)
                    {
                        h *= (z * 1f) / threshold;
                    }
                    if (z > hw - threshold)
                    {
                        h *= (hw - (z * 1f)) / threshold;
                    }

                    heightmap[z, x] = h;
                }
                if (x % 10 == 0)
                {
                    progress = (x * 1f) / (aw * 1f);
                    yield return null;
                }
            }
            progress = 1.0f;

            currentAction = "Creating Water..";

            GameObject water = Instantiate(Water);
            water.name = "Water";
            float scale = td.size.x / Mathf.Sqrt(water.GetComponent<MeshFilter>().mesh.vertices.Length);
            water.transform.position = new Vector3(0, WaterLevel, 0);
            water.transform.localScale = new Vector3(scale, 0, scale);

            currentAction = "Applying height-map..";
            Chunk.terrainData.SetHeights(0, 0, heightmap);
            progress = 1.0f;

            while (progress != 1.0f)
            {
                yield return null;
            }

            if (!Game.noGraphics)
            {
                currentAction = "Painting the terrain..";
                progress = 0.0f;
                StartCoroutine(PaintTheTerrain());
                while (progress != 1.0f)
                {
                    yield return null;
                }
            }

            if (Game.noGraphics)
            {
                Debug.Log("Spawning Trees..");
            }
            currentAction = "Creating Vegetation..";
            progress = 0.0f;
            StartCoroutine(SpawnTheGrass());
            while (progress != 1.0f)
            {
                yield return null;
            }

            progress = 0.0f;
            currentAction = "Planting Trees..";
            StartCoroutine(SpawnTrees(SpawnPos));
            while (progress != 1.0f)
            {
                yield return null;
            }

            currentAction = "Scanning terrain..";
            //GridManager.ScanGrid();
        }
        yield return null;

        if (Chunk == null)
        {
            currentAction = "An error occurred (t3667)!";
            Game.Log("(t3667) The terrain was unexpectedly deleted.");
            yield return new WaitForSeconds(2.5f);
            Network.Disconnect();
            yield break;
        }
        currentAction = "Spawning player..";
        GeneratedTiles++;
        if (Game.noGraphics)
        {
            Debug.Log("Terrain Generation Complete");
        }
    }

    private void Awake()
    {
        int CurDiff = Mathf.RoundToInt((8 * Mathf.Pow(2, 8)));
        for (int i = 1; i < 9; i++)
        {
            int tmp = Mathf.RoundToInt((8 * Mathf.Pow(2, i)));
            if (Mathf.Abs(terrainSize - tmp) < CurDiff)
            {
                CurDiff = Mathf.Min(CurDiff, Mathf.Abs(terrainSize - tmp));
                Ah = tmp;
            }
        }

        map = new float[Ah, Ah, splat.Length];
    }

    private IEnumerator BeforeSpawn(Vector3 Sp)
    {
        if (Seed == 0)
        {
            if (Network.isServer)
            {
                Seed = PlayerPrefs.GetInt("Seed");
            }
            else if (Network.isClient)
            {
                while (Seed == 0)
                {
                    yield return null;
                }
                Game.Log("Received seed from server (" + Seed + ")");
            }
        }
        SpawnPos = Sp;
        OnSeedReceived();
    }

    private void OnEnable()
    {
        instance = this;
    }

    private void OnSeedReceived()
    {
        Random.seed = Seed;
        starto = true;
        StartCoroutine("UpdateTile");
    }

    private IEnumerator PaintTheTerrain()
    {
        int aw = Chunk.terrainData.alphamapWidth;
        for (int x = 0; x < aw; x++)
        {
            for (int y = 0; y < aw; y++)
            {
                float normalPosX = (x * 1f) / (aw * 1f);
                float normalPosY = (y * 1f) / (aw * 1f);

                float angle = Chunk.terrainData.GetSteepness(normalPosX, normalPosY);
                float height = Chunk.terrainData.GetInterpolatedHeight(normalPosX, normalPosY);
                Biome Biome = Biomes.GetPoint(normalPosX, normalPosY);

                if (height < WaterLevel + 25)
                {
                    map[y, x, 2] = 1;
                }
                else if (angle > 25)
                {
                    map[y, x, 1] = 1;
                }
                else
                {
                    map[y, x, Biome.SplatIndex] = 1;
                }
                progress = (x * 1f) / (aw * 1f);
                if (x % 25 == 0 && y % aw - 1 == 0)
                {
                    yield return null;
                }
            }
        }
        currentAction = "Waiting for the paint to dry";
        Chunk.terrainData.SetAlphamaps(0, 0, map);
        progress = 1.0f;
        yield return null;
    }

    private IEnumerator SpawnTheGrass()
    {
        int detailWidth = Chunk.terrainData.detailWidth;

        GameObject CaveMaster = new GameObject();
        CaveMaster.name = "CaveMaster";
        CaveMaster.transform.parent = Chunk.transform;

        GrassClass[] details = new GrassClass[detail.Length];
        for (int i = 0; i < detail.Length; i++)
        {
            details[i] = new GrassClass();
            details[i].details = new int[detailWidth, detailWidth];
        }

        for (int x = 0; x < detailWidth; x++)
        {
            float normalPosX = (x * 1f) / (detailWidth * 1f);
            for (int y = 0; y < detailWidth; y++)
            {
                float normalPosY = (y * 1f) / (detailWidth * 1f);

                float angle = Chunk.terrainData.GetSteepness(normalPosX, normalPosY);
                float height = Chunk.terrainData.GetInterpolatedHeight(normalPosX, normalPosY);
                Biome Biome = Biomes.GetPoint(normalPosX, normalPosY);

                if (height > WaterLevel + 2 && angle < 25)
                {
                    if (Biome.GrassIndex != -1)
                    {
                        details[Biome.GrassIndex].details[y, x] = Random.Range(32, 64);
                    }
                }

                if (MaxCaves <= 0)
                {
                    MaxCaves = 1;
                }

                int SpawnCaveChance = Random.Range(0, (terrainWidth * terrainWidth) / (MaxCaves));
                bool SpawnCave = false;

                if (Biome.name == "Plain" || Biome.name == "Forest")
                {
                    SpawnCave = true;
                }
                if (SpawnCave && SpawnCaveChance < 10)
                {
                    if (Caves < 255 && height > WaterLevel + 5)
                    {
                        if (caves != null)
                        {
                            RaycastHit hito = new RaycastHit();
                            if (Physics.Raycast(new Vector3(normalPosX * terrainWidth, terrainHeight, normalPosY * terrainWidth), Vector3.down, out hito))
                            {
                                GameObject cave = Instantiate(caves[Random.Range(0, caves.Length)], new Vector3(normalPosX * terrainWidth, height - 1f, normalPosY * terrainWidth), Quaternion.identity) as GameObject;
                                cave.layer = 12;
                                cave.name = "Cave";
                                cave.transform.rotation = Quaternion.FromToRotation(Vector3.up, hito.normal);
                                cave.transform.localScale = new Vector3(3, 3, 3);
                                cave.transform.parent = CaveMaster.transform;
                                Caves++;
                            }
                        }
                    }
                }
            }
            if (x % 10 == 0)
            {
                progress = (x * 1f) / (detailWidth * 1f);
                yield return null;
            }
        }
        for (int i = 0; i < details.Length; i++)
        {
            Chunk.terrainData.SetDetailLayer(0, 0, i, details[i].details);
        }
        progress = 1.0f;
        yield return null;
    }

    private IEnumerator SpawnTrees(Vector3 pos)
    {
        if (Spawned)
        {
            progress = 1.0f;
            yield break;
        }
        Spawned = true;
        if (SpawnAll)
        {
            GameObject TreeMaster = new GameObject();
            TreeMaster.name = "Tree Master";
            for (int x = 0; x < terrainWidth; x += 2)
            {
                float normalPosX = (x * 1f) / (terrainWidth * 1f);
                for (int y = 0; y < terrainWidth; y += 2)
                {
                    float normalPosY = (y * 1f) / (terrainWidth * 1f);

                    float angle = Chunk.terrainData.GetSteepness(normalPosX, normalPosY);
                    float height = Chunk.terrainData.GetInterpolatedHeight(normalPosX, normalPosY);
                    int random = Random.Range(0, 100000);
                    Biome Biome = Biomes.GetPoint(normalPosX, normalPosY);

                    if (random < Biome.TreeChance)
                    {
                        if (WaterLevel + 5 < height && angle < 25)
                        {
                            int TreeIndex = Random.Range(0, trees.Length);
                            GameObject tree = GameObject.Instantiate(trees[TreeIndex]) as GameObject;
                            tree.transform.position = new Vector3(x, height, y);
                            tree.transform.parent = TreeMaster.transform;
                            tree.name = trees[TreeIndex].name;
                            tree.tag = "Tree";
                        }
                        Trees++;
                    }
                    progress = normalPosX - 0.01f;
                }
                if (x % 25 == 0)
                {
                    yield return null;
                }
            }
            progress = 1.0f;
        }
        else
        {
            int TileX = Mathf.RoundToInt(pos.x / 250.0f);
            int TileY = Mathf.RoundToInt(pos.z / 250.0f);
            if ((TileX == CurrentX && TileY == CurrentY))
            {
                yield break;
            }
            CurrentX = TileX;
            CurrentY = TileY;

            for (int x = CurrentX - 1; x < CurrentX + 2; x++)
            {
                for (int y = CurrentY - 1; y < CurrentY + 2; y++)
                {
                    if (!GameObject.Find("TreeMaster " + x + "," + y))
                    {
                        GameObject TreeMaster = new GameObject();
                        TreeMaster.name = "TreeMaster " + x + "," + y;
                        TreeMaster.transform.parent = Chunk.transform;

                        //Loop through each tree tile.
                        for (int TreeX = x * 250; TreeX < x * 250 + 251; TreeX++)
                        {
                            for (int TreeY = y * 250; TreeY < y * 250 + 251; TreeY++)
                            {
                                float normalPosX = TreeX / (terrainWidth * 1f);
                                float normalPosY = TreeY / (terrainWidth * 1f);

                                float angle = Chunk.terrainData.GetSteepness(normalPosX, normalPosY);
                                float height = Chunk.terrainData.GetInterpolatedHeight(normalPosX, normalPosY);
                                int random = Random.Range(0, 1000000);
                                Biome Biome = Biomes.GetPoint(normalPosX, normalPosY);

                                if (random < Biome.TreeChance)
                                {
                                    if (WaterLevel + 5 < height && angle < 25)
                                    {
                                        GameObject tree = GameObject.Instantiate(trees[Random.Range(0, trees.Length)]) as GameObject;
                                        tree.transform.parent = TreeMaster.transform;
                                        tree.transform.position = new Vector3(TreeX, height, TreeY);
                                    }
                                    Trees++;
                                }
                            }
                            if (TreeX % 25 == 0)
                            {
                                yield return null;
                            }
                        }
                        progress += 1.0f / 9;
                    }
                }
            }
            progress = 1.0f;
        }
        Game.player.GetComponent<Inventory>().ShowEverything = true;
    }

    private void Start()
    {
        if (_LoadOnPlay)
        {
            StartCoroutine(UpdateTile());
        }
    }

    private void Update()
    {
        if (GeneratedTiles == 1)
        {
            if (Game.player != null || Game.noGraphics)
            {
                if (Chunk == null)
                {
                    Debug.Log("Terrain wasn't set correctly, trying to search for it!");
                    Chunk = Terrain.activeTerrain;
                }
            }
            if (Game.player != null)
            {
                /*if (trees[0].billboard == null) {
                    for (int t = 0; t < trees.Length; t++) {
                        StartCoroutine(trees[t].runThis());
                    }
                }*/
                StartCoroutine(SpawnTrees(Game.player.transform.position));
            }
        }

        if (!starto)
        {
            foreach (GameObject treeMaster in GameObject.FindGameObjectsWithTag("TreeMaster"))
            {
                Destroy(treeMaster);
            }
        }
    }

    #endregion Methods

    #region Classes

    [Serializable]
    public class Biome
    {
        #region Fields

        public Color BiomeColor;
        public int GrassIndex;
        public string name = "Biome name";
        public int SplatIndex;
        public int TreeChance;

        #endregion Fields
    }

    public class BiomeMap
    {
        #region Fields

        public int c = 0;
        public Texture2D map;
        private Biome[] b;
        private bool error;
        private LocalBiome[] LB;

        private List<Vector2> points = new List<Vector2>();

        #endregion Fields

        #region Methods

        public void Generate(Biome[] biomes, int width)
        {
            if (biomes == null || biomes.Length == 0)
            {
                Debug.LogError("Biome Array wasn't set!");
                return;
            }
            b = biomes;
            map = new Texture2D(width, width);
            int PointCount = 100;
            LB = new LocalBiome[PointCount];

            for (int i = 0; i < PointCount; i++)
            {
                points.Add(new Vector2(
                    Random.Range(0, map.width),
                    Random.Range(0, map.height))
                );
            }
            int num = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (Random.Range(0, 3) == 0)
                {
                    num = Random.Range(2, 4);
                }
                else
                {
                    num = Random.Range(0, 2);
                }
                Biome Bio = biomes[num];
                LB[i] = new LocalBiome();
                LB[i].point = i;
                LB[i].biome = Bio;
                map.SetPixel(Mathf.RoundToInt(points[i].x), Mathf.RoundToInt(points[i].y), Bio.BiomeColor);
            }

            for (int x = 0; x < map.width; x++)
            {
                for (int y = 0; y < map.width; y++)
                {
                    Vector2 point = GetNearestPoint(x, y);
                    map.SetPixel(x, y, map.GetPixel(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y)));
                }
            }
            map.Apply();
        }

        public Biome GetPoint(float x, float y)
        {
            Color col = map.GetPixel(Mathf.RoundToInt(x * map.width), Mathf.RoundToInt(y * map.height));
            Biome Bio = new Biome();
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i].BiomeColor == col)
                {
                    Bio = b[i];
                }
            }
            return Bio;
        }

        private Vector2 GetNearestPoint(int x, int y)
        {
            Vector2 point = new Vector2(x, y);
            Vector2 nearest = new Vector2(0, 0);

            for (int i = 0; i < points.Count; i++)
            {
                if (Vector2.Distance(points[i], point) < Vector2.Distance(nearest, point))
                {
                    nearest = points[i];
                }
            }
            return nearest;
        }

        #endregion Methods

        #region Classes

        public class LocalBiome
        {
            #region Fields

            public Biome biome;
            public int point;

            #endregion Fields
        }

        #endregion Classes
    }

    public class GrassClass
    {
        #region Fields

        public int[,] details;

        #endregion Fields
    }

    #endregion Classes
}