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


	void Update() {
		if (!drawing) {
			StartCoroutine (GenerateTexture ());
		}
	}

	void Start() {

		tex = new Texture2D (250, 250);
	}

	void OnGUI() {
		GUI.DrawTexture (new Rect ((Screen.width/2)-125,(Screen.height/2)-125, 250, 250), tex);
	}

	private bool drawing;
	public Texture2D tex;
	public float scale;
	public float speed = 1.0f;

    private IEnumerator GenerateTexture()
    {
		drawing = true;
		for (int x = 0; x < tex.width; x++) {
			for (int y = 0; y < tex.height; y++) {
				
				float h = Mathf.PerlinNoise((((float)(x+(Time.time*speed))/tex.width)/scale), ((float)(y+(Time.time*speed))/tex.height)/scale);

				tex.SetPixel (x, y,  new Color(h,h,h,1));
			}
			if (x % 10 == 0) {
				yield return null;
			}
		}
		tex.Apply ();
		yield return null;
		drawing = false;
    }
}