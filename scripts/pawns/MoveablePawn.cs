using Godot;
using TFGate2.scripts.pawns.utils;

namespace TFGate2.scripts.pawns;

/// <summary>
/// A pawn that can move about this grid with a move budget
/// </summary>
public partial class MoveablePawn : GridPawn
{
    [Export(hintString: "How much movement this pawn can perform per turn.")]
    public int MoveBudget { get; set; }
    public int RemainingMoveBudget { get; set; }

    public PawnMover Mover { get; } = new();

    public override void _Ready()
    {
        RemainingMoveBudget = MoveBudget;

        base._Ready();
    }

    public void SetMoveBudget(int amount)
    {
        RemainingMoveBudget = amount;
    }
}