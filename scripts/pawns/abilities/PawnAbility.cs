using System;
using Godot;

namespace TFGate2.scripts.pawns.abilities;

public partial class PawnAbility : Node3D
{
    [Export]
    public string AbilityName { get; set; }

    [Export]
    public WorldLogic.SelectionState Target { get; set; }

    [Export]
    public AbilityCost Cost { get; set; }

    private WorldLogic _worldLogic;
    private GridPawn _owner;

    public void Register(WorldLogic worldLogic, GridPawn owner)
    {
        _worldLogic = worldLogic;
        _owner = owner;
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

