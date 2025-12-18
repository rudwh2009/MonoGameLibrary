using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Audio;
using MonoGameLibrary.Input;
using MonoGameLibrary.Scenes;

namespace MonoGameLibrary;

public class Core : Game
{
    internal static Core s_instance;

    /// <summary>
    /// Gets a reference to the Core instance.
    /// </summary>
    public static Core Instance => s_instance;

    /// <summary>
    /// Gets the graphics device manager to control the presentation of graphics.
    /// </summary>
    public static GraphicsDeviceManager Graphics { get; private set; }

    /// <summary>
    /// Gets the graphics device used to create graphical resources and perform primitive rendering.
    /// </summary>
    public static new GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// Gets the sprite batch used for all 2D rendering.
    /// </summary>
    public static SpriteBatch SpriteBatch { get; private set; }

    /// <summary>
    /// Gets the content manager used to load global assets.
    /// </summary>
    public static new ContentManager Content { get; private set; }

    /// <summary>
    /// Gets a reference to to the input management system.
    /// </summary>
    public static InputManager Input { get; private set; }

    /// <summary>
    /// Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.
    /// </summary>
    public static bool ExitOnEscape { get; set; }

    /// <summary>
    /// Gets a reference to the audio control system.
    /// </summary>
    public static AudioController Audio { get; private set; }

    /// <summary>
    /// Manages the stack of scenes (main gameplay, menus, overlays).
    /// </summary>
    public static SceneManager SceneManager { get; private set; }

    /// <summary>
    /// Context object passed into all scenes.
    /// Bundles graphics, input, audio, and content information.
    /// </summary>
    public SceneContext SceneContext { get; private set; }

    private bool _useVirtualResolution;
    private int _virtualWidth;
    private int _virtualHeight;
    private int _virtualRenderScale = 1;
    private RenderTarget2D _virtualTarget;
    private Color _virtualClearColor = Color.CornflowerBlue;

    private Rectangle _virtualDestinationRect;

    /// <summary>
    /// Gets whether virtual resolution rendering is enabled.
    /// </summary>
    public bool IsVirtualResolutionEnabled => _useVirtualResolution;

    /// <summary>
    /// Gets the configured virtual backbuffer size.
    /// Only meaningful when <see cref="IsVirtualResolutionEnabled"/> is true.
    /// </summary>
    public Point VirtualResolution => new(_virtualWidth, _virtualHeight);

    /// <summary>
    /// Gets the integer scale factor currently used when presenting the virtual resolution.
    /// When virtual resolution is enabled, the engine renders the scene to the fixed virtual size,
    /// then presents it to the backbuffer using integer scaling + letterboxing.
    /// </summary>
    public int VirtualRenderScale => _useVirtualResolution ? System.Math.Max(1, _virtualRenderScale) : 1;

    /// <summary>
    /// Gets the destination rectangle on the backbuffer where the virtual render is presented.
    /// This accounts for integer scaling and letterboxing.
    /// </summary>
    public Rectangle VirtualDestinationRectangle
    {
        get
        {
            if (!_useVirtualResolution)
                return new Rectangle(0, 0,
                    System.Math.Max(1, GraphicsDevice?.PresentationParameters.BackBufferWidth ?? 1),
                    System.Math.Max(1, GraphicsDevice?.PresentationParameters.BackBufferHeight ?? 1));

            UpdateVirtualDestinationRectangle();
            return _virtualDestinationRect;
        }
    }

    /// <summary>
    /// The clear color used for the virtual backbuffer when virtual resolution is enabled.
    /// This is the "background" color you'll see behind scenes that don't draw a full-screen backdrop.
    /// </summary>
    public Color VirtualClearColor
    {
        get => _virtualClearColor;
        set => _virtualClearColor = value;
    }

    /// <summary>
    /// Creates a new Core instance.
    /// </summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    /// <param name="width">The initial width, in pixels, of the game window.</param>
    /// <param name="height">The initial height, in pixels, of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
    public Core(string title, int width, int height, bool fullScreen)
    {
        // Ensure that multiple cores are not created.
        if (s_instance != null)
        {
            throw new InvalidOperationException($"Only a single Core instance can be created");
        }

        // Store reference to engine for global member access.
        s_instance = this;

        // Create a new graphics device manager.
        Graphics = new GraphicsDeviceManager(this);

        // Set the graphics defaults
        Graphics.PreferredBackBufferWidth = width;
        Graphics.PreferredBackBufferHeight = height;
        Graphics.IsFullScreen = fullScreen;

        // Apply the graphic presentation changes.
        Graphics.ApplyChanges();

        // Set the window title
        Window.Title = title;

        // Pixel-art games usually want to scale to arbitrary window sizes.
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += (_, _) =>
        {
            // Keep the backbuffer in sync with the window size so our virtual-res scaler works.
            if (Graphics == null)
                return;
            var w = System.Math.Max(1, Window.ClientBounds.Width);
            var h = System.Math.Max(1, Window.ClientBounds.Height);

            // For a pixel-art "scale up" workflow, don't allow the window/backbuffer to shrink
            // below the virtual resolution.
            if (_useVirtualResolution && _virtualWidth > 0 && _virtualHeight > 0)
            {
                w = System.Math.Max(w, _virtualWidth);
                h = System.Math.Max(h, _virtualHeight);
            }
            if (Graphics.PreferredBackBufferWidth == w && Graphics.PreferredBackBufferHeight == h)
                return;
            Graphics.PreferredBackBufferWidth = w;
            Graphics.PreferredBackBufferHeight = h;
            Graphics.ApplyChanges();
        };

        // Set the core's content manager to a reference of the base Game's
        // content manager.
        Content = base.Content;

        // Set the root directory for content.
        Content.RootDirectory = "Content";

        // Mouse is visible by default.
        IsMouseVisible = true;

        // Exit on escape is true by default
        ExitOnEscape = true;        
    }

