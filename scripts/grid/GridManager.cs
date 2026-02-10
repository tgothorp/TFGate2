using Godot;

namespace TFGate2.scripts.grid;

[Tool]
public partial class GridManager : Node3D
{
    [Export]
    public float CellSize { get; set; } = 1.0f;

    [Export]
    public int Width { get; set; } = 20;

    [Export]
    public int Height { get; set; } = 20;

    private GridCell[,] _grid;

    public override void _Ready()
    {
        BuildGrid();
    }
    
    private void BuildGrid()
    {
        _grid = new GridCell[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                var gridCoordinate = new Vector2I(x, z);
                var worldPosition = GridToWorld(gridCoordinate);
                
                _grid[x, z] = new GridCell(gridCoordinate, worldPosition);
            }
        }
    }

    /// <summary>
    /// Returns the position of a cell in world space
    /// </summary>
    public Vector3 GridToWorld(Vector2I coordinate)
    {
        var x = (coordinate.X + 0.5f) * CellSize;
        var z = (coordinate.Y + 0.5f) * CellSize;
        return GlobalTransform * new Vector3(x, 0f, z);
    }
    
    /// <summary>
    /// Returns the position of a cell in the grid's space, use this
    /// when positioning children of the grid manager.
    /// </summary>
    public Vector3 GridToLocal(Vector2I coordinate)
    {
        var x = (coordinate.X + 0.5f) * CellSize;
        var z = (coordinate.Y + 0.5f) * CellSize;
        return new Vector3(x, 0f, z);
    }
}