using Godot;
using System;

public partial class PawnInputController : Node3D
{
    private PawnManager _pawnManager;

    public override void _Ready()
    {
        _pawnManager = GetParent<PawnManager>();
        
        if (_pawnManager == null)
        {
            GD.PrintErr("PawnManager not found!");
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
        var camera = GetViewport().GetCamera3D();
        if (camera == null)
        {
            GD.PrintErr("No camera found!");
            return;
        }

        var from = camera.ProjectRayOrigin(mousePosition);
        var to = from + camera.ProjectRayNormal(mousePosition) * 1000;

        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            var collider = result["collider"].As<Node>();

            // Search up the node tree for a GridPawn
            var currentNode = collider;
            while (currentNode != null)
            {
                if (currentNode is GridPawn gridPawn)
                {
                    _pawnManager.SelectPawn(gridPawn);
                    return;
                }

                currentNode = currentNode.GetParent();
            }
        }
    }
}
