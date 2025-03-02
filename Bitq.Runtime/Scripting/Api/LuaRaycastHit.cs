namespace Bitq.Scripting.Api;

public class LuaRaycastHit
{
    [LuauVariable] public bool hit { get; }
    [LuauVariable] public LuaVec3 normal { get; }
    [LuauVariable] public double distance { get; }

    public LuaRaycastHit(bool hit, LuaVec3 normal, double distance)
    {
        this.hit = hit;
        this.normal = normal;
        this.distance = distance;
    }
}