using System.Collections.Generic;
using System.Linq;
using Godot;
using TFGate2.scripts.logic;
using TFGate2.scripts.pawns.abilities;

namespace TFGate2.scripts.pawns;

/// <summary>
/// Represents a player-controlled pawn with abilities
/// </summary>
public partial class PlayerPawn : CharacterPawn
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
    
    public List<PawnAbility> GetAbilities(WorldLogic worldLogic)
    {
        if (_pawnAbilities.Count > 0)
            return _pawnAbilities;

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
            default:
                return false;
        }
    }
    
    public override string ToString()
    {
        var abilities = string.Join(", ", _pawnAbilities.Select(x => x.AbilityName));
        return $"[PAWN] {Name}, Team: {Team.ToString()}, Abilities: {abilities}";
    }
}