using System;
using TFGate2.scripts.grid;
using TFGate2.scripts.pawns.abilities;

namespace TFGate2.scripts.logic;

/// <summary>
/// Represents what is currently selected and what can be selected
/// </summary>
public class SelectionContext
{
    public SelectionContext()
    {
        Clear();
    }

    public bool AbilityBeingResolved { get; private set; }

    public PawnAbility PawnAbility { get; private set; }
    public GridPawn SourcePawn { get; private set; }

    public GridCell SelectedCell { get; private set; }
    public GridPath ConfirmedPath { get; private set; }

    public SelectionState ActiveSelectionState { get; set; }

    public bool CanSelectGrid => ActiveSelectionState == SelectionState.Grid;
    public bool CanSelectPawns => ActiveSelectionState is SelectionState.AllPawns or SelectionState.EnemyPawns or SelectionState.TeamPawns;

    public void Clear()
    {
        PawnAbility = null;
        SourcePawn = null;
        SelectedCell = null;
        ConfirmedPath = GridPath.Invalid;
        AbilityBeingResolved = false;
        ActiveSelectionState = SelectionState.AllPawns;
    }

    public void GridCellSelected(GridCell cell)
    {
        SelectedCell = cell;
    }

    public void ConfirmPath(GridPath path)
    {
        ConfirmedPath = path ?? GridPath.Invalid;
    }

    public void PawnSelected(GridPawn pawn)
    {
        SourcePawn = pawn;
    }

    public void AbilitySelected(PawnAbility pawnAbility)
    {
        AbilityBeingResolved = pawnAbility != null;
        PawnAbility = pawnAbility;
        ConfirmedPath = GridPath.Invalid;

        if (pawnAbility == null)
            return;

        switch (pawnAbility.Target)
        {
            case PawnAbility.AbilityTarget.Grid:
                ActiveSelectionState = SelectionState.Grid;
                break;
            case PawnAbility.AbilityTarget.Self:
                ActiveSelectionState = SelectionState.AllPawns;
                break;
            case PawnAbility.AbilityTarget.Team:
                ActiveSelectionState = SelectionState.TeamPawns;
                break;
            case PawnAbility.AbilityTarget.Opponent:
                ActiveSelectionState = SelectionState.EnemyPawns;
                break;
            case PawnAbility.AbilityTarget.All:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void UpdateSelectionState(SelectionState newState)
    {
        ActiveSelectionState = newState;
    }
    
    public void DisableSelection()
    {
        ActiveSelectionState = SelectionState.Nothing;
    }

    public enum SelectionState
    {
        // Nothing in the world can be selected
        Nothing,

        // Only pawns can be selected
        EnemyPawns,
        TeamPawns,
        AllPawns,

        // Only the grid can be selected
        Grid,
    }
}
