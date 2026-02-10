using Godot;

namespace TFGate2.scripts.grid;

[Tool]
public partial class GridDebugRenderer : Node3D
{
    [Export]
    public new bool Show
    {
        get => _show;
        set
        {
            _show = value;
            ApplyVisibility();
            RebuildDeferred();
        }
    }

    [Export]
    public float YOffset
    {
        get => _yOffset;
        set
        {
            _yOffset = value;
            RebuildDeferred();
        }
    }

    [Export]
    public Color LineColor
    {
        get => _lineColor;
        set
        {
            _lineColor = value;
            RebuildDeferred();
        }
    }

    [ExportToolButton("Redraw Grid")]
    public Callable RedrawGrid => Callable.From(RebuildDeferred);

    private bool _show = true;
    private float _yOffset = 0.05f;
    private Color _lineColor = new(0, 248, 179, 1);

    private MultiMeshInstance3D _mmi;
    private MultiMesh _mm;
    private BoxMesh _box;
    private StandardMaterial3D _mat;

    public override void _EnterTree()
    {
        EnsureNodes();
        ApplyVisibility();
        RebuildDeferred();
    }

    private void EnsureNodes()
    {
        _mmi ??= new MultiMeshInstance3D();
        
        if (_mmi.GetParent() == null) AddChild(_mmi);

        _box = new BoxMesh();
        _mat = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = new Color(0, 1, 0, 1),
        };

        _mmi.MaterialOverride = _mat;

        _mm = new MultiMesh
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            Mesh = _box,
        };
        _mmi.Multimesh = _mm;
    }

    private void ApplyVisibility()
    {
        if (_mmi != null && IsInstanceValid(_mmi))
            _mmi.Visible = _show;

        Visible = _show;
    }

    private void RebuildDeferred()
    {
        if (!IsInsideTree())
            return;

        // Defer so it rebuilds after inspector changes settle
        CallDeferred(nameof(Rebuild));
    }

    public void Rebuild()
    {
        EnsureNodes();

        var grid = GetParent<GridManager>();
        
        // Settings (tweak in inspector if you want)
        float y = YOffset;              // your existing Y offset
        float thickness = 0.02f;        // width of the “line”
        float height = 0.02f;           // height of the box

        // Grid extents in grid-local space (assuming your GridToLocal uses (x+0.5)*CellSize centers)
        // We want line positions on cell boundaries:
        float minX = 0f;
        float minZ = 0f;
        float maxX = grid.Width * grid.CellSize;
        float maxZ = grid.Height * grid.CellSize;

        int verticalCount = grid.Width + 1;
        int horizontalCount = grid.Height + 1;
        int total = verticalCount + horizontalCount;

        _mm.InstanceCount = total;

        int i = 0;

        // Vertical lines: constant X, spanning Z
        // Position on boundary: x * CellSize
        // Center should be half way along span.
        float vSpan = maxZ - minZ;
        for (int x = 0; x <= grid.Width; x++)
        {
            float lx = x * grid.CellSize;
            var center = new Vector3(lx, y, minZ + vSpan * 0.5f);

            // Scale: thin in X, short in Y, long in Z
            var scale = new Vector3(thickness, height, vSpan);

            var t = new Transform3D(Basis.Identity, center).ScaledLocal(scale);
            _mm.SetInstanceTransform(i++, t);
        }

        // Horizontal lines: constant Z, spanning X
        float hSpan = maxX - minX;
        for (int z = 0; z <= grid.Height; z++)
        {
            float lz = z * grid.CellSize;
            var center = new Vector3(minX + hSpan * 0.5f, y, lz);

            // Scale: long in X, short in Y, thin in Z
            var scale = new Vector3(hSpan, height, thickness);

            var t = new Transform3D(Basis.Identity, center).ScaledLocal(scale);
            _mm.SetInstanceTransform(i++, t);
        }
    }
}