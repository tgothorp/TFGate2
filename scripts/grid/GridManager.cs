using Godot;
using Godot.Collections;

namespace TFGate2.scripts.grid;

[Tool]
/// <summary>
/// Owns grid topology, cell selection state, and authoritative pawn occupancy/movement.
/// </summary>
public partial class GridManager : Node3D
{
    [Export]
    public float CellSize { get; set; } = 1.0f;

    [Export]
    public int Width { get; set; } = 20;

    [Export]
    public int Height { get; set; } = 20;

    [Export]
    public bool CanSelectGrid
    {
        get => _canSelectGrid;
        set
        {
            _canSelectGrid = value;
            if (!_canSelectGrid)
            {
                _hoveredCell = null;
            }
        }
    }

    public GridCell SelectedCell => _selectedCell;
    public GridCell HoveredCell => _hoveredCell;
    public GridPath SelectedPath => _selectedPath;

    private GridCell[,] _grid;
    private Dictionary<GridPawn, Vector2I> _occupiedPositions = new();
    private GridCell _selectedCell;
    private GridCell _hoveredCell;
    private GridPath _selectedPath;
    private bool _canSelectGrid = false;
    
    private WorldLogic _worldLogic;

    public override void _Ready()
    {
        BuildGrid();

        _worldLogic = GetParent<WorldLogic>();
        if (_worldLogic == null)
        {
            GD.PrintErr("WorldLogic not found!");
        }
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

    public void SelectCell(Vector2I coordinate)
    {
        if (!CanSelectGrid)
            return;

        _selectedCell = GetCell(coordinate);
        _worldLogic.GridCellSelected(_selectedCell);
    }

    public void SetHoveredCell(Vector2I? coordinate)
    {
        if (!CanSelectGrid)
        {
            _hoveredCell = null;
            return;
        }

        _hoveredCell = coordinate.HasValue ? GetCell(coordinate.Value) : null;
    }

    public GridCell AddPawn(GridPawn pawn)
    {
        var cell = WorldToGrid(pawn.GlobalPosition);
        if (!cell.HasValue)
        {
            GD.Print("Pawn not added to grid: out of bounds");
            return null;
        }

        GD.Print($"[GRID] Pawn added to grid: {pawn.Name} at {cell.Value}");
        _occupiedPositions[pawn] = cell.Value;

        if (pawn.ShouldSnapToCellCenter)
        {
            pawn.GlobalPosition = GridToWorld(cell.Value);
        }
        
        return _grid[cell.Value.X, cell.Value.Y];
    }

    public GridPath FindPath(GridCell startCell, GridCell endCell)
    {
        if (startCell == null || endCell == null)
            return GridPath.Invalid;

        if (startCell.Coordinate == endCell.Coordinate)
            return new GridPath(true, startCell.Coordinate, endCell.Coordinate, [], 0);

        if (IsCellOccupied(endCell.Coordinate))
            return new GridPath(false, startCell.Coordinate, endCell.Coordinate, [], 0);

        _selectedPath = GridPathCalculator.CalculatePath(this, startCell.Coordinate, endCell.Coordinate);
        return _selectedPath;
    }
    
    #region Helpers

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

    /// <summary>
    /// Converts a world position to grid coordinates. Returns null if outside grid bounds.
    /// </summary>
    public Vector2I? WorldToGrid(Vector3 worldPosition)
    {
        // Transform world position to local space
        var localPos = GlobalTransform.AffineInverse() * worldPosition;

        // Convert to grid coordinates
        int x = Mathf.FloorToInt(localPos.X / CellSize);
        int z = Mathf.FloorToInt(localPos.Z / CellSize);

        // Check bounds
        if (x < 0 || x >= Width || z < 0 || z >= Height)
            return null;

        return new Vector2I(x, z);
    }

    /// <summary>
    /// Gets a grid cell at the specified coordinate. Returns null if out of bounds.
    /// </summary>
    public GridCell GetCell(Vector2I coordinate)
    {
        if (!IsWithinBounds(coordinate))
            return null;

        return _grid[coordinate.X, coordinate.Y];
    }

    public bool IsWithinBounds(Vector2I coordinate)
    {
        return coordinate.X >= 0 && coordinate.X < Width && coordinate.Y >= 0 && coordinate.Y < Height;
    }

    public bool IsCellOccupied(Vector2I coordinate)
    {
        return _occupiedPositions.Values.Contains(coordinate);
    }
    
    #endregion
}
