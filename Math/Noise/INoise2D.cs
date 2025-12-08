namespace MonoGameLibrary.Math.Noise;

/// <summary>
/// Basic 2D noise interface. Implementations: Perlin, Simplex, etc.
/// Returns values typically in [-1, 1].
/// </summary>
public interface INoise2D
{
    float Sample(float x, float y);
}
