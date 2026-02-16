using System;
using Godot;

namespace TFGate2.scripts.pawns.abilities;

public partial class PawnAbility : Node3D
{
    [Export]
    public string AbilityName { get; set; }

    [Export]
    public AbilityTarget Target { get; set; }

    [Export]
    public AbilityCost Cost { get; set; }

    private WorldLogic _worldLogic;
    private GridPawn _owner;

    public void Register(WorldLogic worldLogic, GridPawn owner)
    {
        _worldLogic = worldLogic;
        _owner = owner;
    }

    public virtual void ExecuteAbility()
    {
        switch (Target)
        {
            case AbilityTarget.Self:
                break;
            case AbilityTarget.EnemyPawn:
                break;
            case AbilityTarget.TeammatePawn:
                break;
            case AbilityTarget.AnyPawn:
                break;
            case AbilityTarget.Grid:
                _worldLogic.UpdateSelectionState(WorldLogic.SelectionState.Grid);
                break;
            case AbilityTarget.GridAndEnemies:
                break;
            case AbilityTarget.GridAndTeammates:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public enum AbilityTarget
    {
        Self,
        EnemyPawn,
        TeammatePawn,
        AnyPawn,
        Grid,
        GridAndEnemies,
        GridAndTeammates,
    }

    public enum AbilityCost
    {
        Free,
        Action,
        BonusAction,
        Reaction,
        Special,
    }
}

