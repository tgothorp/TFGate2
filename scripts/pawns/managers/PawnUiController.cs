using Godot;
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

        DisplayDataForPawn();

        if (_pawnManager.SelectedPawn.Team == _worldLogic.PlayerTeam)
        {
            DisplayAbilitiesForPawn();
        }
        
        _shouldRedraw = false;
        base._Process(delta);
    }

    public void UpdatePawnData()
    {
        _shouldRedraw = true;
    }

    private void DisplayDataForPawn()
    {
        var idLabel = new Label { Text = $"ID: {_pawnManager.SelectedPawn.PawnId}" };
        var teamLabel = new Label { Text = $"Team: {_pawnManager.SelectedPawn.Team.ToString()}" };
        var moveBudgetLabel = new Label { Text = $"Move Budget: {_pawnManager.SelectedPawn.MoveBudget}" };
        var remainingMoveBudgetLabel = new Label { Text = $"Remaining Move Budget: {_pawnManager.SelectedPawn.RemainingMoveBudget}" };
        var actionLabel = new Label { Text = $"Action: {_pawnManager.SelectedPawn.CanTakeAction.ToString()}" };
        var bonusActionLabel = new Label { Text = $"Bonus Action: {_pawnManager.SelectedPawn.CanTakeBonusAction.ToString()}" };
        var reactionLabel = new Label { Text = $"Reaction: {_pawnManager.SelectedPawn.CanTakeReaction.ToString()}" };
        
        _dataContainer.AddChild(idLabel);
        _dataContainer.AddChild(teamLabel);
        _dataContainer.AddChild(moveBudgetLabel);
        _dataContainer.AddChild(remainingMoveBudgetLabel);
        _dataContainer.AddChild(actionLabel);
        _dataContainer.AddChild(bonusActionLabel);
        _dataContainer.AddChild(reactionLabel);
    }
    
    private void DisplayAbilitiesForPawn()
    {
        var abilities = _pawnManager.SelectedPawn.GetAbilities(null);
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

        GD.Print($"[UI] Ability selected - Pawn: {pawn.Name}, Ability: {ability.AbilityName}, Index: {abilityIndex}");
        _pawnManager.SelectAbility(ability);
    }
}
