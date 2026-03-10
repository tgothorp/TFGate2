using System.Collections.Generic;
using System.Linq;
using Godot;
using TFGate2.scripts.logic;
using TFGate2.scripts.pawns.abilities;

namespace TFGate2.scripts.pawns;

/// <summary>
/// Represents a combat-capable pawn with abilities and turn resources.
/// </summary>
public partial class CombatPawn : CharacterPawn
{
    [Export]
    public CharacterClass Class { get; set; }

    [Export]
    public bool CanTakeAction { get; set; }
    public bool HasTakenAction { get; set; }

    [Export]
    public bool CanTakeBonusAction { get; set; }
    public bool HasTakenBonusAction { get; set; }

    [Export]
    public bool CanTakeReaction { get; set; }
    public bool HasTakenReaction { get; set; }
    
    private List<PawnAbility> _pawnAbilities = [];
    private bool _abilitiesInitialized;

    public override void _Ready()
    {
        base._Ready();
        InitializeAbilities();
    }

    public void InitializeAbilities()
    {
        if (_abilitiesInitialized)
            return;

        var worldLogic = GetTree().CurrentScene?.GetNodeOrNull<WorldLogic>("WorldManager");
        if (worldLogic == null)
        {
            GD.PrintErr($"[PAWN] Could not initialize abilities for {Name}: WorldManager not found.");
            return;
        }

        var abilities = GetNode("Abilities").GetChildren();
        foreach (var ability in abilities)
        {
            if (ability is PawnAbility abilityNode)
            {
                GD.Print($"[PAWN] {Name} has ability: " + abilityNode.Name);
                abilityNode.Register(worldLogic, this);

                _pawnAbilities.Add(abilityNode);
            }
        }
        
        _abilitiesInitialized = true;
    }

    public List<PawnAbility> GetAbilities()
    {
        return _pawnAbilities;
    }
    
    public bool CanPerformAbility(PawnAbility ability)
    {
        switch (ability.Cost)
        {
            case PawnAbility.AbilityCost.Free:
                return true;
            case PawnAbility.AbilityCost.Action:
                return !HasTakenAction;
            case PawnAbility.AbilityCost.BonusAction:
                return !HasTakenBonusAction;
            case PawnAbility.AbilityCost.Reaction:
                return !HasTakenReaction;
            case PawnAbility.AbilityCost.Special:
                //TODO
                return true;
            default:
                return false;
        }
    }

    public bool OwnsAbility(PawnAbility ability)
    {
        return ability != null && _pawnAbilities.Contains(ability);
    }

    public override void BeginTurn()
    {
        base.BeginTurn();
        ResetTurnResources();
    }

    public void ResetTurnResources()
    {
        HasTakenAction = false;
        HasTakenBonusAction = false;
        HasTakenReaction = false;
    }
    
    public override string ToString()
    {
        var abilities = string.Join(", ", _pawnAbilities.Select(x => x.AbilityName));
        return $"[PAWN] {Name}, Team: {Team.ToString()}, Abilities: {abilities}";
    }
}
