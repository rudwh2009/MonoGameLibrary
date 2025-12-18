using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace MonoGameLibrary.Input;

public sealed class TouchInfo
{
    private bool _previousDown;
    private bool _currentDown;

    private Point _previousPos;
    private Point _currentPos;

    public bool IsDown => _currentDown;
    public bool IsUp => !_currentDown;

    public bool WasJustPressed => _currentDown && !_previousDown;
    public bool WasJustReleased => !_currentDown && _previousDown;

    public Point Position => _currentPos;
    public Point PositionDelta => _currentPos - _previousPos;

    public void Update()
    {
        _previousDown = _currentDown;
        _previousPos = _currentPos;

        TouchCollection touches = TouchPanel.GetState();

        TouchLocation? primary = null;
        for (int i = 0; i < touches.Count; i++)
        {
            var t = touches[i];
            if (t.State == TouchLocationState.Pressed || t.State == TouchLocationState.Moved)
            {
                primary = t;
                break;
            }
        }

        if (primary.HasValue)
        {
            var t = primary.Value;
            _currentDown = true;
            _currentPos = new Point((int)t.Position.X, (int)t.Position.Y);
        }
        else
        {
            // No active touch this frame.
            _currentDown = false;
            // Keep last known position so UI can process a release at the last contact point.
            _currentPos = _previousPos;
        }
    }
}
