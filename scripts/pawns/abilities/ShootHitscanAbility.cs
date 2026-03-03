using Godot;
using TFGate2.scripts.pawns.abilities;

public partial class ShootHitscanAbility : PawnAbility
{
    [Export]
    public int MaxEffectiveRange { get; set; }

    [Export]
    public int MaxRange { get; set; }
    
    public override bool CanExecute(AbilityExecutionContext context)
    {
        if (!base.CanExecute(context)) 
            return false;

        var spaceState = GetWorld3D().DirectSpaceState;
        var source = Pawn.GetCenterMass();
        var target = context.TargetPawn.GetCenterMass();

        var rayResult = spaceState.IntersectRay(new PhysicsRayQueryParameters3D
        {
            From = source,
            To = target,
            CollideWithBodies = true,
        });

        return true;
    }

    public override void Execute(AbilityExecutionContext context)
    {
        base.Execute(context);
    }
}
