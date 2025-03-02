using System.Numerics;
using Bitq.Rendering;
using BulletSharp;
using Hexa.NET.ImGui;

namespace Bitq.Core;

public class BoxCollider : Collider
{
    public Vector3 Size = Vector3.One;
    
    public override void OnLoad()
    {
        if (shape == null)
            shape = new BoxShape(CalculateSize());
    }
    
    public BulletSharp.Math.Vector3 CalculateSize()
    {
        return Transform.Scale.ToBtVec3();
    }

    public override void OnInspectorGUI(float deltaTime)
    {
        ImGui.DragFloat3("Box Size", ref Size);
    }
}