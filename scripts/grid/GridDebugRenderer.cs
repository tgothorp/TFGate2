using Godot;
using System.Collections.Generic;

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
    public Color HoverColor
    {
        get => _hoverColor;
        set
        {
            _hoverColor = value;
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
    
    [Export]
    public Color PathColor
    {
        get => _pathColor;
        set
        {
            _pathColor = value;
            UpdatePathMaterials();
        }
    }

    [Export]
    public float PathYOffset
    {
        get => _pathYOffset;
        set => _pathYOffset = value;
    }

    [ExportToolButton("Redraw Grid")]
    public Callable RedrawGrid => Callable.From(RebuildDeferred);

    private bool _show = true;
    private float _yOffset = 0.05f;
    private Color _lineColor = new(0, 248, 179, 1);
    private bool _showSelection = true;
    private Color _selectionColor = new(0.2f, 1f, 0.2f, 0.85f);
    private Color _hoverColor = new(0.2f, 1f, 0.2f, 0.45f);
    private Color _pathColor = new(0.2f, 0.7f, 1f, 0.35f);
    private float _selectionYOffset = 0.08f;
    private float _pathYOffset = 0.06f;
    private const float HoverYOffset = 0.07f;

    private MultiMeshInstance3D _mmi;
    private MultiMesh _mm;
    private BoxMesh _box;
    private StandardMaterial3D _mat;
    private MeshInstance3D _selectionHighlight;
    private MeshInstance3D _hoverHighlight;
    private readonly List<MeshInstance3D> _pathHighlights = [];
    private GridManager _gridManager;
    private WorldLogic _worldLogic;

    public override void _Ready()
    {
        EnsureNodes();
        ApplyVisibility();
        RebuildDeferred();
    }

    public override void _Process(double delta)
    {
        UpdateHoverHighlight();
        UpdateSelectionHighlight();
        UpdatePathHighlight();
    }

    private void UpdatePathHighlight()
    {
        if (_gridManager == null || _worldLogic?.TargetingContext == null)
            return;

        var path = _worldLogic.TargetingContext.PreviewPath;
        if (!path.PathIsValid || path.Path.Length == 0)
        {
            HideAllPathHighlights();
            return;
        }

        EnsurePathHighlights(path.Path.Length);

        var grid = GetParent<GridManager>();
        for (var i = 0; i < path.Path.Length; i++)
        {
            var cellCoordinate = path.Path[i];
            var localPos = grid.GridToLocal(cellCoordinate);
            var highlight = _pathHighlights[i];

            var planeMesh = (PlaneMesh)highlight.Mesh;
            planeMesh.Size = new Vector2(grid.CellSize, grid.CellSize);

            highlight.Position = new Vector3(localPos.X, _pathYOffset, localPos.Z);
            highlight.Visible = true;
        }

        for (var i = path.Path.Length; i < _pathHighlights.Count; i++)
        {
            _pathHighlights[i].Visible = false;
        }
    }

    private void EnsureNodes()
    {
        _gridManager ??= GetParent<GridManager>();
        if (_worldLogic == null && _gridManager != null)
        {
            _worldLogic = _gridManager.GetParent() as WorldLogic;
        }

        if (_gridManager == null)
            return;

        _mmi ??= new MultiMeshInstance3D();
        
        if (_mmi.GetParent() == null) AddChild(_mmi);

        _box = new BoxMesh();
        _mat = new StandardMaterial3D
        {
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            AlbedoColor = _lineColor
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

        // Create hover highlight
        if (_hoverHighlight == null)
        {
            _hoverHighlight = new MeshInstance3D();
            AddChild(_hoverHighlight);

            var planeMesh = new PlaneMesh { Size = Vector2.One };
            var hoverMat = new StandardMaterial3D
            {
                AlbedoColor = _hoverColor,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                NoDepthTest = true
            };

            _hoverHighlight.Mesh = planeMesh;
            _hoverHighlight.MaterialOverride = hoverMat;
            _hoverHighlight.Visible = false;
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

        if (_gridManager == null || _mm == null)
            return;
        
        // Settings (tweak in inspector if you want)
        float y = YOffset;              // your existing Y offset
        float thickness = 0.02f;        // width of the “line”
        float height = 0.02f;           // height of the box

        // Grid extents in grid-local space (assuming your GridToLocal uses (x+0.5)*CellSize centers)
        // We want line positions on cell boundaries:
        float minX = 0f;
        float minZ = 0f;
        float maxX = _gridManager.Width * _gridManager.CellSize;
        float maxZ = _gridManager.Height * _gridManager.CellSize;

        int verticalCount = _gridManager.Width + 1;
        int horizontalCount = _gridManager.Height + 1;
        int total = verticalCount + horizontalCount;

        _mm.InstanceCount = total;

        int i = 0;

        // Vertical lines: constant X, spanning Z
        // Position on boundary: x * CellSize
        // Center should be half way along span.
        float vSpan = maxZ - minZ;
        for (int x = 0; x <= _gridManager.Width; x++)
        {
            float lx = x * _gridManager.CellSize;
            var center = new Vector3(lx, y, minZ + vSpan * 0.5f);

            // Scale: thin in X, short in Y, long in Z
            var scale = new Vector3(thickness, height, vSpan);

            var t = new Transform3D(Basis.Identity, center).ScaledLocal(scale);
            _mm.SetInstanceTransform(i++, t);
        }

        // Horizontal lines: constant Z, spanning X
        float hSpan = maxX - minX;
        for (int z = 0; z <= _gridManager.Height; z++)
        {
            float lz = z * _gridManager.CellSize;
            var center = new Vector3(minX + hSpan * 0.5f, y, lz);

            // Scale: long in X, short in Y, thin in Z
            var scale = new Vector3(hSpan, height, thickness);

            var t = new Transform3D(Basis.Identity, center).ScaledLocal(scale);
            _mm.SetInstanceTransform(i++, t);
        }
    }

    private void UpdateSelectionHighlight()
    {
        if (!_showSelection || _selectionHighlight == null || _worldLogic?.TargetingContext == null || !_worldLogic.CanSelectGrid)
        {
            if (_selectionHighlight != null)
                _selectionHighlight.Visible = false;
            return;
        }

        var selectedCell = _worldLogic.TargetingContext.SelectedCell;
        if (selectedCell != null)
        {
            var grid = GetParent<GridManager>();
            var localPos = grid.GridToLocal(selectedCell.Coordinate);
            
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
    
    private void UpdateHoverHighlight()
    {
        if (!_showSelection || _hoverHighlight == null || _worldLogic?.TargetingContext == null || !_worldLogic.CanSelectGrid)
        {
            if (_hoverHighlight != null)
                _hoverHighlight.Visible = false;
            return;
        }

        var hoveredCell = _worldLogic.TargetingContext.HoveredCell;
        if (hoveredCell == null)
        {
            _hoverHighlight.Visible = false;
            return;
        }

        var grid = GetParent<GridManager>();
        var localPos = grid.GridToLocal(hoveredCell.Coordinate);

        var planeMesh = (PlaneMesh)_hoverHighlight.Mesh;
        planeMesh.Size = new Vector2(grid.CellSize, grid.CellSize);

        _hoverHighlight.Position = new Vector3(localPos.X, HoverYOffset, localPos.Z);

        var mat = (StandardMaterial3D)_hoverHighlight.MaterialOverride;
        mat.AlbedoColor = _hoverColor;

        _hoverHighlight.Visible = true;
    }

    private void EnsurePathHighlights(int requiredCount)
    {
        while (_pathHighlights.Count < requiredCount)
        {
            var pathHighlight = new MeshInstance3D();
            var planeMesh = new PlaneMesh { Size = Vector2.One };
            var pathMat = new StandardMaterial3D
            {
                AlbedoColor = _pathColor,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                NoDepthTest = true
            };

            pathHighlight.Mesh = planeMesh;
            pathHighlight.MaterialOverride = pathMat;
            pathHighlight.Visible = false;
            AddChild(pathHighlight);

            _pathHighlights.Add(pathHighlight);
        }
    }

    private void HideAllPathHighlights()
    {
        for (var i = 0; i < _pathHighlights.Count; i++)
        {
            _pathHighlights[i].Visible = false;
        }
    }

    private void UpdatePathMaterials()
    {
        for (var i = 0; i < _pathHighlights.Count; i++)
        {
            if (_pathHighlights[i].MaterialOverride is StandardMaterial3D mat)
            {
                mat.AlbedoColor = _pathColor;
            }
        }
    }
}
