using Godot;
using TFGate2.scripts.pawns;
using TFGate2.scripts.pawns.abilities;

public partial class ShootHitscanAbility : PawnAbility
{
    [Export]
    public int MaxEffectiveRange { get; set; }

    [Export]
    public int MaxRange { get; set; }

    [Export]
    public uint Damage { get; set; }

    public override bool CanExecute(AbilityExecutionContext context)
    {
        if (!base.CanExecute(context))
            return false;

        if (context.TargetPawn == null)
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

        if (rayResult.Count == 0)
            return false;

        var collider = rayResult["collider"].AsGodotObject();

        if (collider == context.TargetPawn)
            return true;

        return collider is Node node && node.IsAncestorOf(context.TargetPawn);
    }

    public override void Execute(AbilityExecutionContext context)
    {
        if (context.TargetPawn is CharacterPawn characterPawn)
        {
            characterPawn.TakeDamage(Damage);
        }

        base.Execute(context);

        EmitSignal(PawnAbility.SignalName.AbilityFinished);
    }
}
