using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGameLibrary.Particles;

namespace MonoGameLibrary.Scenes;

/// <summary>
/// Base class for all scenes / screens in your game.
/// 
/// A scene represents a self-contained part of the game:
/// - Main menu
/// - Gameplay
/// - Pause menu
/// - Settings, etc.
/// 
/// Each scene:
/// - Owns its own <see cref="ContentManager"/> for scene-specific assets.
/// - Uses a shared <see cref="SceneContext"/> to access graphics, input, audio, etc.
/// - Is updated and drawn by a <see cref="SceneManager"/> or your game.
/// </summary>
public abstract class Scene : IDisposable
{
    /// <summary>
    /// Shared services provided by the game (graphics, input, audio, etc.).
    /// </summary>
    protected SceneContext Context { get; }

    /// <summary>
    /// Content manager used to load assets that belong only to this scene.
    /// </summary>
    /// <remarks>
    /// This ContentManager is created when the scene is constructed and disposed when
    /// the scene is disposed. Everything loaded through it will be unloaded together.
    /// </remarks>
    protected ContentManager Content { get; }

    /// <summary>
    /// Indicates whether this scene has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// When true on the active scene, <see cref="SceneManager"/> will draw scenes underneath it first,
    /// then draw the active scene on top. Useful for pause menus and popups.
    /// </summary>
    public virtual bool DrawUnderlyingScenes => false;

    /// <summary>
    /// Creates a new scene instance with the given context.
    /// </summary>
    /// <param name="context">Shared services that this scene depends on.</param>
    protected Scene(SceneContext context)
    {
        // Make sure we actually received a context.
        ArgumentNullException.ThrowIfNull(context);

        // Store the context so derived scenes can use it.
        Context = context;

        // Create a child ContentManager using the same services as the game.
        // This lets the scene control its own content lifetime.
        Content = new ContentManager(Context.Services)
        {
            // Use the same root directory as the game's ContentManager so asset paths match.
            RootDirectory = Context.ContentRootDirectory
        };
    }

    // NOTE: No finalizer (~Scene) is used here.
    // Scenes only own managed resources, and we want explicit, deterministic cleanup via Dispose().

    /// <summary>
    /// Initializes the scene.
    /// </summary>
    /// <remarks>
    /// Called once when the scene first becomes active.
    /// The default implementation calls <see cref="LoadContent"/>.
    /// 
    /// Override this method to perform set-up that does not depend on loaded content
    /// (e.g. creating data structures), but remember to call <c>base.Initialize()</c>
    /// if you still want <see cref="LoadContent"/> to be called.
    /// </remarks>
    public virtual void Initialize()
    {
        // By default, just load the content.
        LoadContent();
    }

    /// <summary>
    /// Loads all assets used by this scene.
    /// </summary>
    /// <remarks>
    /// Override this in your derived scene to load textures, fonts, sounds, etc.
    /// 
    /// Example:
    /// <code>
    /// _background = Content.Load&lt;Texture2D&gt;("Backgrounds/MainMenu");
    /// _clickSound = Content.Load&lt;SoundEffect&gt;("Audio/Click");
    /// </code>
    /// </remarks>
    public virtual void LoadContent()
    {
        // Default implementation does nothing.
    }

    /// <summary>
    /// Unloads content that was loaded for this scene.
    /// </summary>
    /// <remarks>
    /// Called during <see cref="Dispose(bool)"/> before the ContentManager is disposed.
    /// 
    /// The default implementation calls <see cref="ContentManager.Unload"/> which unloads
    /// all assets loaded through this ContentManager.
    /// 
    /// If you override this, you can:
    /// - Unsubscribe from events.
    /// - Clear caches.
    /// - Release other managed resources.
    /// 
    /// Usually you still want to call <c>base.UnloadContent()</c>.
    /// </remarks>
    public virtual void UnloadContent()
    {
        // Unload everything this ContentManager loaded.
        Content.Unload();
    }

    /// <summary>
    /// Updates this scene.
    /// </summary>
    /// <param name="gameTime">Timing information for the current frame.</param>
    public virtual void Update(GameTime gameTime)
    {
        // Guard: if someone tries to use a disposed scene, fail fast.
        if (IsDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }

        // Default implementation does nothing.
        // Derived scenes should override this.
    }

    /// <summary>
    /// Draws this scene.
    /// </summary>
    /// <param name="gameTime">Timing information for the current frame.</param>
    public virtual void Draw(GameTime gameTime)
    {
        // Guard against drawing after disposal.
        if (IsDisposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }

        // Default implementation does nothing.
    }

    /// <summary>
    /// Disposes this scene and releases its resources.
    /// </summary>
    public void Dispose()
    {
        // If we are already disposed, no further work is needed.
        if (IsDisposed)
        {
            return;
        }

        // Call the core disposal logic, specifying that this is explicit disposal.
        Dispose(true);

        // Tell the GC there's no need for a finalizer (we don't have one anyway).
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Core disposal logic.
    /// </summary>
    /// <param name="disposing">
    /// True when called from <see cref="Dispose()"/>, false when called from a finalizer.
    /// Scenes only have managed resources, so we only clean up when disposing is true.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        // If we've already run disposal logic, bail out.
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            // Release managed resources in a safe order.

            // 1. Let the scene unload any custom content or detach event handlers.
            UnloadContent();

            // 2. Dispose the ContentManager so it can free whatever it holds internally.
            Content.Dispose();
        }

        // Mark as disposed to prevent repeated cleanup and guard Update/Draw.
        IsDisposed = true;
    }
}
