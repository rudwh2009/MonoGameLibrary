using System;

namespace MonoGameLibrary.Random;

/// <summary>
/// Wrapper around System.Random with an explicit seed and convenience helpers.
/// </summary>
public sealed class SeededRandom
{
    private readonly System.Random _rng;

    public int Seed { get; }

    public SeededRandom(int seed)
    {
        Seed = seed;
        _rng = new System.Random(seed);
    }

    public int NextInt(int minInclusive, int maxExclusive)
        => _rng.Next(minInclusive, maxExclusive);

    public double NextDouble()
        => _rng.NextDouble();

    public float NextFloat()
        => (float)_rng.NextDouble();

    public float NextFloat(float minInclusive, float maxInclusive)
        => minInclusive + (float)_rng.NextDouble() * (maxInclusive - minInclusive);

    /// <summary>Returns true with the given probability (0.0 - 1.0).</summary>
    public bool Chance(double probability)
        => _rng.NextDouble() < probability;

    /// <summary>Shuffles an array in-place using Fisher-Yates.</summary>
    public void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = _rng.Next(0, i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
