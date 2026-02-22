using System.Collections.Generic;
using Godot;

namespace TFGate2.scripts.grid;

public static class GridPathCalculator
{
    private static readonly Vector2I[] NeighborOffsets =
    [
        new Vector2I(-1, -1),
        new Vector2I(0, -1),
        new Vector2I(1, -1),
        new Vector2I(-1, 0),
        new Vector2I(1, 0),
        new Vector2I(-1, 1),
        new Vector2I(0, 1),
        new Vector2I(1, 1)
    ];

    public static GridPath CalculatePath(GridManager gridManager, Vector2I start, Vector2I end)
    {
        if (gridManager == null || !gridManager.IsWithinBounds(start) || !gridManager.IsWithinBounds(end))
            return new GridPath(false, start, end, [], 0);

        if (start == end)
            return new GridPath(true, start, end, [], 0);

        var openSet = new HashSet<Vector2I> { start };
        var closedSet = new HashSet<Vector2I>();
        var nodes = new Dictionary<Vector2I, GridPathCell>
        {
            [start] = new GridPathCell(start, 0, Heuristic(start, end), start)
        };

        while (openSet.Count > 0)
        {
            var current = GetBestOpenNode(openSet, nodes);
            if (current == end)
                return BuildPath(nodes, start, end);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var offset in NeighborOffsets)
            {
                var neighbor = current + offset;

                if (!gridManager.IsWithinBounds(neighbor))
                    continue;

                if (closedSet.Contains(neighbor))
                    continue;

                if (gridManager.IsCellOccupied(neighbor))
                    continue;

                if (IsDiagonalMove(current, neighbor) && !CanTraverseDiagonal(gridManager, current, neighbor))
                    continue;

                var movementCost = IsDiagonalMove(current, neighbor) ? 14 : 10;
                var tentativeG = nodes[current].G + movementCost;

                if (!nodes.TryGetValue(neighbor, out var neighborCell) || tentativeG < neighborCell.G)
                {
                    nodes[neighbor] = new GridPathCell(
                        neighbor,
                        tentativeG,
                        Heuristic(neighbor, end),
                        current);

                    openSet.Add(neighbor);
                }
            }
        }

        return new GridPath(false, start, end, [], 0);
    }

    private static GridPath BuildPath(Dictionary<Vector2I, GridPathCell> nodes, Vector2I start, Vector2I end)
    {
        var reversedPath = new List<Vector2I>();
        var current = end;

        while (current != start)
        {
            if (!nodes.TryGetValue(current, out var currentCell))
                return new GridPath(false, start, end, [], 0);

            reversedPath.Add(current);
            current = currentCell.Parent;
        }

        reversedPath.Reverse();
        var finalPath = reversedPath.ToArray();

        return new GridPath(true, start, end, finalPath, finalPath.Length);
    }

    private static Vector2I GetBestOpenNode(HashSet<Vector2I> openSet, Dictionary<Vector2I, GridPathCell> nodes)
    {
        var first = true;
        var bestCoordinate = Vector2I.Zero;
        var bestF = int.MaxValue;
        var bestH = int.MaxValue;

        foreach (var coordinate in openSet)
        {
            var node = nodes[coordinate];
            if (first || node.F < bestF || (node.F == bestF && node.H < bestH))
            {
                first = false;
                bestCoordinate = coordinate;
                bestF = node.F;
                bestH = node.H;
            }
        }

        return bestCoordinate;
    }

    private static int Heuristic(Vector2I from, Vector2I to)
    {
        var dx = Mathf.Abs(from.X - to.X);
        var dy = Mathf.Abs(from.Y - to.Y);
        var diagonal = Mathf.Min(dx, dy);
        var straight = Mathf.Max(dx, dy) - diagonal;

        return (diagonal * 14) + (straight * 10);
    }

    private static bool IsDiagonalMove(Vector2I from, Vector2I to)
    {
        return Mathf.Abs(from.X - to.X) == 1 && Mathf.Abs(from.Y - to.Y) == 1;
    }

    private static bool CanTraverseDiagonal(GridManager gridManager, Vector2I from, Vector2I to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        if (Mathf.Abs(dx) != 1 || Mathf.Abs(dy) != 1)
            return true;

        var horizontalNeighbor = new Vector2I(from.X + dx, from.Y);
        var verticalNeighbor = new Vector2I(from.X, from.Y + dy);

        if (!gridManager.IsWithinBounds(horizontalNeighbor) || !gridManager.IsWithinBounds(verticalNeighbor))
            return false;

        return !gridManager.IsCellOccupied(horizontalNeighbor) && !gridManager.IsCellOccupied(verticalNeighbor);
    }
}