    /// <summary>
    /// Enables a fixed internal render resolution that is scaled up to the window.
    /// This is ideal for pixel art (render at 640x360, scale to 1280x720/1920x1080, etc.).
    /// </summary>
    public void SetVirtualResolution(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        _useVirtualResolution = true;
        _virtualWidth = width;
        _virtualHeight = height;

        // Ensure the current backbuffer is at least the virtual size.
        // (We still allow larger sizes; those will be integer-scaled with letterboxing.)
        if (Graphics != null)
        {
            int w = System.Math.Max(Graphics.PreferredBackBufferWidth, _virtualWidth);
            int h = System.Math.Max(Graphics.PreferredBackBufferHeight, _virtualHeight);
            if (w != Graphics.PreferredBackBufferWidth || h != Graphics.PreferredBackBufferHeight)
            {
                Graphics.PreferredBackBufferWidth = w;
                Graphics.PreferredBackBufferHeight = h;
                Graphics.ApplyChanges();
            }
        }

        // If graphics device exists, (re)create immediately.
        if (GraphicsDevice != null)
            EnsureVirtualTarget();
    }

    public void DisableVirtualResolution()
    {
        _useVirtualResolution = false;
        _virtualWidth = 0;
        _virtualHeight = 0;
        _virtualTarget?.Dispose();
        _virtualTarget = null;
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Set the core's graphics device to a reference of the base Game's
        // graphics device.
        GraphicsDevice = base.GraphicsDevice;

        // Create the sprite batch instance.
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        // Create the scene manager that manages scene stack.
        SceneManager = new SceneManager();

        // Create a new input manager.
        Input = new InputManager();

        // Create a new audio controller.
        Audio = new AudioController();

        // Build the SceneContext that will be passed into all scenes.
        SceneContext = new SceneContext(
            Services,                // Global services (for new ContentManagers).
            Content.RootDirectory,   // Content root directory.
            GraphicsDevice,          // Graphics device.
            SpriteBatch,             // Shared SpriteBatch.
            Input,                   // Input manager.
            Audio                    // Audio controller.
        );

        EnsureVirtualTarget();
    }

    private void EnsureVirtualTarget()
    {
        if (!_useVirtualResolution)
            return;
        if (_virtualWidth <= 0 || _virtualHeight <= 0)
            return;
        if (GraphicsDevice == null)
            return;

        // Ensure our cached destination rectangle/scale is up-to-date before allocating.
        UpdateVirtualDestinationRectangle();

        // Render at the virtual resolution; presentation scaling happens when drawing to the backbuffer.
        int targetWidth = System.Math.Max(1, _virtualWidth);
        int targetHeight = System.Math.Max(1, _virtualHeight);

        if (_virtualTarget != null && _virtualTarget.Width == targetWidth && _virtualTarget.Height == targetHeight)
            return;

        _virtualTarget?.Dispose();
        _virtualTarget = new RenderTarget2D(
            GraphicsDevice,
            targetWidth,
            targetHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.DiscardContents);
    }

