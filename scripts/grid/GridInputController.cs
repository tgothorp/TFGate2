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
        _camera = GetViewport().GetCamera3D();
        if (_camera == null)
            return;
        
        // Create ray from camera through mouse position
        var from = _camera.ProjectRayOrigin(mousePosition);
        var to = from + _camera.ProjectRayNormal(mousePosition) * 1000f;
        
        // Perform raycast
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;
        
        var result = spaceState.IntersectRay(query);
        
        if (result.Count > 0)
        {
            var hitPosition = (Vector3)result["position"];
            var gridCoord = _gridManager.WorldToGrid(hitPosition);
            
            if (gridCoord.HasValue)
            {
                _gridManager.SelectCell(gridCoord.Value);
            }
        }
    }
}