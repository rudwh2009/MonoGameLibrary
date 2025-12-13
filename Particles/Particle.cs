using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Particles;

public class Particle<T>
{
    public Texture2D Texture;
    public Vector2 Position;
    public float Orientation;
    public Vector2 Scale = Vector2.One;

    public ParticleBlendMode BlendMode = ParticleBlendMode.Alpha;

    private Color _tint = Color.White;
    public Color Tint { get => _tint; set => _tint = value; }
    public Color Color { get => _tint; set => _tint = value; }

    /// <summary>
    /// Lifespan in seconds.
    /// </summary>
    public float Duration;

    /// <summary>
    /// Remaining life in normalized range [0..1].
    /// </summary>
    public float PercentLife = 1f;

    public T State;
}