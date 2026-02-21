using TFGate2.scripts.grid;

namespace TFGate2.scripts.pawns.abilities;

/// <summary>
/// Does nothing, stops continual enumeration of pawns with no abilities. 
/// </summary>
public partial class NoAbility : PawnAbility
{
    public override bool CanExecute(AbilityExecutionContext context)
    {
        return false;
    }

    public override void Execute(AbilityExecutionContext context)
    {
        // Intentionally empty.
    }
}
