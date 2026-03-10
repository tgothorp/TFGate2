using Godot;
using TFGate2.scripts.pawns;
using TFGate2.scripts.pawns.abilities;

namespace TFGate2.scripts.pawns.managers;

/// <summary>
/// Renders the selected pawn's ability list and forwards UI selection to PawnManager.
/// </summary>
public partial class PawnUiController : Control
{
    private WorldLogic _worldLogic;
    private PawnManager _pawnManager;
    private VBoxContainer _abilitiesContainer;
    private VBoxContainer _dataContainer;
    
    private bool _shouldRedraw = true;

    public override void _Ready()
    {
        _pawnManager = GetParent<PawnManager>();
        if (_pawnManager == null)
        {
            GD.PrintErr("PawnManager not found!");
        }
        
        _worldLogic = _pawnManager?.GetParent<WorldLogic>();
        if (_worldLogic == null)
        {
            GD.PrintErr("WorldLogic not found!");
        }
        
        _abilitiesContainer = GetNode<VBoxContainer>("AbilitiesContainer");
        if (_abilitiesContainer == null)
        {
            GD.PrintErr("AbilitiesContainer not found!");
        }
        
        _dataContainer = GetNode<VBoxContainer>("DataContainer");
        if (_dataContainer == null)
        {
            GD.PrintErr("DataContainer not found!");
        }
    }

    public override void _Process(double delta)
    {
        if (!_shouldRedraw)
            return;

        if (_pawnManager.SelectedPawn == null)
        {
            Visible = false;
            return;
        }
        
        Visible = true;
        foreach (var child in _dataContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        foreach (var child in _abilitiesContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (_pawnManager.SelectedPawn is CombatPawn combatPawn)
        {
            DisplayDataForPawn(combatPawn);
            
            if (combatPawn.Team == _worldLogic.PlayerTeam)
            {
                DisplayAbilitiesForPawn(combatPawn);
            }
        }

        _shouldRedraw = false;
        base._Process(delta);
    }

    public void UpdatePawnData()
    {
        _shouldRedraw = true;
    }

    private void DisplayDataForPawn(CombatPawn combatPawn)
    {
        var idLabel = new Label { Text = $"ID: {combatPawn.PawnId}" };
        var teamLabel = new Label { Text = $"Team: {combatPawn.Team.ToString()}" };
        var hpLabel = new Label { Text = $"HP: {combatPawn.CurrentHitPoints.ToString()} / {combatPawn.HitPoints.ToString()}" };
        var moveBudgetLabel = new Label { Text = $"Move Budget: {combatPawn.MoveBudget}" };
        var remainingMoveBudgetLabel = new Label { Text = $"Remaining Move Budget: {combatPawn.RemainingMoveBudget}" };
        var actionLabel = new Label { Text = $"Has Taken Action: {combatPawn.HasTakenAction.ToString()}" };
        var bonusActionLabel = new Label { Text = $"Has Taken Bonus Action: {combatPawn.HasTakenBonusAction.ToString()}" };
        var reactionLabel = new Label { Text = $"Has Taken Reaction: {combatPawn.HasTakenReaction.ToString()}" };
        
        _dataContainer.AddChild(idLabel);
        _dataContainer.AddChild(teamLabel);
        _dataContainer.AddChild(hpLabel);
        _dataContainer.AddChild(moveBudgetLabel);
        _dataContainer.AddChild(remainingMoveBudgetLabel);
        _dataContainer.AddChild(actionLabel);
        _dataContainer.AddChild(bonusActionLabel);
        _dataContainer.AddChild(reactionLabel);
    }
    
    private void DisplayAbilitiesForPawn(CombatPawn combatPawn)
    {
        var abilities = combatPawn.GetAbilities();
        for (var i = 0; i < abilities.Count; i++)
        {
            var ability = abilities[i];
            var abilityIndex = i;
            var abilityButton = new Button
            {
                Text = string.IsNullOrWhiteSpace(ability.AbilityName) ? ability.Name : ability.AbilityName,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };

            abilityButton.Pressed += () => OnAbilityButtonPressed(_pawnManager.SelectedPawn, ability, abilityIndex);
            _abilitiesContainer.AddChild(abilityButton);
        }
    }

    private void OnAbilityButtonPressed(GridPawn pawn, PawnAbility ability, int abilityIndex)
    {
        if (pawn == null || ability == null)
            return;

        GD.Print($"[PAWN-UI] Ability selected - Pawn: {pawn.Name}, Ability: {ability.AbilityName}, Index: {abilityIndex}");
        _pawnManager.SelectAbility(ability);
    }
}
