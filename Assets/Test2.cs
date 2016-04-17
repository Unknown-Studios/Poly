using CoherentNoise.Generation.Fractal;
using UnityEngine;

public class Test2 : MonoBehaviour
{
    public int Octaves = 3;
    public float groundFrq = 0.005f;
    public Texture2D tex;

    public int mOctaves = 3;
    public float mFreq = 0.1f;

    private RidgeNoise noise;
    private PinkNoise pink;

    private int CurOct;
    private float CurFrq;

    private void Start()
    {
        Random.seed = PlayerPrefs.GetInt("Seed");
        noise = new RidgeNoise(Random.seed);
        pink = new PinkNoise(Random.seed);
        pink.OctaveCount = Octaves;
        pink.Frequency = groundFrq;

        noise.OctaveCount = mOctaves;
        noise.Frequency = mFreq;
        GenerateTexture();
    }

    private void Update()
    {
        if (CurOct != mOctaves || CurFrq != mFreq)
        {
            CurFrq = mFreq;
            CurOct = mOctaves;
            noise.OctaveCount = mOctaves;
            noise.Frequency = mFreq;
            GenerateTexture();
        }
    }

    private void GenerateTexture()
    {
        tex = new Texture2D(512, 512);
        for (int x = 0; x < 512; x++)
        {
            for (int y = 0; y < 512; y++)
            {
                float plain = pink.GetValue(x, y, 0) / 2.0f;
                float mountains = Mathf.Max(0f, noise.GetValue(x - 549, y + 2585, 0f) - 0.75f) / 2.0f;

                float h = plain + mountains;

                tex.SetPixel(x, y, Color.white * h);
            }
        }
        tex.Apply();

        tex.mipMapBias = 1;
    }
}