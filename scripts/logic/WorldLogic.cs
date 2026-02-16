using Godot;
using System;
using TFGate2.scripts.grid;

public partial class WorldLogic : Node3D
{
    [Export]
    public CurrentSelectable CurrentSelectableEntities
    {
        get => _currentSelectableEntities;
        set => UpdateSelectableEntities(value);
    }

    private GridManager _gridManager;
    private PawnManager _pawnManager;
    private CurrentSelectable _currentSelectableEntities;

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

        CurrentSelectableEntities = CurrentSelectable.Nothing;
    }

    private void UpdateSelectableEntities(CurrentSelectable entities)
    {
        if (_gridManager == null || _pawnManager == null)
            return;

        _currentSelectableEntities = entities;
        switch (_currentSelectableEntities)
        {
            case CurrentSelectable.Grid:
                _gridManager.CanSelectGrid = true;
                _pawnManager.CanSelectPawns = false;
                break;
            case CurrentSelectable.Pawns:
                _gridManager.CanSelectGrid = false;
                _pawnManager.CanSelectPawns = true;
                break;
            case CurrentSelectable.Nothing:
                _gridManager.CanSelectGrid = false;
                _pawnManager.CanSelectPawns = false;
                break;
            case CurrentSelectable.GridAndPawns:
                _gridManager.CanSelectGrid = true;
                _pawnManager.CanSelectPawns = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public enum CurrentSelectable
    {
        Nothing,
        Grid,
        Pawns,
        GridAndPawns,
    }
}