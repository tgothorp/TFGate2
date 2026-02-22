using System;
using System.Collections.Generic;
using Godot;
using TFGate2.scripts.grid;
using TFGate2.scripts.pawns.abilities;
using TFGate2.scripts.pawns.managers;

/// <summary>
/// Owns pawn/ability selection state and orchestrates ability resolution flow.
/// </summary>
public partial class PawnManager : Node3D
{
    public bool IsResolvingAbility { get; private set; }
    public GridPawn SelectedPawn => _selectedPawn;
    public Dictionary<Guid, GridPawn> RegisteredPawns => _registeredPawns;

    private GridPawn _selectedPawn;
    private PawnAbility _selectedAbility;

    private PawnUiController _uiController;
    private WorldLogic _worldLogic;
    private GridManager _gridManager;
    private Dictionary<Guid, GridPawn> _registeredPawns = new();

    public override void _Ready()
    {
        _uiController = GetNode<PawnUiController>("PawnUiManager");
        if (_uiController == null)
        {
            GD.PrintErr("PawnUiManager not found!");
        }

        _worldLogic = GetParent<WorldLogic>();
        if (_worldLogic == null)
        {
            GD.PrintErr("WorldLogic not found!");
        }

        _gridManager = GetParent<WorldLogic>()?.GetNode<GridManager>("GridManager");
        if (_gridManager == null)
        {
            GD.PrintErr("GridManager not found!");
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
            {
                if (IsResolvingAbility)
                    DeselectAbility();
                else
                    DeselectPawn();
            }
        }

        base._UnhandledInput(@event);
    }

    public Guid RegisterPawn(GridPawn pawn)
    {
        var pawnId = Guid.NewGuid();
        _registeredPawns.Add(pawnId, pawn);

        return pawnId;
    }

    public void SelectPawn(GridPawn pawn)
    {
        if (!CanSelectPawns())
            return;

        _uiController.UpdatePawnData();
        
        if (IsResolvingAbility)
        {
            // We have selected a pawn as part of an action resolution
            ResolveAbility(pawn, null);
            return;
        }

        _selectedPawn = pawn;
        GD.Print($"Selected GridPawn: {pawn}");
    }

    public void SelectAbility(PawnAbility ability)
    {
        _selectedAbility = ability;
        _worldLogic.UpdateSelectionState(_selectedAbility);

        IsResolvingAbility = true;
    }

    public void ResolveAbility(GridPawn pawn, GridCell targetCell)
    {
        if (_selectedAbility == null || _selectedPawn == null || _worldLogic == null || _gridManager == null)
            return;

        GD.Print($"Resolving ability '{_selectedAbility.AbilityName}' for {_selectedPawn.Name}, (pawn = {pawn}, targetCell = {targetCell})");

        var context = new AbilityExecutionContext(
            _worldLogic,
            this,
            _gridManager,
            _selectedPawn,
            pawn,
            targetCell);

        if (!_selectedAbility.CanExecute(context))
        {
            GD.PrintErr($"Ability '{_selectedAbility.AbilityName}' cannot be executed for {_selectedPawn.Name}");
            return;
        }

        _selectedAbility.Execute(context);
        _uiController.UpdatePawnData();

        DeselectAbility();
    }

    private bool CanSelectPawns()
    {
        return _worldLogic.CurrentSelectionState switch
        {
            WorldLogic.SelectionState.Grid or WorldLogic.SelectionState.Nothing => false,
            WorldLogic.SelectionState.EnemyPawns or WorldLogic.SelectionState.TeamPawns or WorldLogic.SelectionState.AllPawns => true,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void DeselectAbility()
    {
        GD.Print("Deselecting ability");
        IsResolvingAbility = false;
        _selectedAbility = null;
        _worldLogic.UpdateSelectionState(WorldLogic.SelectionState.AllPawns);
    }

    private void DeselectPawn()
    {
        GD.Print("Deselecting pawn");
        IsResolvingAbility = false;
        _selectedPawn = null;
        _selectedAbility = null;
        _uiController.UpdatePawnData();
        _worldLogic.UpdateSelectionState(WorldLogic.SelectionState.AllPawns);
    }
}