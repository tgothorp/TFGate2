using Godot;

namespace TFGate2.scripts.signals;

public partial class SignalBus : Node
{
    public static SignalBus Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance != null)
        {
            GD.PrintErr("Multiple instances of SignalBus detected!");
            QueueFree();

            return;
        }

        Instance = this;
    }
}