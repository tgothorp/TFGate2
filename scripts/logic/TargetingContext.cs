using TFGate2.scripts.grid;
using TFGate2.scripts.pawns.abilities;

namespace TFGate2.scripts.logic;

public class TargetingContext
{
    public bool IsActive { get; set; }

    public PawnAbility PawnAbility { get; set; }
    public GridPawn SourcePawn { get; set; }
    
    public GridCell HoveredCell { get; set; }
    public GridCell SelectedCell { get; set; }
    public GridPath PreviewPath { get; set; }

    public WorldLogic.SelectionState SelectionState { get; set; }

    public void Clear()
    {
        IsActive = false;
        PawnAbility = null;
        SourcePawn = null;
        HoveredCell = null;
        SelectedCell = null;
        PreviewPath = GridPath.Invalid;
        SelectionState = WorldLogic.SelectionState.AllPawns;
    }
}
