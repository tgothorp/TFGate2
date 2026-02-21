using Godot;
using TFGate2.scripts.utils;

/// <summary>
/// Handles tactical camera movement, rotation, and zoom input.
/// </summary>
public partial class CameraManager : Node3D
{
    public Node3D RotationX { get; set; }
    public Node3D ZoomPivot { get; set; }
    public Camera3D Camera { get; set; }

    [Export]
    public float MoveSpeed { get; set; } = 0.2f;

    [Export]
    public float RotationSpeed { get; set; } = 1.5f;

    [Export]
    public float ZoomSpeed { get; set; } = 3.0f;

    [Export]
    public float MouseSensitivity { get; set; } = 1.5f;

    [Export]
    public float PanSensitivity { get; set; } = 0.015f;

    [Export]
    public float YawSensitivity { get; set; } = 0.2f; // lower = less Y rotation

    [Export]
    public float PitchSensitivity { get; set; } = 0.2f; // tilt speed

    [Export]
    public float YawDeadzonePx { get; set; } = 6.0f; // ignore small horizontal movement

    [Export]
    public float YawExponent { get; set; } = 1.5f; // makes small motion softer, big motion stronger


    private Vector3 _moveTarget;
    private float _rotationTarget;
    private float _zoomTarget;
    private float _zoomMin = -20.0f;
    private float _zoomMax = 20.0f;

    public override void _Ready()
    {
        RotationX = GetNode<Node3D>("%CameraRotationX");
        ZoomPivot = GetNode<Node3D>("%CameraZoomPivot");
        Camera = GetNode<Camera3D>("%Camera3D");

        _moveTarget = Position;
        _rotationTarget = RotationDegrees.Y;
        _zoomTarget = Camera.Position.Z;

        base._Ready();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && Input.IsActionPressed("rotate"))
        {
            // Check if Shift is held to determine pan vs rotate
            bool isPanning = Input.IsKeyPressed(Key.Shift);
            
            if (isPanning)
            {
                // Pan the camera
                var panDirection = new Vector3(-mouseMotion.Relative.X, 0, -mouseMotion.Relative.Y);
                var worldPan = (Transform.Basis * panDirection).Normalized() * panDirection.Length();
                _moveTarget += worldPan * PanSensitivity;
            }
            else
            {
                // Rotate the camera
                // Rotate around Y with deadzone + curve
                var dx = mouseMotion.Relative.X;

                if (Mathf.Abs(dx) > YawDeadzonePx)
                {
                    var sign = Mathf.Sign(dx);
                    var mag = Mathf.Abs(dx) - YawDeadzonePx;

                    // curve so small movements do less, big movements do more
                    mag = Mathf.Pow(mag, YawExponent);

                    _rotationTarget -= sign * mag * YawSensitivity;
                }

                // Rotate around X
                var dy = mouseMotion.Relative.Y;
                var pitch = RotationX.RotationDegrees.X - (dy * PitchSensitivity);
                RotationX.RotationDegrees = new Vector3(Mathf.Clamp(pitch, -45, 35), 0, 0);
            }
        }

        base._UnhandledInput(@event);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("rotate"))
            Input.SetMouseMode(Input.MouseModeEnum.Captured);

        if (Input.IsActionJustReleased("rotate"))
            Input.SetMouseMode(Input.MouseModeEnum.Visible);

        // Get Input dirs
        var inputDirection = Input.GetVector("left", "right", "up", "down");
        var rotationAxis = Input.GetAxis("rotate_left", "rotate_right");
        var zoomDirection = ((Input.IsActionJustReleased("zoom_out") ? 1 : 0) - (Input.IsActionJustReleased("zoom_in") ? 1 : 0));

        var moveDirection = (Transform.Basis * new Vector3(inputDirection.X, 0, inputDirection.Y)).Normalized();

        // set target
        _moveTarget += MoveSpeed * moveDirection;
        _rotationTarget += rotationAxis * RotationSpeed;
        _zoomTarget += zoomDirection * ZoomSpeed;
        _zoomTarget = Mathf.Clamp(_zoomTarget, _zoomMin, _zoomMax);

        // interp
        Position = Vector3Extensions.Lerp(Position, _moveTarget, 0.05f);

        var rotationY = Mathf.Lerp(RotationDegrees.Y, _rotationTarget, 0.1f);
        RotationDegrees = new Vector3(RotationDegrees.X, rotationY, RotationDegrees.Z);

        var cameraZ = Mathf.Lerp(Camera.Position.Z, _zoomTarget, 0.10f);
        Camera.Position = new Vector3(Camera.Position.X, Camera.Position.Y, cameraZ);

        base._Process(delta);
    }
}
