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
        _gridManager.GridPathConfirmed += OnGridPathConfirmed;

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

    private void OnGridCellSelected(GridCell cell)
    {
        GD.Print($"[WORLD-LOGIC] Cell selected: {cell.Coordinate}");
        SelectionContext.GridCellSelected(cell);
    }

    private void OnGridPathConfirmed(GridPath path, GridCell targetCell)
    {
        GD.Print($"[WORLD-LOGIC] Path confirmed: {path.Start} -> {path.End}");

        if (SelectionContext is not { AbilityBeingResolved: true })
            return;

        if (!path.PathIsValid)
            return;

        SelectionContext.ConfirmPath(path);
        _pawnManager.ResolveAbility(null, targetCell, path);
    }

    private void OnPawnSelected(GridPawn pawn)
    {
        GD.Print($"[WORLD-LOGIC] Pawn selected: {pawn.Name}");
        SelectionContext.PawnSelected(pawn);
    }

    private void OnPawnAbilitySelected(PawnAbility ability)
    {
        GD.Print($"[WORLD-LOGIC] Pawn ability selected: {ability?.AbilityName ?? "None"}");
        SelectionContext.AbilitySelected(ability);
        _gridManager?.ClearPreviewPath();
    }

    private void OnPawnAbilityResolutionStarted()
    {
        GD.Print("[WORLD-LOGIC] Pawn ability resolution started");
        SelectionContext.DisableSelection();
        _gridManager?.ClearPreviewPath();
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
}
