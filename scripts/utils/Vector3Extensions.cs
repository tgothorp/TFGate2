using Godot;

namespace TFGate2.scripts.utils;

public class Vector3Extensions
{
    public static Vector3 Lerp(Vector3 first, Vector3 second, float amount)
    {
        var retX = Mathf.Lerp(first.X, second.X, amount);
        var retY = Mathf.Lerp(first.Y, second.Y, amount);
        var retZ = Mathf.Lerp(first.Z, second.Z, amount);
        
        return new Vector3(retX, retY, retZ);
    }
}