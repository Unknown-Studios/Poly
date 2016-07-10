using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProceduralTree : MonoBehaviour
{
	public int Type = -1;
	public Material mat;

    // Use this for initialization
    private void Start()
    {
		tree = new GameObject();
		tree.transform.position = Vector3.zero;
		tree.AddComponent<MeshFilter>();
		MeshRenderer mr = tree.AddComponent<MeshRenderer>();
		tree.AddComponent<MeshCollider>();
		mr.sharedMaterial = mat;
		InvokeRepeating ("Showcase", 0.0f, 2.5f);
    }

	private GameObject tree;

	public void Showcase() {
		Type = Random.Range (0, TreeTypes.Length);
		tree.GetComponent<MeshFilter> ().sharedMesh = ProceduralTree.GenerateTree (TreeTypes[Type]);
		MeshCollider col = tree.GetComponent<MeshCollider> ();
		if (col.sharedMesh != null) {
			col.sharedMesh.Clear ();
		}
		col.sharedMesh = tree.GetComponent<MeshFilter> ().sharedMesh;
	}
		
	public static Mesh GenerateTree(TreeClass treeInfo)
    {
        //Initialization
        Mesh treeMesh = new Mesh();

		int trunkHeight = Random.Range (treeInfo.minTrunk, treeInfo.maxTrunk);
		int leavesHeight = Random.Range (treeInfo.minLeaves, treeInfo.maxLeaves);
		int ringVertices = 8;
		float LeavesRadius = 2f + Random.Range(-0.1f,0.1f);
		float trunkRadius = 0.5f;

		while (leavesHeight % treeInfo.LeavesPattern.length != 0) {
			leavesHeight = Random.Range (treeInfo.minLeaves, treeInfo.maxLeaves);
		}

		float xDegree = Random.Range (-0.1f, 0.1f);
		float zDegree = Random.Range (-0.1f, 0.1f);

		List<Vector3> vertices = new List<Vector3>();

        //Vertices
		int leaf = 0;
		int ah = 0;
		int extra = 0;
		for (float h = 0.0f; ah < trunkHeight + leavesHeight + extra; ah++)
        {
			if (ah < trunkHeight) {
				for (int v = 0; v < ringVertices; v++) {
					float rad = (float)v / ringVertices * (2 * Mathf.PI);

					float xRan = Random.Range (0.0f, 0.1f);
					float zRan = Random.Range (0.0f, 0.1f);

					//Trunk
					vertices.Add(new Vector3 ((Mathf.Cos (rad) * trunkRadius) + (Mathf.Pow(1.0f+xDegree,h) - 1.0f) + xRan, h, (Mathf.Sin (rad) * trunkRadius) + (Mathf.Pow(1.0f+zDegree,h)-1.0f) + zRan));
				}
				h++;
			} else {
				float radiu = 0.0f;
				float addH = 0.0f;
				if (treeInfo.LeavesPattern.postWrapMode == WrapMode.Loop) {
					radiu = treeInfo.LeavesPattern.keys [leaf % treeInfo.LeavesPattern.length].value;
					leaf++;
					addH = treeInfo.LeavesPattern.keys [leaf % treeInfo.LeavesPattern.length].time * 1.5f;
				} else {
					radiu = treeInfo.LeavesPattern.Evaluate ((1.0f*leaf) / leavesHeight);
					addH = 1.0f;
					leaf++;
				}
				float FinalRadius = (trunkRadius+radiu) * (LeavesRadius);
				for (int v = 0; v < ringVertices; v++) {
					float rad = (float)v / ringVertices * (2 * Mathf.PI);
					vertices.Add(new Vector3 (Mathf.Cos (rad) * FinalRadius, h, Mathf.Sin (rad) * FinalRadius));

					if (ah == trunkHeight + leavesHeight + extra - 1) {
						Vector3 temp = vertices [vertices.Count-1];
						temp.x = 0;
						temp.z = 0;
						vertices [vertices.Count - 1] = temp;
					}
					Vector3 tmp = vertices [vertices.Count-1];
					tmp.x += Mathf.Pow (1.0f + xDegree, h) - 1.0f;
					tmp.y += Random.Range (-0.5f, 0.0f);
					tmp.z += Mathf.Pow (1.0f + zDegree, h) - 1.0f;
					vertices [vertices.Count - 1] = tmp;
				}
				h += addH * 2.0f;
			}
		}

		int[] triangles = new int[vertices.Count * 6];

        //Triangles
        int i = 0;
		for (int h = 0; h < trunkHeight + leavesHeight- 1; h++) {
			for (int c = 0; c < ringVertices - 1; c++)
	        {
				int inde = (h*ringVertices)+c; 
				triangles[i++] = inde + ringVertices;
				triangles[i++] = inde + 1;
				triangles[i++] = inde;

				triangles[i++] = inde + ringVertices;	
				triangles[i++] = inde + ringVertices + 1;
				triangles[i++] = inde + 1; 
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
		treeMesh.vertices = vertices.ToArray();
        treeMesh.triangles = triangles;

        treeMesh.RecalculateNormals();
        treeMesh.RecalculateBounds();
        treeMesh.Optimize();
        return treeMesh;
    }

	public TreeClass[] TreeTypes;

	[System.Serializable]
	public class TreeClass {
		public int minTrunk = 5;
		public int maxTrunk = 11;
		public int minLeaves = 4;
		public int maxLeaves = 7;
		public AnimationCurve LeavesPattern;
	}
}