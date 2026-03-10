using Godot;
using TFGate2.scripts.pawns;

namespace TFGate2.scripts.pawns.abilities;

public abstract partial class PawnAbility : Node3D
{
    [Export]
    public string AbilityName { get; set; }
    
    [Export]
    public string AbilityDescription { get; set; }

    [Export]
    public AbilityTarget Target { get; set; }

    [Export]
    public AbilityCost Cost { get; set; }

    [Export(hintString:"Can this ability target the owner?")]
    public bool CanTargetSelf { get; set; }

    [Export(hintString:"Can this ability ONLY target the owner?")]
    public bool CanOnlyTargetSelf { get; set; }

    [Signal]
    public delegate void AbilityFinishedEventHandler();

    public CombatPawn Pawn => _owner;
    
    private WorldLogic _worldLogic;
    private CombatPawn _owner;
    
    public void Register(WorldLogic worldLogic, CombatPawn owner)
    {
        _worldLogic = worldLogic;
        _owner = owner;
    }

    public virtual bool CanExecute(AbilityExecutionContext context)
    {
        if (context.SourcePawn == null || context.Command.Ability != this)
            return false;

        if (context.SourcePawn != Pawn)
            return false;

        if (!ValidateTargeting(context))
            return false;
        
        return Pawn.CanPerformAbility(this);
    }

    public void BeginExecute(AbilityExecutionContext context)
    {
        Execute(context);
    }
    
    public virtual void Execute(AbilityExecutionContext context)
    {
        switch (Cost)
        {
            case AbilityCost.Action:
                Pawn.HasTakenAction = true;
                break;
            case AbilityCost.BonusAction:
                Pawn.HasTakenBonusAction = true;
                break;
            case AbilityCost.Reaction:
                Pawn.HasTakenReaction = true;
                break;
            case AbilityCost.Free:
            case AbilityCost.Special:
            default:
                return;
        }
    }

    public enum AbilityCost
    {
        Free,
        Action,
        BonusAction,
        Reaction,
        Special,
    }
    
    public enum AbilityTarget
    {
        Grid,
        Self,
        Team,
        Opponent,
        All,
    }

    private bool ValidateTargeting(AbilityExecutionContext context)
    {
        switch (Target)
        {
            case AbilityTarget.Grid:
                return context.TargetCell != null;
            case AbilityTarget.Self:
                return context.TargetPawn == context.SourcePawn || (CanOnlyTargetSelf && context.TargetPawn == null);
            case AbilityTarget.Team:
                return ValidatePawnTarget(context, sameTeam: true);
            case AbilityTarget.Opponent:
                return ValidatePawnTarget(context, sameTeam: false);
            case AbilityTarget.All:
                return context.TargetPawn != null || context.TargetCell != null || context.SourcePawn != null;
            default:
                return false;
        }
    }

    private bool ValidatePawnTarget(AbilityExecutionContext context, bool sameTeam)
    {
        if (context.TargetPawn is not CharacterPawn targetCharacter || context.SourcePawn is not CharacterPawn sourceCharacter)
            return false;

        if (CanOnlyTargetSelf)
            return ReferenceEquals(context.TargetPawn, context.SourcePawn);

        if (!CanTargetSelf && ReferenceEquals(context.TargetPawn, context.SourcePawn))
            return false;

        return sameTeam
            ? targetCharacter.Team == sourceCharacter.Team
            : targetCharacter.Team != sourceCharacter.Team;
    }
}
