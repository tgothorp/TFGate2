using Godot;
using TFGate2.scripts.grid;
using TFGate2.scripts.logic;
using TFGate2.scripts.logic.actions;
using TFGate2.scripts.pawns;
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

    public PlayerTargetingContext PlayerTargetingContext { get; } = new();
    public bool IsPlayerInputAllowed => CurrentTeamTurn == PlayerTeam && _actionExecutor is not { IsExecuting: true };

    private GridManager _gridManager;
    private GridTargetingController _gridTargetingController;
    private PawnManager _pawnManager;
    private ActionExecutor _actionExecutor;

    public override void _Ready()
    {
        _gridManager = GetNodeOrNull<GridManager>("GridManager");
        if (_gridManager == null)
        {
            GD.PrintErr("GridManager not found!");
            return;
        }

        _gridTargetingController = _gridManager.GetNodeOrNull<GridTargetingController>("GridTargetingController");
        if (_gridTargetingController == null)
        {
            GD.PrintErr("GridTargetingController not found!");
            return;
        }

        _gridTargetingController.GridCellSelected += OnGridCellSelected;
        _gridTargetingController.GridPathConfirmed += OnGridPathConfirmed;

        _pawnManager = GetNodeOrNull<PawnManager>("PawnManager");
        if (_pawnManager == null)
        {
            GD.PrintErr("PawnManager not found!");
            return;
        }

        _pawnManager.PawnSelected += OnPawnSelected;
        _pawnManager.AbilitySelected += OnPawnAbilitySelected;

        _actionExecutor = GetNodeOrNull<ActionExecutor>("ActionExecutor");
        if (_actionExecutor == null)
        {
            _actionExecutor = new ActionExecutor { Name = "ActionExecutor" };
            AddChild(_actionExecutor);
        }

        _actionExecutor.Initialize(this, _pawnManager, _gridManager);
        _actionExecutor.ActionExecutionStarted += OnActionExecutionStarted;
        _actionExecutor.ActionExecutionCompleted += OnActionExecutionCompleted;
        _actionExecutor.ActionExecutionFailed += OnActionExecutionFailed;

        BeginCurrentTurn();
    }

    private void OnGridCellSelected(GridCell cell)
    {
        GD.Print($"[WORLD-LOGIC] Cell selected: {cell.Coordinate}");
    }

    private void OnGridPathConfirmed(GridPath path, GridCell targetCell)
    {
        GD.Print($"[WORLD-LOGIC] Path confirmed: {path.Start} -> {path.End}");

        if (PlayerTargetingContext is not { HasActiveAbility: true, SourcePawn: not null, SelectedAbility: not null })
            return;

        if (!path.PathIsValid)
            return;

        var command = new PawnActionCommand(
            PlayerTargetingContext.SourcePawn,
            PlayerTargetingContext.SelectedAbility,
            targetCell: targetCell,
            confirmedPath: path);

        _actionExecutor.TryExecute(command);
    }

    private void OnPawnSelected(GridPawn pawn)
    {
        GD.Print($"[WORLD-LOGIC] Pawn selected: {pawn.Name}");
        if (PlayerTargetingContext is { HasActiveAbility: true, SourcePawn: not null, SelectedAbility: not null })
        {
            var command = new PawnActionCommand(
                PlayerTargetingContext.SourcePawn,
                PlayerTargetingContext.SelectedAbility,
                targetPawn: pawn);

            _actionExecutor.TryExecute(command);
            return;
        }

        PlayerTargetingContext.PawnSelected(pawn);
    }

    private void OnPawnAbilitySelected(PawnAbility ability)
    {
        GD.Print($"[WORLD-LOGIC] Pawn ability selected: {ability?.AbilityName ?? "None"}");
        PlayerTargetingContext.AbilitySelected(ability);
        _gridTargetingController?.ClearPreviewPath();
    }

    private void OnActionExecutionStarted()
    {
        GD.Print("[WORLD-LOGIC] Action execution started");
        PlayerTargetingContext.BeginExecutionLock();
        _gridTargetingController?.ClearPreviewPath();
    }

    private void OnActionExecutionCompleted()
    {
        GD.Print("[WORLD-LOGIC] Action execution completed");
        _pawnManager.DeselectAbility();
        PlayerTargetingContext.CompleteAbilitySelection();
        _gridTargetingController?.ClearPreviewPath();
        _pawnManager.RefreshUi();
    }

    private void OnActionExecutionFailed(string reason)
    {
        GD.PrintErr($"[WORLD-LOGIC] Action execution failed: {reason}");
        if (_pawnManager.SelectedAbility != null)
            PlayerTargetingContext.AbilitySelected(_pawnManager.SelectedAbility);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsPlayerInputAllowed)
        {
            base._UnhandledInput(@event);
            return;
        }

        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Right &&
            mouseButton.Pressed)
        {
            if (PlayerTargetingContext.HasActiveAbility)
            {
                PlayerTargetingContext.CompleteAbilitySelection();
                _pawnManager.DeselectAbility();
                _gridTargetingController?.ClearPreviewPath();
            }
            else if (PlayerTargetingContext.SourcePawn != null)
            {
                PlayerTargetingContext.PawnSelected(null);
                PlayerTargetingContext.UpdateSelectionState(PlayerTargetingContext.SelectionState.AllPawns);
                _pawnManager.DeselectPawn();
            }
        }

        base._UnhandledInput(@event);
    }

    public void AdvanceTurn()
    {
        CurrentTeamTurn = CurrentTeamTurn switch
        {
            Team.Red => Team.Blue,
            Team.Blue => Team.Red,
            _ => PlayerTeam
        };

        PlayerTargetingContext.Clear();
        _pawnManager.DeselectPawn();
        _gridTargetingController?.ClearPreviewPath();
        BeginCurrentTurn();
    }

    private void BeginCurrentTurn()
    {
        foreach (var pawn in _pawnManager.RegisteredPawns.Values)
        {
            if (pawn is CombatPawn combatPawn && combatPawn.Team == CurrentTeamTurn)
                combatPawn.BeginTurn();
        }

        _pawnManager.RefreshUi();
    }
}
