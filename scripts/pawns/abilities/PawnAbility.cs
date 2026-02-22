using Godot;
using TFGate2.scripts.grid;

namespace TFGate2.scripts.pawns.abilities;

public abstract partial class PawnAbility : Node3D
{
    [Export]
    public string AbilityName { get; set; }

    [Export]
    public WorldLogic.SelectionState Target { get; set; }

    [Export]
    public AbilityCost Cost { get; set; }

    [Export(hintString:"Can this ability target the owner?")]
    public bool CanTargetSelf { get; set; }

    [Export(hintString:"Can this ability ONLY target the owner?")]
    public bool CanOnlyTargetSelf { get; set; }

    public GridPawn Pawn => _owner;
    
    private WorldLogic _worldLogic;
    private GridPawn _owner;
    
    public void Register(WorldLogic worldLogic, GridPawn owner)
    {
        _worldLogic = worldLogic;
        _owner = owner;
    }

    public abstract bool CanExecute(AbilityExecutionContext context);
    public abstract void Execute(AbilityExecutionContext context);

    public enum AbilityCost
    {
        Free,
        Action,
        BonusAction,
        Reaction,
        Special,
    }
}

