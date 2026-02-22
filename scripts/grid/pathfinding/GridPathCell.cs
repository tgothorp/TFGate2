using Godot;

namespace TFGate2.scripts.grid;

/// <summary>
/// Represents a cell in the grid pathfinding graph.
/// </summary>
public struct GridPathCell
{
    public Vector2I Coordinate { get; set; }
    public Vector2I Parent { get; set; }

    public int F => G + H;
    public int G { get; set; }
    public int H { get; set; }

    public GridPathCell(Vector2I coordinate, int g, int h, Vector2I parent)
    {
        Coordinate = coordinate;
        G = g;
        H = h;
        Parent = parent;
    }
}