    protected override void UnloadContent()
    {
        // Dispose of the audio controller.
        Audio.Dispose();

        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        // Keep viewport consistent for update logic (camera, spawning, UI layout).
        // Draw() will temporarily switch viewport when presenting to the backbuffer.
        ApplySceneViewport();

        if (ExitOnEscape && Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            Exit();
        }

        // Update the input manager.
        Input.Update(gameTime);

        // Update the audio controller.
        Audio.Update();
        
        // Update the active scene via the SceneManager.
        SceneManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        EnsureVirtualTarget();

        if (_useVirtualResolution && _virtualTarget != null)
        {
            // 1) Draw the scene into the fixed-size virtual target.
            GraphicsDevice.SetRenderTarget(_virtualTarget);
            GraphicsDevice.Viewport = new Viewport(0, 0, _virtualWidth, _virtualHeight);
            GraphicsDevice.Clear(_virtualClearColor);
            SceneManager.Draw(gameTime);

            // 2) Present the virtual target to the backbuffer, scaled with pixel-perfect sampling.
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Viewport = new Viewport(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            GraphicsDevice.Clear(Color.Black);

            UpdateVirtualDestinationRectangle();
            var dest = _virtualDestinationRect;

            SpriteBatch.Begin(blendState: BlendState.Opaque, samplerState: SamplerState.PointClamp);
            SpriteBatch.Draw(_virtualTarget, dest, Color.White);
            SpriteBatch.End();

            // Restore the scene viewport so anything that queries GraphicsDevice.Viewport after Draw()
            // (or at the start of next Update()) sees the virtual resolution.
            ApplySceneViewport();
        }
        else
        {
            // Ask the SceneManager to draw the active scene.
            SceneManager.Draw(gameTime);
        }

        base.Draw(gameTime);
    }

    /// <summary>
    /// Converts a point in window/backbuffer coordinates to virtual-resolution coordinates.
    /// Returns coordinates in virtual pixels; if the point is in the letterbox area,
    /// the result may be outside the [0..VirtualWidth/Height) range unless clamped.
    /// </summary>
    public Point WindowToVirtual(Point windowPoint, bool clampToVirtualBounds = false)
    {
        if (!_useVirtualResolution)
            return windowPoint;

        UpdateVirtualDestinationRectangle();

        var dest = _virtualDestinationRect;
        int scale = System.Math.Max(1, dest.Width / System.Math.Max(1, _virtualWidth));

        int vx = (windowPoint.X - dest.X) / scale;
        int vy = (windowPoint.Y - dest.Y) / scale;

        if (clampToVirtualBounds)
        {
            vx = System.Math.Clamp(vx, 0, System.Math.Max(0, _virtualWidth - 1));
            vy = System.Math.Clamp(vy, 0, System.Math.Max(0, _virtualHeight - 1));
        }

        return new Point(vx, vy);
    }

    /// <summary>
    /// Convenience accessor for the current mouse position in virtual coordinates.
    /// </summary>
    public Point MousePositionVirtual(bool clampToVirtualBounds = false) => WindowToVirtual(Input.Mouse.Position, clampToVirtualBounds);

    private void ApplySceneViewport()
    {
        if (GraphicsDevice == null)
            return;

        if (_useVirtualResolution && _virtualWidth > 0 && _virtualHeight > 0)
        {
            GraphicsDevice.Viewport = new Viewport(0, 0, _virtualWidth, _virtualHeight);
            return;
        }

        GraphicsDevice.Viewport = new Viewport(
            0,
            0,
            System.Math.Max(1, GraphicsDevice.PresentationParameters.BackBufferWidth),
            System.Math.Max(1, GraphicsDevice.PresentationParameters.BackBufferHeight));
    }

    private void UpdateVirtualDestinationRectangle()
    {
        if (GraphicsDevice == null)
        {
            _virtualDestinationRect = new Rectangle(0, 0, 1, 1);
            _virtualRenderScale = 1;
            return;
        }

        if (!_useVirtualResolution || _virtualWidth <= 0 || _virtualHeight <= 0)
        {
            _virtualDestinationRect = new Rectangle(0, 0,
                System.Math.Max(1, GraphicsDevice.PresentationParameters.BackBufferWidth),
                System.Math.Max(1, GraphicsDevice.PresentationParameters.BackBufferHeight));
            _virtualRenderScale = 1;
            return;
        }

        int w = System.Math.Max(1, GraphicsDevice.PresentationParameters.BackBufferWidth);
        int h = System.Math.Max(1, GraphicsDevice.PresentationParameters.BackBufferHeight);
        int vw = System.Math.Max(1, _virtualWidth);
        int vh = System.Math.Max(1, _virtualHeight);

        int scaleX = w / vw;
        int scaleY = h / vh;
        _virtualRenderScale = System.Math.Max(1, System.Math.Min(scaleX, scaleY));

        _virtualDestinationRect = GetLetterboxedDestination(w, h, vw, vh);
    }

    private static Rectangle GetLetterboxedDestination(int backBufferWidth, int backBufferHeight, int virtualWidth, int virtualHeight)
    {
        int w = System.Math.Max(1, backBufferWidth);
        int h = System.Math.Max(1, backBufferHeight);
        int vw = System.Math.Max(1, virtualWidth);
        int vh = System.Math.Max(1, virtualHeight);

        int scaleX = w / vw;
        int scaleY = h / vh;
        int scale = System.Math.Max(1, System.Math.Min(scaleX, scaleY));

        int destW = vw * scale;
        int destH = vh * scale;
        int x = (w - destW) / 2;
        int y = (h - destH) / 2;
        return new Rectangle(x, y, destW, destH);
    }
}
