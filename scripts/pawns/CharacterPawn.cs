using Godot;

namespace TFGate2.scripts.pawns;

/// <summary>
/// Generic player / NPC pawn that can die / take damage.
/// This is the top most pawn that can be selected.
/// </summary>
public partial class CharacterPawn : MoveablePawn
{
    [Export]
    public uint HitPoints { get; set; }
    public uint CurrentHitPoints { get; set; }

    [Export(hintString: "Team this pawn belongs to.")]
    public Team Team { get; set; }

    public override void _Ready()
    {
        CurrentHitPoints = HitPoints;
        base._Ready();
    }

    public void TakeDamage(uint amount)
    {
        CurrentHitPoints -= amount;
    }
}