using System.Collections.Generic;
using System.Linq;
using Godot;
using TFGate2.scripts.grid;
using TFGate2.scripts.pawns.abilities;

public partial class GridPawn : Node3D
{
    [Export(hintString: "Should this pawn snap to the center of its cell?")]
    public bool ShouldSnapToCellCenter { get; set; } = false;

    [Export(hintString: "Team this pawn belongs to.")]
    public WorldLogic.Team Team { get; set; }

    public GridCell OccupiedCell { get; private set; }

    private List<PawnAbility> _pawnAbilities = [];

    public override void _Ready()
    {
        var root = GetTree().CurrentScene;

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

    public void SetOccupiedCell(GridCell cell)
    {
        OccupiedCell = cell;
    }

    public override string ToString()
    {
        var abilities = string.Join(", ", _pawnAbilities.Select(x => x.AbilityName));
        return $"[PAWN] {Name}, Abilities: {abilities}";
    }
}
