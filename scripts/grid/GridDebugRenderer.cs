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

    [Export]
    public bool ShowSelection
    {
        get => _showSelection;
        set
        {
            _showSelection = value;
            RebuildDeferred();
        }
    }

    [Export]
    public Color SelectionColor
    {
        get => _selectionColor;
        set
        {
            _selectionColor = value;
            RebuildDeferred();
        }
    }

    [Export]
    public float SelectionYOffset
    {
        get => _selectionYOffset;
        set
        {
            _selectionYOffset = value;
            RebuildDeferred();
        }
    }

    [ExportToolButton("Redraw Grid")]
    public Callable RedrawGrid => Callable.From(RebuildDeferred);

    private bool _show = true;
    private float _yOffset = 0.05f;
    private Color _lineColor = new(0, 248, 179, 1);
    private bool _showSelection = true;
    private Color _selectionColor = new(1f, 1f, 0f, 0.8f);
    private float _selectionYOffset = 0.08f;

    private MultiMeshInstance3D _mmi;
    private MultiMesh _mm;
    private BoxMesh _box;
    private StandardMaterial3D _mat;
    private MeshInstance3D _selectionHighlight;
    private GridInputController _inputController;

    public override void _EnterTree()
    {
        EnsureNodes();
        ApplyVisibility();
        RebuildDeferred();
    }

    public override void _Process(double delta)
    {
        UpdateSelectionHighlight();
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

        // Create selection highlight
        if (_selectionHighlight == null)
        {
            _selectionHighlight = new MeshInstance3D();
            AddChild(_selectionHighlight);
            
            var planeMesh = new PlaneMesh { Size = Vector2.One };
            var selectionMat = new StandardMaterial3D
            {
                AlbedoColor = _selectionColor,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                NoDepthTest = true
            };
            
            _selectionHighlight.Mesh = planeMesh;
            _selectionHighlight.MaterialOverride = selectionMat;
            _selectionHighlight.Visible = false;
        }
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

    private void UpdateSelectionHighlight()
    {
        if (!_showSelection || _selectionHighlight == null)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.Visible = false;
            return;
        }

        // Get the input controller if we don't have it yet
        if (_inputController == null)
        {
            var grid = GetParent<GridManager>();
            if (grid != null)
            {
                _inputController = grid.GetNodeOrNull<GridInputController>("GridInputController");
            }
        }

        if (_inputController == null)
            return;

        var selectedCell = _inputController.GetSelectedCell();
        if (selectedCell.HasValue)
        {
            var grid = GetParent<GridManager>();
            var localPos = grid.GridToLocal(selectedCell.Value);
            
            // Update highlight size and position
            var planeMesh = (PlaneMesh)_selectionHighlight.Mesh;
            planeMesh.Size = new Vector2(grid.CellSize, grid.CellSize);
            
            _selectionHighlight.Position = new Vector3(localPos.X, _selectionYOffset, localPos.Z);
            
            // Update color if changed
            var mat = (StandardMaterial3D)_selectionHighlight.MaterialOverride;
            mat.AlbedoColor = _selectionColor;
            
            _selectionHighlight.Visible = true;
        }
        else
        {
            _selectionHighlight.Visible = false;
        }
    }
}