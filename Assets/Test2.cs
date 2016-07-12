using UnityEngine;
using CoherentNoise.Generation.Fractal;

public class Test2 : MonoBehaviour
{
	int Octaves = 2;

    private void Start()
    {
		Texture2D tex = new Texture2D (1000,1000);
		RidgeNoise noise = new RidgeNoise(PlayerPrefs.GetInt("Seed"));
		PinkNoise pink = new PinkNoise(PlayerPrefs.GetInt("Seed"));
		BillowNoise hills = new BillowNoise(PlayerPrefs.GetInt("Seed"));

		pink.Frequency = 0.005f;
		pink.Lacunarity = 1.2f;
		pink.Persistence = 0.17f;

		noise.OctaveCount = Octaves;
		noise.Frequency = 0.0015f;
		noise.Gain = 1f;
		noise.Exponent = 2f;

		hills.OctaveCount = 2;
		hills.Lacunarity = 0.5f;
		hills.Frequency = 0.01f;

		for (int x = 0; x < tex.width; x++) {
			for (int y = 0; y < tex.width; y++) {
				float plain = (pink.GetValue(x / 1.5f, y / 1.5f, 0 / 1.5f) + 1.0f) / 4.0f;

				float val = noise.GetValue(x - 549, y + 2585, 54);
				float n = val < 0 ? 0 : val;
				val = (n - 0.5f) / 2.5f;
				float mountains = val < 0 ? 0 : val;

				val = hills.GetValue(x + 549, y - 2585,-544);
				float hill = val < 0 ? 0 : val;
				val = (hill - 0.25f) / 7.5f;
				hill = val < 0 ? 0 : val;

				float no = plain + mountains + hill;
				tex.SetPixel (x, y, new Color(no,no,no));
			}
		}

		tex.Apply ();
		gameObject.GetComponent<MeshRenderer> ().material.mainTexture = tex;
    }
}