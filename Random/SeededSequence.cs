using System;

namespace MonoGameLibrary.Random;

/// <summary>
/// Generates derived seeds from a root seed so different systems
/// can have independent but reproducible random streams.
/// </summary>
public sealed class SeedSequence
{
    private readonly System.Random _rng;

    public int RootSeed { get; }

    public SeedSequence(int rootSeed)
    {
        RootSeed = rootSeed;
        _rng = new System.Random(rootSeed);
    }

    /// <summary>Returns a new SeededRandom using a derived seed.</summary>
    public SeededRandom NextStream()
    {
        int derived = _rng.Next(int.MinValue, int.MaxValue);
        return new SeededRandom(derived);
    }

    /// <summary>Returns a raw derived seed.</summary>
    public int NextSeed()
        => _rng.Next(int.MinValue, int.MaxValue);
}
