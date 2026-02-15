using Godot;
using TFGate2.scripts.grid;

public partial class GridPawn : Node3D
{
    [Export(hintString: "Should this pawn snap to the center of its cell?")]
    public bool ShouldSnapToCellCenter { get; set; } = false;
    
    public override void _Ready()
    {
        var root = GetTree().CurrentScene;

        var gridManager = root.GetNode<GridManager>("WorldState/GridManager");
        if (gridManager == null)
        {
            GD.PrintErr("GridManager not found!");
            return;
        }

        gridManager.AddPawn(this);
    }
}