using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
	GameObject gameobject;
	public ProceduralTree.TreeClass tree;
	public Material material;

    // Use this for initialization
    private void Start()
    {
		gameobject = new GameObject ();
		gameobject.name = "Tree";
		gameobject.AddComponent<MeshFilter> ();
		MeshRenderer mr = gameobject.AddComponent<MeshRenderer> ();
		mr.sharedMaterial = material;
		InvokeRepeating ("Showcase", 0.0f, 2.5f);
    }

	public void Showcase() {
		MeshFilter mf = gameobject.GetComponent<MeshFilter> ();
		MeshRenderer mr = gameobject.GetComponent<MeshRenderer> ();
		ProceduralTree.TreeMesh tm = ProceduralTree.GenerateTree (tree);
		mf.mesh = tm.mesh;
		mr.sharedMaterial.mainTexture = tm.uvTex;

	}
}