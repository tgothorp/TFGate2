using Godot;

namespace TFGate2.scripts;

public class GridCell
{
    /// <summary>
    /// Center of the cell in world space
    /// </summary>
    public Vector3 WorldPosition { get; set; }
    
    /// <summary>
    /// Normalized position of the cell in the grid
    /// </summary>
    public Vector3 GridPosition { get; set; }
}