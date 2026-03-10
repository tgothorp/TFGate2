using TFGate2.scripts.grid;
using TFGate2.scripts.logic.actions;

namespace TFGate2.scripts.pawns.abilities;

/// <summary>
/// Runtime context passed into abilities so they can resolve gameplay
/// without reaching into the scene tree.
/// </summary>
public readonly struct AbilityExecutionContext
{
    public AbilityExecutionContext(
        WorldLogic worldLogic,
        PawnManager pawnManager,
        GridManager gridManager,
        PawnActionCommand command)
    {
        WorldLogic = worldLogic;
        PawnManager = pawnManager;
        GridManager = gridManager;
        Command = command;
    }

    public WorldLogic WorldLogic { get; }
    public PawnManager PawnManager { get; }
    public GridManager GridManager { get; }
    public PawnActionCommand Command { get; }
    public GridPawn SourcePawn => Command.SourcePawn;
    public GridPawn TargetPawn => Command.TargetPawn;
    public GridCell TargetCell => Command.TargetCell;
    public GridPath ConfirmedPath => Command.ConfirmedPath;
}
