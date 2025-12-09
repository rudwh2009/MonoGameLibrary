using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// Simple 2D camera that provides a transform matrix for panning/zooming.
/// </summary>
public sealed class Camera2D
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Zoom { get; set; } = 1f;
    public float Rotation { get; set; } = 0f;

    public Matrix GetViewMatrix(Viewport viewport)
    {
        var zoom = MathHelper.Max(Zoom, 0.01f);
        var center = new Vector2(viewport.Width * 0.5f, viewport.Height * 0.5f);

        return Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
               Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateScale(zoom, zoom, 1f) *
               Matrix.CreateTranslation(new Vector3(center, 0f));
    }
}
