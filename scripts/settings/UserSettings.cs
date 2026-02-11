    using Godot;
using System;

public partial class UserSettings : Node
{
    public static UserSettings Instance { get; private set; }

    public MiddleMouseAction MiddleMouseCameraAction { get; set; } = MiddleMouseAction.Rotate;

    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("Multiple instances of UserSettings detected!");
            QueueFree();

            return;
        }

        Instance = this;
    }

    public enum MiddleMouseAction
    {
        Rotate,
        Pan
    }
}
