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
    
    private Label3D _healthLabel;

    public override void _EnterTree()
    {
        _healthLabel = GetNode<Label3D>("HealthLabel");

        base._EnterTree();
    }

    public override void _Ready()
    {
        CurrentHitPoints = HitPoints;
        if (_healthLabel != null)
        {
            _healthLabel.Text = $"{CurrentHitPoints.ToString()}/{HitPoints}";
        }
        
        base._Ready();
    }

    public void TakeDamage(uint amount)
    {
        CurrentHitPoints -= amount;
        if (_healthLabel != null)
        {
            _healthLabel.Text = $"{CurrentHitPoints.ToString()}/{HitPoints}";
        }
    }
}