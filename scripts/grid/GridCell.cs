using Godot;

namespace TFGate2.scripts.grid;

public sealed class GridCell
{
    public GridCell(Vector2I coordinate, Vector3 worldPosition)
    {
        Coordinate = coordinate;
        WorldPosition = worldPosition;
    }
    
    /// <summary>
    /// Center of the cell in world space
    /// </summary>
    public Vector3 WorldPosition { get; private set; }
    
    /// <summary>
    /// Coordinate position of the cell in the grid
    /// </summary>
    public Vector2I Coordinate { get; private set; }
    
    public override string ToString() => $"(X:{Coordinate.X}, Y:{Coordinate.Y})";
}