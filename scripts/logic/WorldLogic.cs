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

    public SelectionContext SelectionContext { get; set; } = new();

    private GridManager _gridManager;
    private PawnManager _pawnManager;

    public override void _Ready()
    {
        var gridManager = GetNode<GridManager>("GridManager");
        if (gridManager == null)
        {
            GD.PrintErr("GridManager not found!");
            return;
        }
        _gridManager = gridManager;
        _gridManager.GridCellSelected += OnGridCellSelected;
        _gridManager.GridPathCalculated += OnGridPathCalculated;

        var pawnManager = GetNode<PawnManager>("PawnManager");
        if (pawnManager == null)
        {
            GD.PrintErr("PawnManager not found!");
            return;
        }
        _pawnManager = pawnManager;
        _pawnManager.PawnSelected += OnPawnSelected;
        _pawnManager.AbilityResolving += OnPawnAbilityResolutionStarted;
        _pawnManager.AbilitySelected += OnPawnAbilitySelected;
    }

    private void OnGridPathCalculated(GridPath path)
    {
        GD.Print($"[WORLD-LOGIC] Path calculated: {path}");
        SelectionContext.GridPathSelected(path);
    }

    private void OnGridCellSelected(GridCell cell)
    {
        GD.Print($"[WORLD-LOGIC] Cell selected: {cell.Coordinate}");
        SelectionContext.GridCellSelected(cell);
    }

    private void OnPawnSelected(GridPawn pawn)
    {
        GD.Print($"[WORLD-LOGIC] Pawn selected: {pawn.Name}");
        SelectionContext.PawnSelected(pawn);
    }

    private void OnPawnAbilitySelected(PawnAbility ability)
    {
        GD.Print($"[WORLD-LOGIC] Pawn ability selected: {ability.AbilityName}");
        SelectionContext.AbilitySelected(ability);
    }

    private void OnPawnAbilityResolutionStarted()
    {
        GD.Print("[WORLD-LOGIC] Pawn ability resolution started");
        SelectionContext.DisableSelection();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
            {
                // TODO: Deselect ability / pawn / cell
            }
        }
        base._UnhandledInput(@event);
    }

    // public void UpdateTargetingContext(PawnAbility ability)
    // {
    //     SelectionContext ??= new SelectionContext();
    //
    //     UpdateSelectionState(ability.Target);
    //
    //     SelectionContext.IsActive = true;
    //     SelectionContext.PawnAbility = ability;
    //     SelectionContext.SourcePawn = ability.Pawn;
    //     SelectionContext.HoveredCell = null;
    //     SelectionContext.SelectedCell = null;
    //     SelectionContext.PreviewPath = GridPath.Invalid;
    // }

    // public void ClearTargetingContext()
    // {
    //     SelectionContext ??= new SelectionContext();
    //     SelectionContext.Clear();
    // }
    
    public void GridCellSelected(GridCell cell)
    {
        if (SelectionContext is { AbilityBeingResolved: true })
            _pawnManager.ResolveAbility(null, cell);
    }

    public void PawnSelected(GridPawn pawn)
    {
        if (!SelectionContext.CanSelectPawns)
            return;

        if (SelectionContext is { AbilityBeingResolved: true })
            _pawnManager.ResolveAbility(pawn, null);
        else
            _pawnManager.SelectPawn(pawn);
    }
}
