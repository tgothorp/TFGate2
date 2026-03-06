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

        if (_pawnManager.SelectedPawn is PlayerPawn playerPawn)
        {
            DisplayDataForPawn(playerPawn);
            
            if (playerPawn.Team == _worldLogic.PlayerTeam)
            {
                DisplayAbilitiesForPawn(playerPawn);
            }
        }

        _shouldRedraw = false;
        base._Process(delta);
    }

    public void UpdatePawnData()
    {
        _shouldRedraw = true;
    }

    private void DisplayDataForPawn(PlayerPawn playerPawn)
    {
        var idLabel = new Label { Text = $"ID: {playerPawn.PawnId}" };
        var teamLabel = new Label { Text = $"Team: {playerPawn.Team.ToString()}" };
        var moveBudgetLabel = new Label { Text = $"Move Budget: {playerPawn.MoveBudget}" };
        var remainingMoveBudgetLabel = new Label { Text = $"Remaining Move Budget: {playerPawn.RemainingMoveBudget}" };
        var actionLabel = new Label { Text = $"Action: {playerPawn.CanTakeAction.ToString()}" };
        var bonusActionLabel = new Label { Text = $"Bonus Action: {playerPawn.CanTakeBonusAction.ToString()}" };
        var reactionLabel = new Label { Text = $"Reaction: {playerPawn.CanTakeReaction.ToString()}" };
        
        _dataContainer.AddChild(idLabel);
        _dataContainer.AddChild(teamLabel);
        _dataContainer.AddChild(moveBudgetLabel);
        _dataContainer.AddChild(remainingMoveBudgetLabel);
        _dataContainer.AddChild(actionLabel);
        _dataContainer.AddChild(bonusActionLabel);
        _dataContainer.AddChild(reactionLabel);
    }
    
    private void DisplayAbilitiesForPawn(PlayerPawn playerPawn)
    {
        var abilities = playerPawn.GetAbilities(null);
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
