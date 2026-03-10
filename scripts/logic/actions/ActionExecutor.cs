using Godot;
using TFGate2.scripts.grid;
using TFGate2.scripts.pawns;
using TFGate2.scripts.pawns.abilities;

namespace TFGate2.scripts.logic.actions;

/// <summary>
/// Executes explicit gameplay action commands without relying on UI selection state.
/// </summary>
public partial class ActionExecutor : Node
{
    [Signal]
    public delegate void ActionExecutionStartedEventHandler();

    [Signal]
    public delegate void ActionExecutionCompletedEventHandler();

    [Signal]
    public delegate void ActionExecutionFailedEventHandler(string reason);

    public bool IsExecuting { get; private set; }
    public PawnActionCommand CurrentCommand { get; private set; }

    private WorldLogic _worldLogic;
    private PawnManager _pawnManager;
    private GridManager _gridManager;
    private PawnAbility _executingAbility;

    public void Initialize(WorldLogic worldLogic, PawnManager pawnManager, GridManager gridManager)
    {
        _worldLogic = worldLogic;
        _pawnManager = pawnManager;
        _gridManager = gridManager;
    }

    public bool TryExecute(PawnActionCommand command)
    {
        if (IsExecuting)
            return Fail("Another action is already executing.");

        if (!command.IsValid)
            return Fail("Action command is invalid.");

        if (_worldLogic == null || _pawnManager == null || _gridManager == null)
            return Fail("Action executor is not initialized.");

        if (command.SourcePawn is not CombatPawn combatPawn)
            return Fail("Source pawn cannot execute abilities.");

        if (!combatPawn.OwnsAbility(command.Ability))
            return Fail("Source pawn does not own the requested ability.");

        var context = new AbilityExecutionContext(
            _worldLogic,
            _pawnManager,
            _gridManager,
            command);

        if (!command.Ability.CanExecute(context))
            return Fail($"Ability '{command.Ability.AbilityName}' cannot execute.");

        CurrentCommand = command;
        IsExecuting = true;
        _executingAbility = command.Ability;

        EmitSignal(SignalName.ActionExecutionStarted);

        _executingAbility.AbilityFinished += OnAbilityFinished;
        _executingAbility.BeginExecute(context);
        return true;
    }

    private void OnAbilityFinished()
    {
        if (_executingAbility != null)
            _executingAbility.AbilityFinished -= OnAbilityFinished;

        _executingAbility = null;
        IsExecuting = false;
        CurrentCommand = default;

        EmitSignal(SignalName.ActionExecutionCompleted);
    }

    private bool Fail(string reason)
    {
        GD.PrintErr($"[ACTION-EXECUTOR] {reason}");
        EmitSignal(SignalName.ActionExecutionFailed, reason);
        return false;
    }
}
