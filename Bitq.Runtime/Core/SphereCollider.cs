using System.Numerics;
using BulletSharp;
using Hexa.NET.ImGui;

namespace Bitq.Core;

public class SphereCollider : Collider
{
    public float Radius = 0.5f;
    
    public override void OnLoad()
    {
        if (shape == null)
            shape = new SphereShape(CalculateRadius());
    }

    public float CalculateRadius()
    {
        return (((Transform.Scale.X + Transform.Scale.Y + Transform.Scale.Z) / 3f) * Radius) * 2;
    }


    public override void OnInspectorGUI(float deltaTime)
    {
        ImGui.DragFloat("Radius", ref Radius);
    }
}