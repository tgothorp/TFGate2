using Godot;
using TFGate2.scripts.logic;

namespace TFGate2.scripts.pawns;

/// <summary>
/// Represents a player-controlled pawn with abilities
/// </summary>
public partial class PlayerPawn : GridPawn
{
    [Export]
    public CharacterClass Class { get; set; }
    
    public override void _Ready()
    {
        base._Ready();
    }
}