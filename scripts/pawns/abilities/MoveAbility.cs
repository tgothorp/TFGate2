using Godot;
using TFGate2.scripts.grid;

namespace TFGate2.scripts.pawns.abilities;

public partial class MoveAbility : PawnAbility
{
    private GridPath _path;

    public override bool CanExecute(AbilityExecutionContext context)
    {
        if (context.SourcePawn == null || context.TargetCell == null)
        {
            GD.PrintErr("Invalid ability execution context!");
            return false;
        }

        if (context.SourcePawn.OccupiedCell == null || context.SourcePawn.MoveBudget <= 0)
        {
            GD.PrintErr("Pawn cannot move!");
            return false;
        }

        _path = ResolvePath(context);
        if (!_path.PathIsValid)
        {
            GD.PrintErr("No path found!");
            return false;
        }

        if (_path.Cost > context.SourcePawn.MoveBudget)
        {
            GD.PrintErr("Path is too expensive!");
            return false;
        }

        return true;
    }

    public override void Execute(AbilityExecutionContext context)
    {
        if (!_path.PathIsValid || _path.Cost > context.SourcePawn.MoveBudget || context.SourcePawn.MoveBudget <= 0)
        {
            GD.PushWarning($"Pawn move state is not valid ({_path})");
        }
        
        GD.Print($"[ABILITY] Move executed by {context.SourcePawn.Name}. Path: {_path}");
        context.SourcePawn.SetMoveBudget(context.SourcePawn.MoveBudget - _path.Cost);

        // if (!CanExecute(context))
        //     return;
        //
        // var sourceCell = context.SourcePawn.OccupiedCell;
        // var targetCell = context.TargetCell;
        // var movementCost = Mathf.Abs(sourceCell.Coordinate.X - targetCell.Coordinate.X) +
        //                    Mathf.Abs(sourceCell.Coordinate.Y - targetCell.Coordinate.Y);
        //
        // if (!context.GridManager.TryMovePawnToCell(context.SourcePawn, targetCell))
        //     return;
        //
        // _remainingDistance = Math.Max(0, _remainingDistance - movementCost);
        // GD.Print($"[ABILITY] Move executed by {context.SourcePawn.Name}. Remaining distance: {_remainingDistance}");
    }

    private static GridPath ResolvePath(AbilityExecutionContext context)
    {
        var targetingContext = context.WorldLogic.TargetingContext;
        if (targetingContext != null && targetingContext.PreviewPath.PathIsValid)
        {
            var previewPath = targetingContext.PreviewPath;
            var sourceCoordinate = context.SourcePawn.OccupiedCell.Coordinate;
            var targetCoordinate = context.TargetCell.Coordinate;

            if (previewPath.Start == sourceCoordinate && previewPath.End == targetCoordinate)
                return previewPath;
        }

        return context.GridManager.FindPath(context.SourcePawn.OccupiedCell, context.TargetCell);
    }
}
