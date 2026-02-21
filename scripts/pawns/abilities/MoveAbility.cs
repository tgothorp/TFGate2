using Godot;
using TFGate2.scripts.grid;

namespace TFGate2.scripts.pawns.abilities;

public partial class MoveAbility : PawnAbility
{
    [Export]
    public int Distance { get; set; } = 7;

    private int _remainingDistance;

    public override void _EnterTree()
    {
        _remainingDistance = Distance;
        base._EnterTree();
    }

    public override bool CanExecute(GridPawn targetPawn, GridCell targetCell)
    {
        // TODO
        return true;
    }

    public override void Execute(GridPawn targetPawn, GridCell targetCell)
    {
        throw new System.NotImplementedException();
    }
}