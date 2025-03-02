using System.Numerics;
using BulletSharp;
using Hexa.NET.ImGui;

namespace Bitq.Core;

public class CapsuleCollider : Collider
{
    public float Radius = 0.5f;
    public float Height = 1f;
    
    public override void OnLoad()
    {
        if (shape == null)
            shape = new CapsuleShape(CalculateSize(Radius), CalculateSize(Height));
    }

    public float CalculateSize(float size)
    {
        return (((Transform.Scale.X + Transform.Scale.Y + Transform.Scale.Z) / 3f) * size) * 2;
    }


    public override void OnInspectorGUI(float deltaTime)
    {
        ImGui.DragFloat("Radius", ref Radius);
        ImGui.DragFloat("Height", ref Height);
    }
}