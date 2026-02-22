using Godot;
using System;
using TFGate2.scripts.grid;
using TFGate2.scripts.pawns.abilities;

/// <summary>
/// Coordinates high-level combat state such as turn context and world selection mode.
/// </summary>
public partial class WorldLogic : Node3D
{
    [Export]
    public Team PlayerTeam { get; set; } = Team.Red;
    
    [Export]
    public Team CurrentTeamTurn { get; set; } = Team.Red;
    
    [Export]
    public SelectionState CurrentSelectionState { get; set; } = SelectionState.AllPawns;

    private GridManager _gridManager;
    private PawnManager _pawnManager;

    public override void _Ready()
    {
        var gridManager = GetNode<GridManager>("GridManager");
        if (gridManager == null)
        {
            GD.PrintErr("GridManager not found!");
            return;
        }

        var pawnManager = GetNode<PawnManager>("PawnManager");
        if (pawnManager == null)
        {
            GD.PrintErr("PawnManager not found!");
            return;
        }

        _gridManager = gridManager;
        _pawnManager = pawnManager;
        
        UpdateSelectionState(SelectionState.AllPawns);
    }

    public void UpdateSelectionState(SelectionState newState)
    {
        CurrentSelectionState = newState;
        switch (CurrentSelectionState)
        {
            case SelectionState.Nothing:
                _gridManager.CanSelectGrid = false;
                break;
            case SelectionState.EnemyPawns:
                _gridManager.CanSelectGrid = false;
                break;
            case SelectionState.TeamPawns:
                _gridManager.CanSelectGrid = false;
                break;
            case SelectionState.AllPawns:
                _gridManager.CanSelectGrid = false;
                break;
            case SelectionState.Grid:
                _gridManager.CanSelectGrid = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void UpdateSelectionState(PawnAbility ability)
    {
        UpdateSelectionState(ability.Target);
    }

    
    public void GridCellSelected(GridCell cell)
    {
        if (_pawnManager.IsResolvingAbility)
            _pawnManager.ResolveAbility(null, cell);
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
    
    public enum Team
    {
        World,
        Blue,
        Red,
    }
}
