using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Bitq.Scripting;

namespace Bitq.Scripting.Api;

public class LuaVec2
{
    [LuauVariable] public double x { get; set; }
    [LuauVariable] public double y { get; set; }
    
    [LuauVariable]
    public double magnitude
    {
        get
        {
            return Math.Sqrt(x * x + y * y);
        }
    }

    public Vector2 vector => new ((float)x, (float)y);

    public LuaVec2(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    [LuauMetamethod(LuauOperators.Add)]
    public static int Add(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        
        var x = vm.ReadUserdata<LuaVec2>(1);
        var y = vm.ReadUserdata<LuaVec2>(2);

        if (x == null || y == null)
        {
            vm.ThrowError("Supported operations with 'Add' are Vec3 + Vec3");
            return 0;
        }
    
        var result = new LuaVec2(x.x + y.x, x.y + y.y);
        vm.PushValueToStack(result);
        return 1;
    }
    
    [LuauMetamethod(LuauOperators.Sub)]
    public static int Sub(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        
        var x = vm.ReadUserdata<LuaVec2>(1);
        var y = vm.ReadUserdata<LuaVec2>(2);

        if (x == null || y == null)
        {
            vm.ThrowError("Supported operations with 'Sub' are Vec3 - Vec3");
            return 0;
        }
    
        var result = new LuaVec2(x.x - y.x, x.y - y.y);
        vm.PushValueToStack(result);
        return 1;
    }

    [LuauMetamethod(LuauOperators.Mul)]
    public static int Mul(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var x = vm.ReadUserdata<LuaVec2>(1);
        var y = vm.ReadNumber(2);

        if (x == null || y == null)
        {
            vm.ThrowError("Supported operations with 'Mul' are Vec3 * number");
            return 0;
        }
        
        var result = new LuaVec2(x.x * y.Value, x.y * y.Value);
        vm.PushValueToStack(result);

        return 1;
    }
    
    [LuauMetamethod(LuauOperators.Div)]
    public static int Div(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var x = vm.ReadUserdata<LuaVec2>(1);
        var y = vm.ReadNumber(2);

        if (x == null || y == null)
        {
            vm.ThrowError("Supported operations with 'Div' are Vec3 / number");
            return 0;
        }
        
        var result = new LuaVec2(x.x / y.Value, x.y / y.Value);
        vm.PushValueToStack(result);

        return 1;
    }
    
    [LuauMetamethod(LuauOperators.ToString)]
    public static int ToString(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadUserdata<LuaVec2>(1);
        vm.PushValueToStack(value == null ? "null" : $"({value.x}, {value.y})");
        return 1;
    }


    [LuauCallableFunction]
    public static int New(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var x = vm.ReadNumber(1);
        var y = vm.ReadNumber(2);
        if (x.HasValue && y.HasValue)
        {
            var obj = new LuaVec2(x.Value, y.Value);
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
        var x = vm.ReadUserdata<LuaVec2>(1);
        if (x != null)
        {
            if (x.magnitude == 0)
            {
                vm.PushValueToStack(new LuaVec2(0, 0));
                return 1;
            }
        
            var result = new LuaVec2(x.x / x.magnitude, x.y / x.magnitude);
            vm.PushValueToStack(result);
            return 1;
        }

        return 0;
    }
}
