using Godot;

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

    public PlayerPawn Pawn => _owner;
    
    private WorldLogic _worldLogic;
    private PlayerPawn _owner;
    
    public void Register(WorldLogic worldLogic, PlayerPawn owner)
    {
        _worldLogic = worldLogic;
        _owner = owner;
    }

    public virtual bool CanExecute(AbilityExecutionContext context)
    {
        //TODO: Top level check for pawn team targeting validity
        
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
}

