using Godot;
using TFGate2.scripts.grid;
using TFGate2.scripts.logic;
using TFGate2.scripts.pawns.abilities;

/// <summary>
/// Coordinates high-level combat state such as turn context and world selection mode.
/// </summary>
public partial class WorldLogic : Node3D
{
    [Export]
    public Team PlayerTeam { get; set; } = Team.Red;
    
    [Export]
    public Team CurrentTeamTurn { get; set; } = Team.Red;
    
    public SelectionState CurrentSelectionState => TargetingContext?.SelectionState ?? SelectionState.AllPawns;

    public bool CanSelectGrid => CurrentSelectionState == SelectionState.Grid;
    public bool CanSelectPawns => CurrentSelectionState is SelectionState.EnemyPawns or SelectionState.TeamPawns or SelectionState.AllPawns;

    public TargetingContext TargetingContext { get; set; } = null;

    private GridManager _gridManager;
    private PawnManager _pawnManager;

    public override void _EnterTree()
    {
        TargetingContext = new TargetingContext
        {
            IsActive = false,
            SelectionState = SelectionState.AllPawns,
            PreviewPath = GridPath.Invalid
        };

        base._EnterTree();
    }

    public override void _Ready()
    {
        var gridManager = GetNode<GridManager>("GridManager");
        if (gridManager == null)
        {
            GD.PrintErr("GridManager not found!");
            return;
        }

        var pawnManager = GetNode<PawnManager>("PawnManager");
        if (pawnManager == null)
        {
            GD.PrintErr("PawnManager not found!");
            return;
        }

        _gridManager = gridManager;
        _pawnManager = pawnManager;
        
        ClearTargetingContext();
    }

    public void UpdateSelectionState(SelectionState newState)
    {
        TargetingContext ??= new TargetingContext();
        TargetingContext.SelectionState = newState;
    }

    public void UpdateTargetingContext(PawnAbility ability)
    {
        TargetingContext ??= new TargetingContext();

        UpdateSelectionState(ability.Target);

        TargetingContext.IsActive = true;
        TargetingContext.PawnAbility = ability;
        TargetingContext.SourcePawn = ability.Pawn;
        TargetingContext.HoveredCell = null;
        TargetingContext.SelectedCell = null;
        TargetingContext.PreviewPath = GridPath.Invalid;
    }

    public void ClearTargetingContext()
    {
        TargetingContext ??= new TargetingContext();
        TargetingContext.Clear();
    }

    public void GridCellSelected(GridCell cell)
    {
        if (TargetingContext is { IsActive: true } && _pawnManager != null)
            _pawnManager.ResolveAbility(null, cell);
    }
    
    public enum SelectionState
    {
        // Nothing in the world can be selected
        Nothing,
        
        // Only pawns can be selected
        EnemyPawns,
        TeamPawns,
        AllPawns,
        
        // Only the grid can be selected
        Grid,
    }
    
    public enum Team
    {
        World,
        Blue,
        Red,
    }
}
