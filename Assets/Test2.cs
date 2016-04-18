using CoherentNoise.Generation.Fractal;
using UnityEngine;
using System.Collections;

public class Test2 : MonoBehaviour
{
    public int Octaves = 3;
    public float groundFrq = 0.01f;
	public float Persistence = 0.17f;

    public int mOctaves = 3;
    public float mFreq = 0.1f;

    private RidgeNoise noise;
    private PinkNoise pink;

    private void Start()
    {
        Random.seed = PlayerPrefs.GetInt("Seed");
        noise = new RidgeNoise(Random.seed);
        pink = new PinkNoise(Random.seed);
        pink.OctaveCount = Octaves;
        pink.Frequency = groundFrq;
		pink.Persistence = Persistence;
		pink.Lacunarity = 4.0f;

        noise.OctaveCount = mOctaves;
        noise.Frequency = mFreq;
		StartCoroutine (GenerateTexture ());
    }

	public AnimationCurve curve;
	public float scale;

    private IEnumerator GenerateTexture()
    {
		while (true) {
			pink.OctaveCount = Octaves;
			pink.Frequency = groundFrq;

			noise.OctaveCount = mOctaves;
			noise.Frequency = mFreq;

			Mesh mesh = GetComponent<MeshFilter> ().mesh;
			Vector3[] vert = mesh.vertices;
			for (int i = 0; i < vert.Length; i++) {
				float plain = (pink.GetValue (transform.position.x + vert [i].x, transform.position.z + vert [i].z, 0)+1.0f) / 3.0f;
				float mountains = Mathf.Max (0f, noise.GetValue (transform.position.x + vert [i].x - 549, transform.position.z + vert [i].z + 2585, 0f) - 0.75f) / 2.0f;

				float h = plain + mountains;

				vert [i].y = curve.Evaluate (h) * 50f;
				if (i % 50 == 0) {
					yield return null;
				}
			}
			mesh.vertices = vert;

			mesh.RecalculateBounds ();
			mesh.RecalculateNormals ();
		}
    }
}