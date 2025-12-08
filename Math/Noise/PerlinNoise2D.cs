using System;

namespace MonoGameLibrary.Math.Noise;

/// <summary>
/// Classic 2D Perlin noise. Returns values approximately in [-1, 1].
/// </summary>
public sealed class PerlinNoise2D : INoise2D
{
    private readonly int[] _perm = new int[512];

    public PerlinNoise2D(int seed = 0)
    {
        var rng = new System.Random(seed);
        var p = new int[256];
        for (int i = 0; i < 256; i++)
            p[i] = i;

        // Fisher-Yates shuffle
        for (int i = 255; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (p[i], p[j]) = (p[j], p[i]);
        }

        // Duplicate to avoid overflow
        for (int i = 0; i < 512; i++)
            _perm[i] = p[i & 255];
    }

    public float Sample(float x, float y)
    {
        int xi = (int)MathF.Floor(x) & 255;
        int yi = (int)MathF.Floor(y) & 255;

        float xf = x - MathF.Floor(x);
        float yf = y - MathF.Floor(y);

        float u = Fade(xf);
        float v = Fade(yf);

        int aa = _perm[_perm[xi] + yi];
        int ab = _perm[_perm[xi] + yi + 1];
        int ba = _perm[_perm[xi + 1] + yi];
        int bb = _perm[_perm[xi + 1] + yi + 1];

        float x1 = Lerp(Grad(aa, xf,     yf    ),
                        Grad(ba, xf - 1, yf    ), u);
        float x2 = Lerp(Grad(ab, xf,     yf - 1),
                        Grad(bb, xf - 1, yf - 1), u);

        return Lerp(x1, x2, v);
    }

    private static float Fade(float t)
        => t * t * t * (t * (t * 6 - 15) + 10);

    private static float Lerp(float a, float b, float t)
        => a + t * (b - a);

    private static float Grad(int hash, float x, float y)
    {
        switch (hash & 7)
        {
            case 0: return  x + y;
            case 1: return  x - y;
            case 2: return -x + y;
            case 3: return -x - y;
            case 4: return  x;
            case 5: return -x;
            case 6: return  y;
            default: return -y;
        }
    }
}
