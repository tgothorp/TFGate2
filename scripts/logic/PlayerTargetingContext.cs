using System;
using TFGate2.scripts.pawns.abilities;

namespace TFGate2.scripts.logic;

/// <summary>
/// Represents the player's in-progress targeting workflow.
/// </summary>
public class PlayerTargetingContext
{
    public PlayerTargetingContext()
    {
        Clear();
    }

    public bool HasActiveAbility => SelectedAbility != null;
    public PawnAbility SelectedAbility { get; private set; }
    public GridPawn SourcePawn { get; private set; }

    public SelectionState ActiveSelectionState { get; set; }

    public bool CanSelectGrid => ActiveSelectionState == SelectionState.Grid;
    public bool CanSelectPawns => ActiveSelectionState is SelectionState.AllPawns or SelectionState.EnemyPawns or SelectionState.TeamPawns;

    public void Clear()
    {
        SelectedAbility = null;
        SourcePawn = null;
        ActiveSelectionState = SelectionState.AllPawns;
    }

    public void PawnSelected(GridPawn pawn)
    {
        SourcePawn = pawn;
    }

    public void AbilitySelected(PawnAbility ability)
    {
        SelectedAbility = ability;

        if (ability == null)
        {
            ActiveSelectionState = SelectionState.AllPawns;
            return;
        }

        switch (ability.Target)
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
    
    public void BeginExecutionLock()
    {
        ActiveSelectionState = SelectionState.Nothing;
    }

    public void CompleteAbilitySelection()
    {
        SelectedAbility = null;
        ActiveSelectionState = SelectionState.AllPawns;
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
