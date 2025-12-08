using System;
using System.Collections.Generic;

namespace MonoGameLibrary.Pathfinding;

/// <summary>
/// Generic A* pathfinding over a 2D grid. The caller defines
/// which cells are walkable and how much moves cost.
/// </summary>
public static class AStar
{
    public readonly struct Node
    {
        public readonly int X;
        public readonly int Y;

        public Node(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public static List<Node> FindPath(
        int width,
        int height,
        Node start,
        Node goal,
        Func<int, int, bool> isWalkable,
        Func<int, int, int, int, float> moveCost,
        Func<int, int, int, int, float>? heuristic = null)
    {
        heuristic ??= HeuristicManhattan;

        var openSet = new PriorityQueue<Node, float>();
        var cameFrom = new Dictionary<Node, Node>();
        var gScore = new Dictionary<Node, float>
        {
            [start] = 0f
        };

        openSet.Enqueue(start, 0f);

        var directions = new (int dx, int dy)[]
        {
            (1, 0), (-1, 0), (0, 1), (0, -1)
            // add diagonals if you want
        };

        while (openSet.TryDequeue(out var current, out _))
        {
            if (current.X == goal.X && current.Y == goal.Y)
                return ReconstructPath(cameFrom, current);

            foreach (var (dx, dy) in directions)
            {
                int nx = current.X + dx;
                int ny = current.Y + dy;

                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;

                if (!isWalkable(nx, ny))
                    continue;

                float tentativeG = gScore[current] +
                                   moveCost(current.X, current.Y, nx, ny);

                var neighbor = new Node(nx, ny);
                if (!gScore.TryGetValue(neighbor, out float oldG) || tentativeG < oldG)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;

                    float f = tentativeG + heuristic(nx, ny, goal.X, goal.Y);
                    openSet.Enqueue(neighbor, f);
                }
            }
        }

        // No path
        return new List<Node>();
    }

    private static float HeuristicManhattan(int x1, int y1, int x2, int y2)
        => System.Math.Abs(x1 - x2) + System.Math.Abs(y1 - y2);

    private static List<Node> ReconstructPath(
        Dictionary<Node, Node> cameFrom,
        Node current)
    {
        var path = new List<Node> { current };
        while (cameFrom.TryGetValue(current, out var prev))
        {
            current = prev;
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}
