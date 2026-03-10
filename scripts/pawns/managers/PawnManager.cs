using System;
using System.Collections.Generic;
using Godot;
using TFGate2.scripts.pawns.abilities;
using TFGate2.scripts.pawns.managers;

/// <summary>
/// Owns pawn registry and player-facing selection state.
/// </summary>
public partial class PawnManager : Node3D
{
    [Signal]
    public delegate void PawnSelectedEventHandler(GridPawn pawn);
    
    [Signal]
    public delegate void AbilitySelectedEventHandler(PawnAbility ability);
    
    public GridPawn SelectedPawn => _selectedPawn;
    public PawnAbility SelectedAbility => _selectedAbility;
    public Dictionary<Guid, GridPawn> RegisteredPawns => _registeredPawns;

    private GridPawn _selectedPawn;
    private PawnAbility _selectedAbility;

    private PawnUiController _uiController;
    private WorldLogic _worldLogic;
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
        
        if (_worldLogic.PlayerTargetingContext is not { HasActiveAbility: true })
            _selectedPawn = pawn;

        EmitSignal(SignalName.PawnSelected, pawn);
    }

    public void SelectAbility(PawnAbility ability)
    {
        _selectedAbility = ability;

        EmitSignal(SignalName.AbilitySelected, ability);
    }

    public void DeselectAbility()
    {
        GD.Print("Deselecting ability");
        _selectedAbility = null;

        EmitSignal(SignalName.AbilitySelected, null);
    }

    public void DeselectPawn()
    {
        GD.Print("Deselecting pawn");
        _selectedPawn = null;
        _selectedAbility = null;
        _uiController.UpdatePawnData();
    }

    public void RefreshUi()
    {
        _uiController?.UpdatePawnData();
    }
}
