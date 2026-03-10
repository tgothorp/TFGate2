using Godot;
using TFGate2.scripts.logic;
using TFGate2.scripts.pawns;

namespace TFGate2.scripts.grid;

/// <summary>
/// Owns player-facing grid hover, selection, and path preview state.
/// </summary>
public partial class GridTargetingController : Node3D
{
    [Signal]
    public delegate void GridCellSelectedEventHandler(GridCell cell);

    [Signal]
    public delegate void GridPathConfirmedEventHandler(GridPath path, GridCell targetCell);

    public bool CanSelectGrid => _worldLogic != null && _worldLogic.IsPlayerInputAllowed && _worldLogic.PlayerTargetingContext.CanSelectGrid;
    public GridCell SelectedCell { get; private set; }
    public GridCell HoveredCell { get; private set; }
    public GridPath PreviewPath { get; private set; } = GridPath.Invalid;

    private GridManager _gridManager;
    private WorldLogic _worldLogic;

    public override void _Ready()
    {
        _gridManager = GetParent<GridManager>();
        _worldLogic = _gridManager?.GetParent<WorldLogic>();

        if (_gridManager == null)
            GD.PrintErr("GridTargetingController must be a child of GridManager.");

        if (_worldLogic == null)
            GD.PrintErr("WorldLogic not found for GridTargetingController.");
    }

    public void SelectCell(Vector2I coordinate)
    {
        if (!CanSelectGrid || _gridManager == null)
            return;

        SelectedCell = _gridManager.GetCell(coordinate);
        EmitSignal(SignalName.GridCellSelected, SelectedCell);

        if (PreviewPath.PathIsValid && SelectedCell != null && PreviewPath.End == SelectedCell.Coordinate)
        {
            EmitSignal(SignalName.GridPathConfirmed, PreviewPath, SelectedCell);
            return;
        }

        EmitSignal(SignalName.GridPathConfirmed, GridPath.Invalid, SelectedCell);
    }

    public void SetHoveredCell(Vector2I? coordinate)
    {
        if (!CanSelectGrid || _gridManager == null)
        {
            HoveredCell = null;
            PreviewPath = GridPath.Invalid;
            return;
        }

        HoveredCell = coordinate.HasValue ? _gridManager.GetCell(coordinate.Value) : null;
        UpdatePreviewPath();
    }

    public void ClearPreviewPath()
    {
        PreviewPath = GridPath.Invalid;
        SelectedCell = null;
        HoveredCell = null;
    }

    private void UpdatePreviewPath()
    {
        if (_worldLogic?.PlayerTargetingContext is not { HasActiveAbility: true, SourcePawn: MoveablePawn moveablePawn } ||
            HoveredCell == null ||
            moveablePawn.OccupiedCell == null)
        {
            PreviewPath = GridPath.Invalid;
            return;
        }

        PreviewPath = _gridManager.FindPath(moveablePawn.OccupiedCell, HoveredCell, moveablePawn.RemainingMoveBudget);
    }
}
