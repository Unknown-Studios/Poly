﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ProceduralSphere : MonoBehaviour
{
    public int WidthTick;

    public int Width;

    public int Radius;
    public int MaxHeight;

	public int Seed;

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

    private Queue<GameObject> queue;

    // Use this for initialization
    public void OnBeforeSpawn(Vector3 SpawnPos)
    {
        StartCoroutine(GenerateTerrain());
    }

	//Called on script load.
    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Game")
            OnBeforeSpawn(Vector3.zero);
    }

	public void Update() {
		if (done == true) {
			done = false;

		}
	}
	void Awake() {
		PlayerPrefs.SetInt ("Seed", Seed);
		PlayerPrefs.Save ();
	}

	//Redirect to GetHeight(Vector3);
	public Vector3 GetHeight(float x, float y, float z) {
		return GetHeight (new Vector3 (x, y, z));
	}

	//Get the height of the terrain in any point
	public Vector3 GetHeight(Vector3 v3) {
		Vector3 startPos = new Vector3 ();
		RaycastHit hit;
		//If height found
		if (Physics.Raycast (startPos, Vector3.zero, out hit, (MaxHeight+Radius)*2f)) {
			//Add 0.5 to the height
			Vector3 normal = hit.point / (Radius + MaxHeight);
			Vector3 position = normal * (Radius + MaxHeight + 0.5f);

			return position;
		} else {
			return Vector3.zero * Mathf.Infinity;
		}
	}

	int numSides = 0;

	//Add a side of the cube/cubesphere
    private IEnumerator AddSide(int side)
    {
		if (numSides < 6) { 
			GameObject s0 = new GameObject ();
			sides [side] = s0;
			s0.name = "Side #" + side;
			s0.transform.parent = transform;

			Texture2D tex = new Texture2D (Width, Width);
			tex.wrapMode = TextureWrapMode.Clamp;
			tex.filterMode = FilterMode.Point;
			Material SideMaterial = new Material (TerrainMaterial);
			SideMaterial.mainTexture = tex;
			float localProgress = 0.0f;
			for (int x = 0; x < Width / 16; x++) {
				for (int y = 0; y < Width / 16; y++) {
					GameObject gm = new GameObject ();
					gm.transform.parent = s0.transform;
					gm.name = "Chunk (" + x + "," + y + ") side #" + side;
					gm.layer = LayerMask.NameToLayer ("LOD");

					LOD lod = gm.AddComponent<LOD> ();
					lod.side = side;
					lod.Chunk = new Vector2 (x, y);
					lod.Width = Width;
					lod.terrainMaterial = SideMaterial;
					lod.Regions = Regions;
					lod.curve = curve;
					lod.scale = scale;

					lod.Radius = Radius;
					lod.MaxHeight = MaxHeight;
					lod.Octaves = Octaves;
					queue.Enqueue (gm);
					if (y % 2 == 0) {
						yield return null;
					}
				}
				localProgress = (x + 1) / (Width / 16f);

				progress = Mathf.Clamp01 (localProgress / (6 - side));
			}
			while (progress != 1.0f) {
				yield return null;
			}
			numSides++;
		} else {
			progress = 1.0f;
		}
        if (progress == 1.0f)
        {
            done = true;
        }
    }

    private IEnumerator AddColliders()
    {
        while (queue.Count > 0)
        {
            GameObject current = queue.Dequeue();
            MeshCollider mc = current.GetComponent<MeshCollider>();
            if (mc.convex != true)
            {
                LOD lod = current.GetComponent<LOD>();
                lod.SetTargetLOD(0);
                yield return null;
                mc.convex = true;
                yield return null;
                lod.SetTargetLOD(4);
            }
        }
		Debug.Log (GetHeight(Random.onUnitSphere));
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