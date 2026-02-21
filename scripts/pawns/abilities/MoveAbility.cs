using Godot;
using TFGate2.scripts.grid;
using System;

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

    public override bool CanExecute(AbilityExecutionContext context)
    {
        if (context.SourcePawn == null || context.TargetCell == null)
            return false;

        if (context.SourcePawn.OccupiedCell == null || _remainingDistance <= 0)
            return false;

        return context.GridManager.CanMovePawnToCell(context.SourcePawn, context.TargetCell, _remainingDistance);
    }

    public override void Execute(AbilityExecutionContext context)
    {
        if (!CanExecute(context))
            return;

        var sourceCell = context.SourcePawn.OccupiedCell;
        var targetCell = context.TargetCell;
        var movementCost = Mathf.Abs(sourceCell.Coordinate.X - targetCell.Coordinate.X) +
                           Mathf.Abs(sourceCell.Coordinate.Y - targetCell.Coordinate.Y);

        if (!context.GridManager.TryMovePawnToCell(context.SourcePawn, targetCell))
            return;

        _remainingDistance = Math.Max(0, _remainingDistance - movementCost);
        GD.Print($"[ABILITY] Move executed by {context.SourcePawn.Name}. Remaining distance: {_remainingDistance}");
    }
}
