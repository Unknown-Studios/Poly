using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkIdentity))]
public class LODController : NetworkBehaviour {

	public int[] lodLevels;

	void Start() {
		if (isLocalPlayer) {
			StartCoroutine (StartUpdater ());
		}
	}

	public IEnumerator StartUpdater() {
		while (true) {
			UpdateMesh ();
			yield return new WaitForSeconds (0.1f);
		}
	}

	void UpdateMesh() {
		int layerMask = 1 << LayerMask.NameToLayer ("LOD");
		layerMask = ~layerMask;
		if (lodLevels != null && lodLevels.Length > 0) {
			Collider[] colliders = Physics.OverlapSphere (transform.position, lodLevels [lodLevels.Length - 1], layerMask);

			for (int i = 0; i < colliders.Length; i++) {
				colliders [i].GetComponent<LOD> ().UpdateLODSettings (transform.position, lodLevels);
			}
		}
	}
}
