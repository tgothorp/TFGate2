using Godot;

namespace TFGate2.scripts.pawns.managers;

/// <summary>
/// Visual-only helper that highlights the currently selected pawn.
/// </summary>
public partial class PawnDebugRenderer : Node3D
{
    private PawnManager _pawnManager;
    private GridPawn _highlightedPawn;
    private MeshInstance3D _highlightedMesh;
    private Material _previousOverlay;

    [Export]
    public Color HighlightColor { get; set; } = new(0.2f, 1.0f, 0.2f, 0.35f);

    private StandardMaterial3D _highlightMaterial;

    public override void _Ready()
    {
        _pawnManager = GetParent<PawnManager>();
        if (_pawnManager == null)
        {
            GD.PrintErr("PawnManager not found for PawnDebugRenderer.");
            return;
        }

        _highlightMaterial = new StandardMaterial3D
        {
            AlbedoColor = HighlightColor,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            NoDepthTest = true
        };
    }

    public override void _Process(double delta)
    {
        if (_pawnManager == null)
            return;

        var selectedPawn = _pawnManager.SelectedPawn;

        if (selectedPawn == _highlightedPawn)
            return;

        ClearCurrentHighlight();

        if (selectedPawn != null)
        {
            ApplyHighlight(selectedPawn);
        }
    }

    public override void _ExitTree()
    {
        ClearCurrentHighlight();
        base._ExitTree();
    }

    private void ApplyHighlight(GridPawn pawn)
    {
        var mesh = pawn.GetNodeOrNull<MeshInstance3D>("StaticBody3D/MeshInstance3D");
        if (mesh == null)
        {
            GD.PrintErr($"Could not find MeshInstance3D for pawn '{pawn.Name}'.");
            return;
        }

        _highlightedPawn = pawn;
        _highlightedMesh = mesh;
        _previousOverlay = mesh.MaterialOverlay;
        mesh.MaterialOverlay = _highlightMaterial;
    }

    private void ClearCurrentHighlight()
    {
        if (_highlightedMesh != null && IsInstanceValid(_highlightedMesh))
        {
            _highlightedMesh.MaterialOverlay = _previousOverlay;
        }

        _highlightedPawn = null;
        _highlightedMesh = null;
        _previousOverlay = null;
    }
}
