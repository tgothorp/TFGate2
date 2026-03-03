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
    
    [Export(hintString: "Team this pawn belongs to.")]
    public Team Team { get; set; }
}