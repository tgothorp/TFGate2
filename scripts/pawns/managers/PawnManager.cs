using Godot;
using System;

public partial class PawnManager : Node3D
{
    [Export]
    public bool CanSelectPawns
    {
        get => _canSelectPawns;
        set
        {
            _selectedPawn = null;
            _canSelectPawns = value;
        }
    }

    private bool _canSelectPawns;
    private GridPawn _selectedPawn;
    
    public void SelectPawn(GridPawn pawn)
    {
        if (!CanSelectPawns)
            return;
        
        _selectedPawn = pawn;
        GD.Print($"Selected GridPawn: {pawn.Name}");

        if (pawn is StaticGridPawn)
        {
            GD.Print($"Selected GridPawn is a static pawn!");
        }
    }
}
