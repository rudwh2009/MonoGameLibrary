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

    /// <summary>
    /// When enabled, the camera translation is snapped to the pixel grid in screen space.
    /// This avoids visible shimmer/jitter when using PointClamp sampling with zoom.
    /// Note: pixel snapping is most appropriate when <see cref="Rotation"/> is 0.
    /// </summary>
    public bool PixelSnap { get; set; } = true;

    /// <summary>
    /// Snaps a world position so that it lands on whole pixels in screen space.
    /// Useful for pixel art when using PointClamp sampling.
    /// </summary>
    public Vector2 SnapToPixelGrid(Vector2 worldPosition)
    {
        if (!PixelSnap)
            return worldPosition;

        var zoom = MathHelper.Max(Zoom, 0.01f);
        float step = 1f / zoom;
        return new Vector2(
            System.MathF.Round(worldPosition.X / step) * step,
            System.MathF.Round(worldPosition.Y / step) * step);
    }

    public Matrix GetViewMatrix(Viewport viewport)
    {
        var zoom = MathHelper.Max(Zoom, 0.01f);
        var center = new Vector2(viewport.Width * 0.5f, viewport.Height * 0.5f);

        var position = Position;
        if (PixelSnap)
        {
            // Snap in screen space then convert back to world space:
            // screen = world * zoom => worldStep = 1/zoom
            float step = 1f / zoom;
            position = new Vector2(
                System.MathF.Round(position.X / step) * step,
                System.MathF.Round(position.Y / step) * step);
        }

        return Matrix.CreateTranslation(new Vector3(-position, 0f)) *
               Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateScale(zoom, zoom, 1f) *
               Matrix.CreateTranslation(new Vector3(center, 0f));
    }
}
