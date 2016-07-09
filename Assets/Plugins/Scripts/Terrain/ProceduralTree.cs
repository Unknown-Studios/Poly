using UnityEngine;
using System.Collections;

public class ProceduralTree : MonoBehaviour
{
    public int ringVertices = 8;
    public int trunkHeight = 10;
	public int leavesHeight = 5;
	public float LeavesRadius = 2.5f;
    public float Radius = 2.0f;
	public Material mat;

    public GameObject tmPrefab;

    private Vector3[] drawArray;

    // Use this for initialization
    private void Start()
    {
		InvokeRepeating ("Showcase", 0.0f, 2.0f);
		if (drawArray != null) {
			int op = 0;
			foreach (Vector3 pos in drawArray) {
				GameObject gm = GameObject.Instantiate (tmPrefab, pos, Quaternion.identity) as GameObject;
				TextMesh tm = gm.GetComponent<TextMesh> ();
				if (tm != null) {
					tm.name = "Mesh: " + op.ToString ();
					tm.text = op.ToString ();
					tm.transform.parent = transform;
				}
				op++;
			}
		}
    }

	private GameObject current;

	public void Showcase() {
		if (current != null) {
			Destroy (current);
		}
		current = GenerateTree (Vector3.zero);
	}

	private GameObject GenerateTree(Vector3 position)
    {
        //Initialization
        GameObject tree = new GameObject();
		tree.transform.position = position;
        MeshFilter mft = tree.AddComponent<MeshFilter>();
        MeshRenderer mr = tree.AddComponent<MeshRenderer>();
        MeshCollider collider = tree.AddComponent<MeshCollider>();
        Mesh treeMesh = mft.mesh = new Mesh();
		mr.sharedMaterial = mat;

		trunkHeight = Random.Range (5, 11);
		leavesHeight = Random.Range (4, 7);

		while (leavesHeight % 2 != 0) {
			leavesHeight = Random.Range (4, 7);
		}

		float xDegree = Random.Range (-0.1f, 0.1f);
		float zDegree = Random.Range (-0.1f, 0.1f);

		Vector3[] vertices = new Vector3[ringVertices * (trunkHeight+(leavesHeight* 2))];
        int[] triangles = new int[vertices.Length * 6];

        //Vertices
		for (int h = 0; h < trunkHeight + (leavesHeight*2); h++)
        {
			if (h < trunkHeight) {
				for (int v = 0; v < ringVertices; v++) {
					int index = (h * ringVertices) + v;
					float rad = (float)v / ringVertices * (2 * Mathf.PI);
					//Trunk
					vertices [index] = new Vector3 (Mathf.Cos (rad) * Radius, h, Mathf.Sin (rad) * Radius);
					vertices [index].x += Mathf.Pow(1.0f+xDegree,h) - 1.0f;
					vertices [index].z += Mathf.Pow(1.0f+zDegree,h)-1.0f;
				}
			} else {
				for (int v = 0; v < ringVertices; v++) {
					int index = (h * ringVertices) + v;
					float rad = (float)v / ringVertices * (2 * Mathf.PI);
					if (h % 2 == 0) {
						vertices [index] = new Vector3 (Mathf.Cos (rad) * LeavesRadius, trunkHeight +(h-trunkHeight)/1.5f, Mathf.Sin (rad) * LeavesRadius);
					} else {
						vertices [index] = new Vector3 (Mathf.Cos (rad) * Radius, trunkHeight + ((h-trunkHeight)/1.5f) + 1f, Mathf.Sin (rad) * Radius);
					}

					//Randomness
					if (h == trunkHeight + (2*leavesHeight) - 1) {
						vertices [index].x = 0;
						vertices [index].y += 2.0f;
						vertices [index].z = 0;
					} else if (v % 2 == 0) {
						vertices [index].y += Random.Range (-0.5f, 0.6f);
					}
					vertices [index].x += Mathf.Pow(1.0f+xDegree,h)-1.0f;
					vertices [index].z += Mathf.Pow(1.0f+zDegree,h)-1.0f;
				}
			}
        }

        //Triangles
        int i = 0;
		for (int h = 0; h < trunkHeight + (leavesHeight* 2)- 1; h++) {
			for (int c = 0; c < ringVertices - 1; c++)
	        {
				int index = (h*ringVertices)+c; 
				triangles[i++] = index + ringVertices;
				triangles[i++] = index + 1;
				triangles[i++] = index;

				triangles[i++] = index + ringVertices;	
				triangles[i++] = index + ringVertices + 1;
				triangles[i++] = index + 1; 
	        }
			int ind = (ringVertices * (1+h)) - 1;
			triangles[i++] = ind + ringVertices;
			triangles[i++] = ind + 1 - ringVertices;
			triangles[i++] = ind;

			triangles[i++] = ind + ringVertices;	
			triangles[i++] = ind + 1;
			triangles[i++] = ind + 1 - ringVertices;
		}

        //Finish up and return
        treeMesh.vertices = vertices;
        treeMesh.triangles = triangles;

        treeMesh.RecalculateNormals();
        treeMesh.RecalculateBounds();
        treeMesh.Optimize();
        mft.mesh = treeMesh;
        if (collider.sharedMesh != null)
        {
            collider.sharedMesh.Clear();
        }
        collider.sharedMesh = treeMesh;

        return tree;
    }

    private void OnDrawGizmos()
    {
        if (drawArray != null)
        {
            for (int i = 0; i < drawArray.Length; i++)
            {
                Gizmos.DrawSphere(drawArray[i], 0.1f);
            }
        }
    }
}