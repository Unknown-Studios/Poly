using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrassController : MonoBehaviour {


	void Start() {
		if (grassMesh == null) {
			Debug.LogError ("GrassMesh not set for grasscontroller");
			enabled = false;
			return;
		}
		if (grassMaterial == null) {
			Debug.LogError ("GrassMaterial not set for grasscontroller");
			enabled = false;
			return;
		}
		cam = GetComponent<Camera> ();
		if (cam == null) {
			cam = gameObject.GetComponentInChildren<Camera> ();
		}
		if (cam == null) {
			cam = Camera.main;
		}
		if (cam == null) {
			cam = FindObjectOfType<Camera> ();
		}
		if (cam == null) {
			Debug.LogError ("No camera found in scene");
		}
	}

	private Camera cam;
	public Mesh grassMesh;
	public Material grassMaterial;
	public int grassRadius = 50;

	private Bounds bounds = new Bounds(Vector3.zero,Vector3.one);
	void Update () {
		Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
		Vector3 curPos = transform.position;
		for (int x = -grassRadius; x < grassRadius; x++) {
			for (int y = -grassRadius; y < grassRadius;y++) {
				Vector3 pos = new Vector3 (x * 1.5f, 0.5f, y * 1.5f);
				bounds.center = pos;
				if (GeometryUtility.TestPlanesAABB(frustumPlanes, bounds)) {
					Graphics.DrawMesh (grassMesh, pos, Quaternion.identity, grassMaterial, 0);
				}
			}
		}
	}
}
