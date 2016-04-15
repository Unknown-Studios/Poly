using UnityEngine;

public class PerlinNoise
{
    private const int B = 256;
    private int[] m_perm = new int[B + B];

    public PerlinNoise(int seed)
    {
        UnityEngine.Random.seed = seed;

        int i, j, k;
        for (i = 0; i < B; i++)
        {
            m_perm[i] = i;
        }

        while (--i != 0)
        {
            k = m_perm[i];
            j = UnityEngine.Random.Range(0, B);
            m_perm[i] = m_perm[j];
            m_perm[j] = k;
        }

        for (i = 0; i < B; i++)
        {
            m_perm[B + i] = m_perm[i];
        }
    }

    public float FractalNoise2D(float x, float y, int octNum, float frq, float amp)
    {
        float gain = 1.0f;
        float sum = 0.0f;

        for (int i = 0; i < octNum; i++)
        {
            sum += Noise2D(x * gain / frq, y * gain / frq) * amp / gain;
            gain *= 2.0f;
        }
        return sum;
    }

    private float FADE(float t)
    { return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f); }

    private float GRAD2(int hash, float x, float y)
    {
        //This method uses the mod operator which is slower
        //than bitwise operations but is included out of interest
        //		int h = hash % 16;
        //    	float u = h<4 ? x : y;
        //    	float v = h<4 ? y : x;
        //		int hn = h%2;
        //		int hm = (h/2)%2;
        //    	return ((hn != 0) ? -u : u) + ((hm != 0) ? -2.0f*v : 2.0f*v);

        int h = hash & 7;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return (((h & 1) != 0) ? -u : u) + (((h & 2) != 0) ? -2.0f * v : 2.0f * v);
    }

    private float LERP(float t, float a, float b)
    { return (a) + (t) * ((b) - (a)); }

    private float Noise2D(float x, float y)
    {
        //returns a noise value between -0.75 and 0.75
        int ix0, iy0, ix1, iy1;
        float fx0, fy0, fx1, fy1, s, t, nx0, nx1, n0, n1;

        ix0 = (int)Mathf.Floor(x);  // Integer part of x
        iy0 = (int)Mathf.Floor(y);  // Integer part of y
        fx0 = x - ix0;          // Fractional part of x
        fy0 = y - iy0;          // Fractional part of y
        fx1 = fx0 - 1.0f;
        fy1 = fy0 - 1.0f;
        ix1 = (ix0 + 1) & 0xff; // Wrap to 0..255
        iy1 = (iy0 + 1) & 0xff;
        ix0 = ix0 & 0xff;
        iy0 = iy0 & 0xff;

        t = FADE(fy0);
        s = FADE(fx0);

        nx0 = GRAD2(m_perm[ix0 + m_perm[iy0]], fx0, fy0);
        nx1 = GRAD2(m_perm[ix0 + m_perm[iy1]], fx0, fy1);

        n0 = LERP(t, nx0, nx1);

        nx0 = GRAD2(m_perm[ix1 + m_perm[iy0]], fx1, fy0);
        nx1 = GRAD2(m_perm[ix1 + m_perm[iy1]], fx1, fy1);

        n1 = LERP(t, nx0, nx1);

        return 0.507f * LERP(s, n0, n1);
    }
}