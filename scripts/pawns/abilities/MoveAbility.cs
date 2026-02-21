using Godot;

namespace TFGate2.scripts.pawns.abilities;

public partial class MoveAbility : PawnAbility
{
    [Export]
    public int Distance { get; set; } = 7;

    private int _remainingDistance;

    public override void _EnterTree()
    {
        _remainingDistance = Distance;
        base._EnterTree();
    }
}