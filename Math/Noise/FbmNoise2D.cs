namespace MonoGameLibrary.Math.Noise;

/// <summary>
/// Fractal Brownian Motion (FBM) wrapper around a base 2D noise.
/// </summary>
public sealed class FbmNoise2D : INoise2D
{
    private readonly INoise2D _baseNoise;
    private readonly int _octaves;
    private readonly float _persistence;
    private readonly float _lacunarity;

    public FbmNoise2D(INoise2D baseNoise,
                      int octaves = 4,
                      float persistence = 0.5f,
                      float lacunarity = 2f)
    {
        _baseNoise = baseNoise;
        _octaves = octaves;
        _persistence = persistence;
        _lacunarity = lacunarity;
    }

    public float Sample(float x, float y)
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float max = 0f;

        for (int i = 0; i < _octaves; i++)
        {
            total += _baseNoise.Sample(x * frequency, y * frequency) * amplitude;
            max += amplitude;
            amplitude *= _persistence;
            frequency *= _lacunarity;
        }

        return total / max; // normalize to roughly [-1,1]
    }
}
