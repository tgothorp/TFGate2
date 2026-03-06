using System;
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

    [Export(hintString: "How fast this pawn can move.")]
    public float MoveSpeed { get; set; } = 5f;

    public PawnMover Mover { get; private set; }

    public override void _EnterTree()
    {
        Mover = new PawnMover();
        AddChild(Mover);
        Mover.Initialize(this);

        base._EnterTree();
    }

    public override void _Ready()
    {
        RemainingMoveBudget = MoveBudget;
        base._Ready();
    }

    public void UpdateMoveBudget(int amount)
    {
        if (amount < 0)
            RemainingMoveBudget -= Math.Abs(amount);
        else
            RemainingMoveBudget += amount;
    }
}