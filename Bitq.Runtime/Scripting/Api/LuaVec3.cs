using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Bitq.Scripting;

namespace Bitq.Scripting.Api;

public class LuaVec3
{
    [LuauVariable] public double x { get; set; }
    [LuauVariable] public double y { get; set; }
    [LuauVariable] public double z { get; set; }

    [LuauVariable]
    public double magnitude
    {
        get
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }
    }

    public LuaVec3(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    [LuauMetamethod(LuauOperators.Add)]
    public static int Add(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        
        var x = vm.ReadUserdata<LuaVec3>(1);
        var y = vm.ReadUserdata<LuaVec3>(2);

        if (x == null || y == null)
        {
            vm.ThrowError("Supported operations with 'Add' are Vec3 + Vec3");
            return 0;
        }
    
        var result = new LuaVec3(x.x + y.x, x.y + y.y, x.z + y.z);
        vm.PushValueToStack(result);
        return 1;
    }
    
    [LuauMetamethod(LuauOperators.Sub)]
    public static int Sub(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        
        var x = vm.ReadUserdata<LuaVec3>(1);
        var y = vm.ReadUserdata<LuaVec3>(2);

        if (x == null || y == null)
        {
            vm.ThrowError("Supported operations with 'Sub' are Vec3 - Vec3");
            return 0;
        }
    
        var result = new LuaVec3(x.x - y.x, x.y - y.y, x.z - y.z);
        vm.PushValueToStack(result);
        return 1;
    }

    [LuauMetamethod(LuauOperators.Mul)]
    public static int Mul(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var x = vm.ReadUserdata<LuaVec3>(1);
        var y = vm.ReadNumber(2);

        if (x == null || y == null)
        {
            var yq = vm.ReadUserdata<LuaQuat>(2);
            if (x != null && yq != null)
            {
                var vec3 = Vector3.Transform(new Vector3((float)x.x, (float)x.y, (float)x.z), yq.quaternion);
                var luaVec3 = new LuaVec3(vec3.X, vec3.Y, vec3.Z);
                vm.PushValueToStack(luaVec3);
                return 1;
            }
            else
            {
                vm.ThrowError("Supported operations with 'Mul' are Vec3 * number");
                return 0;   
            }
        }
        
        var result = new LuaVec3(x.x * y.Value, x.y * y.Value, x.z * y.Value);
        vm.PushValueToStack(result);

        return 1;
    }
    
    [LuauMetamethod(LuauOperators.Div)]
    public static int Div(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var x = vm.ReadUserdata<LuaVec3>(1);
        var y = vm.ReadNumber(2);

        if (x == null || y == null)
        {
            vm.ThrowError("Supported operations with 'Div' are Vec3 / number");
            return 0;
        }
        
        var result = new LuaVec3(x.x / y.Value, x.y / y.Value, x.z / y.Value);
        vm.PushValueToStack(result);

        return 1;
    }
    
    [LuauMetamethod(LuauOperators.ToString)]
    public static int ToString(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadUserdata<LuaVec3>(1);
        vm.PushValueToStack(value == null ? "null" : $"({value.x}, {value.y}, {value.z})");
        return 1;
    }


    [LuauCallableFunction]
    public static int New(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var x = vm.ReadNumber(1);
        var y = vm.ReadNumber(2);
        var z = vm.ReadNumber(3);
        if (x.HasValue && y.HasValue && z.HasValue)
        {
            var obj = new LuaVec3(x.Value, y.Value, z.Value);
            vm.PushValueToStack(obj);
            return 1;
        }
        else
        {
            vm.ThrowError("Not valid numbers!");
        }

        return 0;
    }
    
    [LuauCallableFunction(false)]
    public static int Normalize(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var x = vm.ReadUserdata<LuaVec3>(1);
        if (x != null)
        {
            if (x.magnitude == 0)
            {
                vm.PushValueToStack(new LuaVec3(0, 0, 0));
                return 1;
            }
        
            var result = new LuaVec3(x.x / x.magnitude, x.y / x.magnitude, x.z / x.magnitude);
            vm.PushValueToStack(result);
            return 1;
        }

        return 0;
    }
    
    [LuauCallableFunction]
    public static int Dist(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var x = vm.ReadUserdata<LuaVec3>(1);
        var y = vm.ReadUserdata<LuaVec3>(2);
        if (x != null || y != null)
        {
            var res = Vector3.Distance(new Vector3((float)x.x, (float)x.y, (float)x.z),new Vector3((float)y.x, (float)y.y, (float)y.z));
            vm.PushValueToStack(res);
            return 1;
        }

        return 0;
    }
}
