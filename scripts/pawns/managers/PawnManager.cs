using System;
using Godot;
using TFGate2.scripts.grid;
using TFGate2.scripts.pawns.abilities;
using TFGate2.scripts.pawns.managers;

public partial class PawnManager : Node3D
{
    public bool IsResolvingAbility { get; private set; }

    private GridPawn _selectedPawn;
    private PawnAbility _selectedAbility;
    
    private PawnAbilityUiController _abilityUiController;
    private WorldLogic _worldLogic;

    public override void _Ready()
    {
        _abilityUiController = GetNode<PawnAbilityUiController>("PawnAbilityManager");
        if (_abilityUiController == null)
        {
            GD.PrintErr("PawnAbilityManager not found!");
        }

        _worldLogic = GetParent<WorldLogic>();
        if (_worldLogic == null)
        {
            GD.PrintErr("WorldLogic not found!");
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed && IsResolvingAbility)
            {
                GD.Print("Cancel ability resolution");

                IsResolvingAbility = false;
                _selectedAbility = null;
                _worldLogic.UpdateSelectionState(WorldLogic.SelectionState.AllPawns);
            }
        }

        base._UnhandledInput(@event);
    }

    public void SelectPawn(GridPawn pawn)
    {
        if (!CanSelectPawns())
            return;
        
        if (IsResolvingAbility)
        {
            // We have selected a pawn as part of an action resolution
            ResolveAbility(pawn, null);
            return;
        }
        
        _selectedPawn = pawn;
        _abilityUiController.DisplayAbilitiesForPawn(pawn);

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
        GD.Print($"Resolving ability '{_selectedAbility.AbilityName}' for {_selectedPawn.Name}, (pawn = {pawn}, targetCell = {targetCell})");
        
        if (!_selectedAbility.CanExecute(pawn, targetCell))
        {
            GD.PrintErr($"Ability '{_selectedAbility.AbilityName}' cannot be executed for {_selectedPawn.Name}");
            return;
        }
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
}