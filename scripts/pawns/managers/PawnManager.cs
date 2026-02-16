using Godot;

public partial class PawnManager : Node3D
{
    [Export]
    public PawnSelectionMode SelectionMode { get; set; }

    private bool _canSelectPawns;
    private GridPawn _selectedPawn;
    
    public void SelectPawn(GridPawn pawn)
    {
        if (SelectionMode == PawnSelectionMode.Off)
            return;
        
        // TODO: Check if pawn is in the correct team
        
        _selectedPawn = pawn;
        GD.Print($"Selected GridPawn: {pawn.Name}");
    }
}

public enum PawnSelectionMode
{
    Off,
    TeamRed,
    TeamBlue,
    NonTeamPawns,
    All,
}
