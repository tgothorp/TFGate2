using TFGate2.scripts.grid;

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
        GridPawn sourcePawn,
        GridPawn targetPawn,
        GridCell targetCell)
    {
        WorldLogic = worldLogic;
        PawnManager = pawnManager;
        GridManager = gridManager;
        SourcePawn = sourcePawn;
        TargetPawn = targetPawn;
        TargetCell = targetCell;
    }

    public WorldLogic WorldLogic { get; }
    public PawnManager PawnManager { get; }
    public GridManager GridManager { get; }
    public GridPawn SourcePawn { get; }
    public GridPawn TargetPawn { get; }
    public GridCell TargetCell { get; }
}
