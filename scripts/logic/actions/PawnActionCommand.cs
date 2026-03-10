using TFGate2.scripts.grid;
using TFGate2.scripts.pawns.abilities;

namespace TFGate2.scripts.logic.actions;

/// <summary>
/// Explicit gameplay command describing a pawn action to execute.
/// </summary>
public readonly struct PawnActionCommand
{
    public PawnActionCommand(
        GridPawn sourcePawn,
        PawnAbility ability,
        GridPawn targetPawn = null,
        GridCell targetCell = null,
        GridPath confirmedPath = null)
    {
        SourcePawn = sourcePawn;
        Ability = ability;
        TargetPawn = targetPawn;
        TargetCell = targetCell;
        ConfirmedPath = confirmedPath ?? GridPath.Invalid;
    }

    public GridPawn SourcePawn { get; }
    public PawnAbility Ability { get; }
    public GridPawn TargetPawn { get; }
    public GridCell TargetCell { get; }
    public GridPath ConfirmedPath { get; }

    public bool IsValid => SourcePawn != null && Ability != null;
}
