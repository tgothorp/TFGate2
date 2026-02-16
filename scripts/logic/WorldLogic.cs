using Godot;
using System;
using TFGate2.scripts.grid;

public partial class WorldLogic : Node3D
{
    [Export]
    public Team CurrentTeamTurn { get; set; } = Team.World;

    private GridManager _gridManager;
    private PawnManager _pawnManager;
    private SelectionState _currentSelectionState = SelectionState.AllPawns;

    public override void _Ready()
    {
        var root = GetTree().CurrentScene;
        var gridManager = root.GetNode<GridManager>("WorldState/GridManager");
        if (gridManager == null)
        {
            GD.PrintErr("GridManager not found!");
            return;
        }

        var pawnManager = root.GetNode<PawnManager>("WorldState/PawnManager");
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
        _currentSelectionState = newState;
        switch (_currentSelectionState)
        {
            case SelectionState.Nothing:
                _pawnManager.SelectionMode = PawnSelectionMode.Off;
                _gridManager.CanSelectGrid = false;
                break;
            case SelectionState.BluePawns:
                _pawnManager.SelectionMode = PawnSelectionMode.TeamBlue;
                _gridManager.CanSelectGrid = false;
                break;
            case SelectionState.RedPawns:
                _pawnManager.SelectionMode = PawnSelectionMode.TeamRed;
                _gridManager.CanSelectGrid = false;
                break;
            case SelectionState.WorldPawns:
                _pawnManager.SelectionMode = PawnSelectionMode.NonTeamPawns;
                _gridManager.CanSelectGrid = false;
                break;
            case SelectionState.AllPawns:
                _pawnManager.SelectionMode = PawnSelectionMode.All;
                _gridManager.CanSelectGrid = false;
                break;
            case SelectionState.Grid:
                _pawnManager.SelectionMode = PawnSelectionMode.Off;
                _gridManager.CanSelectGrid = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public enum SelectionState
    {
        // Nothing in the world can be selected
        Nothing,
        
        // Only pawns can be selected
        BluePawns,
        RedPawns,
        WorldPawns,
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