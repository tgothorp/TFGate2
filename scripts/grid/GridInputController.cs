using Godot;

namespace TFGate2.scripts.grid;

public partial class GridInputController : Node3D
{
    private GridManager _gridManager;
    private Camera3D _camera;

    public override void _Ready()
    {
        _gridManager = GetParent<GridManager>();
        
        if (_gridManager == null)
        {
            GD.PrintErr("GridInputController must be a child of GridManager!");
        }
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            HandleHover(mouseMotion.Position);
        }

        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
            {
                HandleClick(mouseButton.Position);
            }
        }
    }
    
    private void HandleClick(Vector2 mousePosition)
    {
        if (!TryGetGridCoordAtMouse(mousePosition, out var gridCoord))
            return;

        _gridManager.SelectCell(gridCoord);
    }

    private void HandleHover(Vector2 mousePosition)
    {
        if (!_gridManager.CanSelectGrid)
        {
            _gridManager.SetHoveredCell(null);
            return;
        }

        if (!TryGetGridCoordAtMouse(mousePosition, out var gridCoord))
        {
            _gridManager.SetHoveredCell(null);
            return;
        }

        _gridManager.SetHoveredCell(gridCoord);
    }

    private bool TryGetGridCoordAtMouse(Vector2 mousePosition, out Vector2I gridCoord)
    {
        gridCoord = default;

        _camera = GetViewport().GetCamera3D();
        if (_camera == null)
            return false;

        var from = _camera.ProjectRayOrigin(mousePosition);
        var to = from + _camera.ProjectRayNormal(mousePosition) * 1000f;

        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        var result = spaceState.IntersectRay(query);
        if (result.Count == 0)
            return false;

        var hitPosition = (Vector3)result["position"];
        var maybeGridCoord = _gridManager.WorldToGrid(hitPosition);
        if (!maybeGridCoord.HasValue)
            return false;

        gridCoord = maybeGridCoord.Value;
        return true;
    }
}
