using Godot;

namespace TFGate2.scripts.grid;

public partial class GridDebugRenderer : Node3D
{
    [Export]
    public float YOffset { get; set; } = 0.02f;

    [Export]
    public float Thickness { get; set; } = 0.02f;

    [Export]
    public bool ShouldRender { get; set; } = true;

    private MultiMeshInstance3D _mmi = default!;

    public override void _Ready()
    {
        _mmi = new MultiMeshInstance3D();
        AddChild(_mmi);

        var box = new BoxMesh();

        _mmi.Multimesh = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            Mesh = box
        };

        Visible = ShouldRender;
    }

    public void Rebuild(GridManager grid)
    {
        if (_mmi.Multimesh == null)
            return;

        var count = grid.Width * grid.Height;
        _mmi.Multimesh.InstanceCount = count;

        // Make each cell a thin “plate” sized to CellSize
        var plateScale = new Vector3(grid.CellSize, Thickness, grid.CellSize);

        var i = 0;
        for (var x = 0; x < grid.Width; x++)
        {
            for (var z = 0; z < grid.Height; z++)
            {
                var coord = new Vector2I(x, z);

                // Use grid-local centers so the whole grid inherits GridManager transform naturally
                var localCenter = grid.GridToLocal(coord) + new Vector3(0f, YOffset, 0f);

                var t = new Transform3D(Basis.Identity, localCenter);
                t = t.ScaledLocal(plateScale);

                _mmi.Multimesh.SetInstanceTransform(i, t);
                i++;
            }
        }
    }
}