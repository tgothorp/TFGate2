using Godot;

namespace TFGate2.scripts.grid;

/// <summary>
/// Represents a path through the grid.
/// </summary>
public readonly struct GridPath
{
    public bool PathIsValid { get; }
    public Vector2I Start { get; }
    public Vector2I End { get; }
    public int Cost { get; }

    public GridPath(bool pathIsValid,
        Vector2I start,
        Vector2I end,
        int cost)
    {
        PathIsValid = pathIsValid;
        Start = start;
        End = end;
        Cost = cost;
    }

    public static GridPath Invalid => new GridPath(false, Vector2I.Zero, Vector2I.Zero, 0);
}