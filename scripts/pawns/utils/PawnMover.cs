using Godot;
using TFGate2.scripts.grid;

namespace TFGate2.scripts.pawns.utils;

public partial class PawnMover : Node
{
    [Signal]
    public delegate void MoveFinishedEventHandler(MoveablePawn pawn);

    public bool IsMoving { get; private set; }

    private MoveablePawn _pawn;
    private GridPath _path;
    private int _index;

    public void Initialize(MoveablePawn pawn) => _pawn = pawn;

    public bool TryStart(GridPath path)
    {
        if (IsMoving || !path.PathIsValid || path.WorldPath.Length == 0) 
            return false;

        _path = path;
        _index = 0;
        IsMoving = true;

        return true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsMoving) 
            return;

        var target = _path.WorldPath[_index];
        var next = _pawn.CharacterBody3D.GlobalPosition.MoveToward(target, _pawn.MoveSpeed * (float)delta);
        _pawn.CharacterBody3D.GlobalPosition = next;

        if (next.DistanceTo(target) <= 0.05f)
        {
            _pawn.CharacterBody3D.GlobalPosition = target;
            _index++;
            if (_index >= _path.WorldPath.Length)
            {
                IsMoving = false;
                SetPhysicsProcess(false);
                EmitSignal(SignalName.MoveFinished, _pawn);
            }
        }
    }
}