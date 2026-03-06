using System;
using System.Collections.Generic;
using Godot;
using TFGate2.scripts.grid;
using TFGate2.scripts.pawns;
using TFGate2.scripts.pawns.abilities;
using TFGate2.scripts.pawns.managers;

/// <summary>
/// Owns pawn/ability selection state and orchestrates ability resolution flow.
/// </summary>
public partial class PawnManager : Node3D
{
    public bool IsResolvingAbility => _worldLogic?.TargetingContext?.IsActive ?? false;
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

    public Guid RegisterPawn(GridPawn pawn)
    {
        var pawnId = Guid.NewGuid();
        _registeredPawns.Add(pawnId, pawn);

        return pawnId;
    }

    public void SelectPawn(GridPawn pawn)
    {
        _uiController.UpdatePawnData();
        _selectedPawn = pawn;

        GD.Print($"Selected GridPawn: {pawn}");
    }

    public void SelectAbility(PawnAbility ability)
    {
        _selectedAbility = ability;
        _worldLogic.UpdateTargetingContext(_selectedAbility);
    }

    public void ResolveAbility(GridPawn targetPawn, GridCell targetCell)
    {
        if (_selectedAbility == null || _selectedPawn == null || _worldLogic == null || _gridManager == null)
            return;

        GD.Print($"Resolving ability '{_selectedAbility.AbilityName}' for {_selectedPawn}, (pawn = {targetPawn}, targetCell = {targetCell})");

        var context = new AbilityExecutionContext(
            _worldLogic,
            this,
            _gridManager,
            _selectedPawn,
            targetPawn,
            targetCell);

        if (!_selectedAbility.CanExecute(context))
        {
            GD.PrintErr($"Ability '{_selectedAbility.AbilityName}' cannot be executed for {_selectedPawn.Name}");
            return;
        }

        _selectedAbility.AbilityFinished += OnAbilityResolved;
        _selectedAbility.BeginExecute(context);
        _uiController.UpdatePawnData();
    }

    private void OnAbilityResolved()
    {
        _selectedAbility.AbilityFinished -= OnAbilityResolved;
        DeselectAbility();
        DeselectPawn();
    }

    private void DeselectAbility()
    {
        GD.Print("Deselecting ability");
        _selectedAbility = null;
        _worldLogic.ClearTargetingContext();
    }

    private void DeselectPawn()
    {
        GD.Print("Deselecting pawn");
        _selectedPawn = null;
        _selectedAbility = null;
        _uiController.UpdatePawnData();
        _worldLogic.ClearTargetingContext();
    }
}
