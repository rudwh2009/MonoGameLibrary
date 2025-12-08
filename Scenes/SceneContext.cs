using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Input;

namespace MonoGameLibrary.Scenes;

/// <summary>
/// Provides the shared services a scene needs to function.
/// 
/// The game (or your Core host) creates one of these and passes it into every scene.
/// The scene system itself never needs to know what your Game subclass is.
/// </summary>
public sealed class SceneContext
{
    /// <summary>
    /// Global service provider, usually <see cref="Game.Services"/>.
    /// Used by <see cref="ContentManager"/> and other MonoGame systems.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// The root directory for content (e.g. "Content").
    /// Keeps scene-local ContentManagers in sync with the game's content pipeline.
    /// </summary>
    public string ContentRootDirectory { get; }

    /// <summary>
    /// The graphics device for creating resources and clearing/drawing.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; }

    /// <summary>
    /// Shared SpriteBatch for 2D rendering.
    /// Scenes can use this to draw sprites and text.
    /// </summary>
    public SpriteBatch SpriteBatch { get; }

    /// <summary>
    /// Central input manager (keyboard, mouse, gamepads).
    /// Updated once per frame by the host game.
    /// </summary>
    public InputManager Input { get; }

    /// <summary>
    /// Central audio controller (sound effects & music).
    /// </summary>
    public AudioController Audio { get; }

    /// <summary>
    /// Creates a new SceneContext that bundles all core services.
    /// </summary>
    /// <param name="services">Global game services (usually Game.Services).</param>
    /// <param name="contentRootDirectory">Root directory for content (e.g. "Content").</param>
    /// <param name="graphicsDevice">Graphics device used for rendering.</param>
    /// <param name="spriteBatch">Shared sprite batch for 2D drawing.</param>
    /// <param name="input">Central input manager.</param>
    /// <param name="audio">Central audio controller.</param>
    public SceneContext(
        IServiceProvider services,
        string contentRootDirectory,
        GraphicsDevice graphicsDevice,
        SpriteBatch spriteBatch,
        InputManager input,
        AudioController audio)
    {
        // Validate inputs early so scenes don’t fail later in subtle ways.
        ArgumentNullException.ThrowIfNull(services);         // Services must exist.
        ArgumentNullException.ThrowIfNull(graphicsDevice);   // GraphicsDevice must exist.
        ArgumentNullException.ThrowIfNull(spriteBatch);      // SpriteBatch must exist.
        ArgumentNullException.ThrowIfNull(input);            // Input manager must exist.
        ArgumentNullException.ThrowIfNull(audio);            // Audio controller must exist.
        ArgumentException.ThrowIfNullOrEmpty(contentRootDirectory); // Content root must be valid.

        // Store references in read-only properties.
        Services = services;
        ContentRootDirectory = contentRootDirectory;
        GraphicsDevice = graphicsDevice;
        SpriteBatch = spriteBatch;
        Input = input;
        Audio = audio;
    }
}
