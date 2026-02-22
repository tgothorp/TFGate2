using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TFGate2.scripts.grid;
using TFGate2.scripts.pawns.abilities;

public partial class GridPawn : Node3D
{
    public Guid PawnId { get; set; }
    
    [Export(hintString: "Should this pawn snap to the center of its cell?")]
    public bool ShouldSnapToCellCenter { get; set; } = false;

    [Export(hintString: "Team this pawn belongs to.")]
    public WorldLogic.Team Team { get; set; }

    [Export(hintString: "How much movement this pawn can perform per turn.")]
    public int MoveBudget { get; set; }
    public int RemainingMoveBudget { get; set; }

    [Export]
    public bool CanTakeAction { get; set; }
    public bool HasTakenAction { get; set; }

    [Export]
    public bool CanTakeBonusAction { get; set; }
    public bool HasTakenBonusAction { get; set; }

    [Export]
    public bool CanTakeReaction { get; set; }
    public bool HasTakenReaction { get; set; }

    public GridCell OccupiedCell { get; private set; }

    private List<PawnAbility> _pawnAbilities = [];

    public override void _Ready()
    {
        var root = GetTree().CurrentScene;

        var pawnManager = root.GetNode<PawnManager>("WorldManager/PawnManager");
        if (pawnManager == null)
        {
            GD.PrintErr("[PAWN] PawnManager not found!");
            return;
        }
        
        PawnId = pawnManager.RegisterPawn(this);
        RemainingMoveBudget = MoveBudget;
        
        var worldLogic = root.GetNode<WorldLogic>("WorldManager");
        var gridManager = root.GetNode<GridManager>("WorldManager/GridManager");
        if (gridManager == null)
        {
            GD.PrintErr("[PAWN] GridManager not found!");
            return;
        }

        var cell = gridManager.AddPawn(this);
        if (cell != null)
        {
            OccupiedCell = cell;
        }

        GetAbilities(worldLogic);
    }

    public List<PawnAbility> GetAbilities(WorldLogic worldLogic)
    {
        if (_pawnAbilities.Count > 0)
            return _pawnAbilities;

        var abilities = GetNode("Abilities").GetChildren();
        foreach (var ability in abilities)
        {
            if (ability is PawnAbility abilityNode)
            {
                GD.Print($"[PAWN] {Name} has ability: " + abilityNode.Name);
                abilityNode.Register(worldLogic, this);

                _pawnAbilities.Add(abilityNode);
            }
        }

        return _pawnAbilities;
    }

    public void StartTurn()
    {
        RemainingMoveBudget = MoveBudget;

        if (CanTakeAction)
            HasTakenAction = false;

        if (CanTakeBonusAction)
            HasTakenBonusAction = false;

        if (CanTakeReaction)
            HasTakenReaction = false;
    }

    public bool CanPerformAction(PawnAbility.AbilityCost cost)
    {
        switch (cost)
        {
            case PawnAbility.AbilityCost.Free:
                return true;
            case PawnAbility.AbilityCost.Action:
                return !HasTakenAction;
            case PawnAbility.AbilityCost.BonusAction:
                return !HasTakenBonusAction;
            case PawnAbility.AbilityCost.Reaction:
                return !HasTakenReaction;
            case PawnAbility.AbilityCost.Special:
                //TODO
            default:
                return false;
        }
    }

    public void SetOccupiedCell(GridCell cell)
    {
        OccupiedCell = cell;
    }

    public void SetMoveBudget(int amount)
    {
        RemainingMoveBudget = amount;
    }

    public override string ToString()
    {
        var abilities = string.Join(", ", _pawnAbilities.Select(x => x.AbilityName));
        return $"[PAWN] {Name}, Team: {Team.ToString()}, Abilities: {abilities}";
    }
}