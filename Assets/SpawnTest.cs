using UnityEngine;
using System.Collections;

public class SpawnTest : MonoBehaviour {

	ProceduralSphere PS;

	// Use this for initialization
	void Start () {
		PS = FindObjectOfType<ProceduralSphere> ();
	}

	bool done;
	
	// Update is called once per frame
	void Update () {
		if (PS.isDone && !done) {
			done = true;
			transform.position = PS.GetHeight (Random.onUnitSphere);
		}
	}
}
