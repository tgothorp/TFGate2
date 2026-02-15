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
    private CurrentSelectable _currentSelectableEntities;

    public override void _Ready()
    {
        _currentSelectableEntities = CurrentSelectable.Nothing;

        var root = GetTree().CurrentScene;
        var gridManager = root.GetNode<GridManager>("WorldState/GridManager");
        if (gridManager == null)
        {
            GD.PrintErr("GridManager not found!");
            return;
        }
        
        _gridManager = gridManager;
    }
    
    private void UpdateSelectableEntities(CurrentSelectable entities)
    {
        _currentSelectableEntities = entities;
        switch (_currentSelectableEntities)
        {
            case CurrentSelectable.Grid:
                _gridManager.CanSelectGrid = true;
                break;
            case CurrentSelectable.Pawns:
                _gridManager.CanSelectGrid = false;
                break;
            case CurrentSelectable.Nothing:
                _gridManager.CanSelectGrid = false;
                break;
            case CurrentSelectable.GridAndPawns:
                _gridManager.CanSelectGrid = true;
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
