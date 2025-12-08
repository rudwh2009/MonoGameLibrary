using System;

namespace MonoGameLibrary.Pathfinding;

/// <summary>
/// Simple 2D grid wrapper. Can hold any type of cell.
/// Useful for maps and pathfinding.
/// </summary>
public sealed class Grid2D<T>
{
    private readonly T[] _cells;

    public int Width { get; }
    public int Height { get; }

    public Grid2D(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        Width = width;
        Height = height;
        _cells = new T[width * height];
    }

    private int Index(int x, int y) => y * Width + x;

    public bool InBounds(int x, int y)
        => x >= 0 && y >= 0 && x < Width && y < Height;

    public T this[int x, int y]
    {
        get => _cells[Index(x, y)];
        set => _cells[Index(x, y)] = value;
    }
}
