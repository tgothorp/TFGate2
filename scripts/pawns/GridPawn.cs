using System;
using Godot;
using TFGate2.scripts.grid;

/// <summary>
/// Pawn base class, a pawn occupies a single cell in the grid.
/// </summary>
public partial class GridPawn : Node3D
{
    public Guid PawnId { get; set; }
    
    [Export(hintString: "Should this pawn snap to the center of its cell?")]
    public bool ShouldSnapToCellCenter { get; set; } = false;

    [Export]
    public CollisionShape3D CollisionShape { get; set; }

    public GridCell OccupiedCell { get; private set; }

    public override void _Ready()
    {
        var root = GetTree().CurrentScene;

        var pawnManager = root.GetNode<PawnManager>("WorldManager/PawnManager");
        if (pawnManager == null)
        {
            GD.PrintErr("[PAWN] PawnManager not found!");
            return;
        }
        
        PawnId = pawnManager.RegisterPawn(this);
        
        var gridManager = root.GetNode<GridManager>("WorldManager/GridManager");
        if (gridManager == null)
        {
            GD.PrintErr("[PAWN] GridManager not found!");
            return;
        }

        var cell = gridManager.AddPawn(this);
        if (cell != null)
        {
            OccupiedCell = cell;
        }
    }

    public Vector3 GetCenterMass()
    {
        return CollisionShape.GlobalPosition;
    }

    public void SetOccupiedCell(GridCell cell)
    {
        OccupiedCell = cell;
    }

    public override string ToString()
    {
        return $"[PAWN] {Name}";
    }
}