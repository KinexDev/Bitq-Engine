using System.Numerics;
using Bitq.Core;

namespace Bitq.Scripting.Api;

public class LuaPhysics
{
    [LuauVariable]
    public double timeStep
    {
        get
        {
            return Physics.timeStep;
        }
        set
        {
            Physics.timeStep = (float)value;
        }
    }

    [LuauCallableFunction(true)]
    public static int RayCast(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var origin = vm.ReadUserdata<LuaVec3>(1);
        var direction = vm.ReadUserdata<LuaVec3>(2);
        var maxDistance = vm.ReadNumber(3);

        if (origin == null || direction == null || !maxDistance.HasValue)
        {
            vm.ThrowError("Invalid arguments for raycast!");
            return 0;
        }

        vm.PushValueToStack(1);
        return 1;
        
        //var hit = Physics.world.DynamicTree.RayCast(new JVector((float)origin.x, (float)origin.y, (float)origin.z), 
        //    new JVector((float)direction.x, (float)direction.y, (float)direction.z), (float)maxDistance.Value,
        //    null, null, out var proxy, out var normal, out float lambda);
            
        //var luaHit = new LuaRaycastHit(hit, new LuaVec3(normal.X, normal.Y, normal.Z), lambda);
        //vm.PushValueToStack(luaHit);
        //return 1;
    }

    [LuauCallableFunction]
    public static int SetGravity(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var newGravity = vm.ReadUserdata<LuaVec3>(1);
        
        if (newGravity == null)
        {
            vm.ThrowError("Invalid arguments for setting gravity!");
            return 0;
        }
        return 0;
    }
}