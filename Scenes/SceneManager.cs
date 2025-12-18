using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Scenes;

/// <summary>
/// Manages a stack of scenes.
/// 
/// The top of the stack is considered the "active" scene:
/// - It receives <see cref="Update"/> and <see cref="Draw"/> calls.
/// - Scenes underneath are paused and not drawn (unless *they* do custom behavior).
///
/// This design lets you:
/// - Push a pause menu over gameplay.
/// - Push modal dialogs or overlays.
/// - Pop back to the previous scene when done.
/// </summary>
public sealed class SceneManager
{
    /// <summary>
    /// Stack of scenes. The last pushed scene is the active one.
    /// </summary>
    private readonly Stack<Scene> _scenes = new();

    /// <summary>
    /// Gets the scene at the top of the stack, or null if no scenes exist.
    /// </summary>
    public Scene? ActiveScene => _scenes.Count > 0 ? _scenes.Peek() : null;

    /// <summary>
    /// Pushes a new scene onto the stack and initializes it.
    /// </summary>
    /// <param name="scene">The scene to push and make active.</param>
    public void Push(Scene scene)
    {
        // Ensure we were given a valid scene.
        ArgumentNullException.ThrowIfNull(scene);

        // Add the scene to the top of the stack.
        _scenes.Push(scene);

        // Initialize triggers LoadContent inside the scene.
        // This is where the scene sets itself up.
        scene.Initialize();
    }

    /// <summary>
    /// Pops the active scene off the stack and disposes it.
    /// </summary>
    public void Pop()
    {
        // If there are no scenes, there's nothing to pop.
        if (_scenes.Count == 0)
        {
            return;
        }

        // Take the top scene off the stack.
        Scene top = _scenes.Pop();

        // Dispose it to release its resources.
        top.Dispose();
    }

    /// <summary>
    /// Replaces the active scene with a new one.
    /// </summary>
    /// <param name="scene">The scene that will become active.</param>
    public void Replace(Scene scene)
    {
        // Ensure the new scene is valid.
        ArgumentNullException.ThrowIfNull(scene);

        // Pop the current active scene (if any) and dispose it.
        Pop();

        // Push the new scene and initialize it.
        Push(scene);
    }

    /// <summary>
    /// Clears all scenes and pushes a new root scene.
    /// </summary>
    /// <param name="scene">The scene that becomes the only scene on the stack.</param>
    public void ClearAndPush(Scene scene)
    {
        // Dispose every existing scene.
        Clear();

        // Push and initialize the new root scene.
        Push(scene);
    }

    /// <summary>
    /// Disposes all scenes and empties the stack.
    /// </summary>
    public void Clear()
    {
        // While there are scenes on the stack…
        while (_scenes.Count > 0)
        {
            // Pop a scene off…
            Scene scene = _scenes.Pop();

            // …and dispose it.
            scene.Dispose();
        }

        // Optionally force a GC collection to free memory more aggressively.
        // (You might decide to remove this in a large game and rely on normal GC behavior.)
        GC.Collect();
    }

    /// <summary>
    /// Updates the active scene only.
    /// </summary>
    /// <param name="gameTime">Timing values for the current frame.</param>
    public void Update(GameTime gameTime)
    {
        // Forward the update call to the scene at the top of the stack.
        ActiveScene?.Update(gameTime);
    }

    /// <summary>
    /// Draws the active scene only.
    /// </summary>
    /// <param name="gameTime">Timing values for the current frame.</param>
    public void Draw(GameTime gameTime)
    {
        var active = ActiveScene;
        if (active == null)
            return;

        // If the active scene is an overlay, draw scenes underneath it first.
        if (active.DrawUnderlyingScenes)
        {
            // Stack enumeration yields top->bottom. We want bottom->top.
            var scenes = _scenes.ToArray();
            for (int i = scenes.Length - 1; i >= 0; i--)
                scenes[i].Draw(gameTime);
            return;
        }

        // Otherwise, draw only the active scene.
        active.Draw(gameTime);
    }
}
