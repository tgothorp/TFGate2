using Godot;
using TFGate2.scripts.pawns.abilities;

namespace TFGate2.scripts.pawns.managers;

/// <summary>
/// Renders the selected pawn's ability list and forwards UI selection to PawnManager.
/// </summary>
public partial class PawnAbilityUiController : Control
{
    private PawnManager _pawnManager;
    private VBoxContainer _abilitiesContainer;
    
    private GridPawn _selectedPawn;

    public override void _Ready()
    {
        _pawnManager = GetParent<PawnManager>();
        if (_pawnManager == null)
        {
            GD.PrintErr("PawnManager not found!");
        }
        
        _abilitiesContainer = GetNode<VBoxContainer>("AbilitiesContainer");
        if (_abilitiesContainer == null)
        {
            GD.PrintErr("AbilitiesContainer not found!");
        }
    }
    
    public void DisplayAbilitiesForPawn(GridPawn pawn)
    {
        if (_selectedPawn == pawn) 
            return;

        foreach (var child in _abilitiesContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (pawn == null)
        {
            _selectedPawn = null;
            Visible = false;
            return;
        }

        _selectedPawn = pawn;
        Visible = true;

        var abilities = pawn.GetAbilities(null);
        for (var i = 0; i < abilities.Count; i++)
        {
            var ability = abilities[i];
            var abilityIndex = i;
            var abilityButton = new Button
            {
                Text = string.IsNullOrWhiteSpace(ability.AbilityName) ? ability.Name : ability.AbilityName,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };

            abilityButton.Pressed += () => OnAbilityButtonPressed(_selectedPawn, ability, abilityIndex);
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
