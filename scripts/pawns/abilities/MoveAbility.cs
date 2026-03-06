using Godot;
using TFGate2.scripts.grid;

namespace TFGate2.scripts.pawns.abilities;

public partial class MoveAbility : PawnAbility
{
    private GridPath _path;
    private AbilityExecutionContext _context;

    public override bool CanExecute(AbilityExecutionContext context)
    {
        if (context.SourcePawn == null || context.TargetCell == null)
        {
            GD.PrintErr("Invalid ability execution context!");
            return false;
        }

        if (context.SourcePawn is not MoveablePawn pawn)
        {
            GD.PrintErr("Move ability was called on a non-moveable pawn!");
            return false;
        }

        if (pawn.OccupiedCell == null || pawn is { MoveBudget: <= 0 })
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

        if (_path.Cost > pawn.RemainingMoveBudget)
        {
            GD.PrintErr("Path is too expensive!");
            return false;
        }

        return base.CanExecute(context);
    }

    public override void Execute(AbilityExecutionContext context)
    {
        var pawn = context.SourcePawn as MoveablePawn;
        if (!_path.PathIsValid || _path.Cost > pawn!.MoveBudget || pawn!.MoveBudget <= 0)
        {
            GD.PushWarning($"Pawn move state is not valid ({_path})");
            return;
        }
        
        GD.Print($"[ABILITY] Move executed by {context.SourcePawn.Name}. Path: {_path}");

        if (pawn.Mover.TryGridMove(_path))
        {
            _context = context;
            pawn.Mover.MoveFinished += OnMoveFinished;
            pawn!.UpdateMoveBudget(-_path.Cost);
        }
    }

    private void OnMoveFinished(MoveablePawn pawn)
    {
        pawn.Mover.MoveFinished -= OnMoveFinished;
        var newCell = _context.GridManager.MovePawn(pawn, _path.End);
        pawn.SetOccupiedCell(newCell);

        EmitSignal(PawnAbility.SignalName.AbilityFinished);
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
