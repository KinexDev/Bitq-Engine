using BulletSharp;

namespace Bitq.Core;

[ToBeInherited]
public class Collider : Component
{
    public CollisionShape shape;

    public override void OnDispose()
    {
        if (shape != null)
            shape.Dispose();
    }
}