using System;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Graphics;

public class AnimatedSprite : Sprite 
{
    private int _currentFrame;
    private TimeSpan _elapsed;
    private Animation _animation;

    public bool Loop { get; set; } = true;

	public int CurrentFrameIndex => _currentFrame;
	public TimeSpan ElapsedInFrame => _elapsed;

    /// <summary>
    /// Gets or Sets the animation for this animated sprite.
    /// </summary>
    public Animation Animation
    {
        get => _animation;
        set
        {
            if (ReferenceEquals(_animation, value))
                return;

            _animation = value;
            _currentFrame = 0;
            _elapsed = TimeSpan.Zero;

            if (_animation != null && _animation.Frames.Count != 0)
                Region = _animation.Frames[0];
        }
    }

    public (int frameIndex, TimeSpan elapsedInFrame) GetPlaybackState() => (_currentFrame, _elapsed);

    public void SetPlaybackState(int frameIndex, TimeSpan elapsedInFrame)
    {
        if (_animation == null || _animation.Frames == null || _animation.Frames.Count == 0)
            return;

        int clampedFrame = System.Math.Clamp(frameIndex, 0, _animation.Frames.Count - 1);
        TimeSpan clampedElapsed = elapsedInFrame;
        if (clampedElapsed < TimeSpan.Zero)
            clampedElapsed = TimeSpan.Zero;
        if (_animation.Delay > TimeSpan.Zero && clampedElapsed >= _animation.Delay)
            clampedElapsed = _animation.Delay;

        _currentFrame = clampedFrame;
        _elapsed = clampedElapsed;
        Region = _animation.Frames[_currentFrame];
    }

    /// <summary>
    /// Creates a new animated sprite.
    /// </summary>
    public AnimatedSprite() { }

    /// <summary>
    /// Creates a new animated sprite with the specified frames and delay.
    /// </summary>
    /// <param name="animation">The animation for this animated sprite.</param>
    public AnimatedSprite(Animation animation)
    {
        Animation = animation;
    }

    /// <summary>
    /// Updates this animated sprite.
    /// </summary>
    /// <param name="gameTime">A snapshot of the game timing values provided by the framework.</param>
    public void Update(GameTime gameTime)
    {
        if (_animation == null || _animation.Frames == null || _animation.Frames.Count == 0)
            return;

        // If non-looping and already on last frame, hold.
        if (!Loop && _currentFrame >= _animation.Frames.Count - 1)
            return;

        _elapsed += gameTime.ElapsedGameTime;

        if (_elapsed >= _animation.Delay)
        {
            _elapsed -= _animation.Delay;
            _currentFrame++;

            if (_currentFrame >= _animation.Frames.Count)
            {
                if (Loop)
                    _currentFrame = 0;
                else
                    _currentFrame = _animation.Frames.Count - 1;
            }

            Region = _animation.Frames[_currentFrame];
        }
    }

}
