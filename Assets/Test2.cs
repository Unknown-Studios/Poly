using CoherentNoise.Generation.Fractal;
using System.Collections;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    public int Octaves = 3;
    public float groundFrq = 0.01f;
    public float Persistence = 0.17f;

    public int mOctaves = 3;
    public float mFreq = 0.1f;

    public Texture2D tex;

    public float scale;

    public float speed = 1.0f;

    private bool drawing;

    private RidgeNoise noise;

    private void Update()
    {
        if (!drawing)
        {
            StartCoroutine(GenerateTexture());
        }
    }

    private void Start()
    {
        tex = new Texture2D(250, 250);

        noise = new RidgeNoise(PlayerPrefs.GetInt("Seed"));
        noise.OctaveCount = 3;
        noise.Gain = 1.2f;
        noise.Exponent = 2f;
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect((Screen.width / 2) - 125, (Screen.height / 2) - 125, 250, 250), tex);
    }

    private IEnumerator GenerateTexture()
    {
        drawing = true;
        noise.Frequency = groundFrq;
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                float h = Mathf.PerlinNoise((((float)x / tex.width) / scale), ((float)y / tex.height) / scale);

                float n = Mathf.Max(0.0f, noise.GetValue(x - 549, y + 2585, 54));
                float mountains = Mathf.Max(0f, (n - 0.5f));

                h = mountains != 0 ? mountains : mountains;

                tex.SetPixel(x, y, new Color(h, h, h, 1));
            }
            if (x % 10 == 0)
            {
                yield return null;
            }
        }
        tex.Apply();
        yield return null;
        drawing = false;
    }
}