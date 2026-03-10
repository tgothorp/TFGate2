using Godot;

namespace TFGate2.scripts.logic;

public partial class WorldLogicUiController : Control
{
    [Export]
    public Label TeamLabel { get; set; }

    [Export]
    public Button PassTurnButton { get; set; }

    private WorldLogic _worldLogic;

    public override void _EnterTree()
    {
        _worldLogic = GetParent<WorldLogic>();
        base._EnterTree();
    }

    public override void _Ready()
    {
        PassTurnButton.Pressed += PassPlayerTurn;
        TurnAdvanced();

        base._Ready();
    }

    public void TurnAdvanced()
    {
        PassTurnButton.Visible = _worldLogic.PlayerTeam == _worldLogic.CurrentTeamTurn;
        TeamLabel.Text = $"Current Team Turn: {_worldLogic.CurrentTeamTurn.ToString()}";
    }
    
    private void PassPlayerTurn()
    {
        if (_worldLogic.CurrentTeamTurn != _worldLogic.PlayerTeam)
            return;

        _worldLogic.AdvanceTurn();
    }
}