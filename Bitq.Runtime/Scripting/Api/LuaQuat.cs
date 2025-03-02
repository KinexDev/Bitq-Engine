using System;
using System.Numerics;
using Bitq.Rendering;
using Bitq.Scripting;

namespace Bitq.Scripting.Api;

public class LuaQuat
{
    [LuauVariable] public double x { get; set; }
    [LuauVariable] public double y { get; set; }
    [LuauVariable] public double z { get; set; }
    [LuauVariable] public double w { get; set; }

    [LuauVariable]
    public double magnitude
    {
        get { return Math.Sqrt(x * x + y * y + z * z + w * w); }
    }

    public Quaternion quaternion => new((float)x, (float)y, (float)z, (float)w);

    public LuaQuat(double x, double y, double z, double w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
    
    [LuauMetamethod(LuauOperators.Mul)]
    public static int Mul(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);

        var q1 = vm.ReadUserdata<LuaQuat>(1);
        var q2 = vm.ReadUserdata<LuaQuat>(2);
        if (q1 == null || q2 == null)
        {
            vm.ThrowError("Supported operations with 'Mul' are Quat - Quat");
            return 0;
        }

        var res = q1.quaternion * q2.quaternion;
        vm.PushValueToStack(new LuaQuat(res.X, res.Y, res.Z, res.W));
        return 1;
    }

    [LuauMetamethod(LuauOperators.ToString)]
    public static int ToString(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadUserdata<LuaQuat>(1);
        vm.PushValueToStack(value == null ? "null" : $"({value.x}, {value.y}, {value.z}, {value.w})");
        return 1;
    }
    
    [LuauCallableFunction]
    public static int ToEuler(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var x = vm.ReadUserdata<LuaQuat>(1);
        var eulerAngleC = x.quaternion.ToEulerAngles();
        var eulerAngle = new LuaVec3(eulerAngleC.X, eulerAngleC.Y, eulerAngleC.Z);
        vm.PushValueToStack(eulerAngle);
        return 1;
    }
    
    [LuauCallableFunction]
    public static int New(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var x = vm.ReadNumber(1);
        var y = vm.ReadNumber(2);
        var z = vm.ReadNumber(3);
        var w = vm.ReadNumber(4);
        if (x.HasValue && y.HasValue && z.HasValue && w.HasValue)
        {
            var obj = new LuaQuat(x.Value, y.Value, z.Value, w.Value);
            vm.PushValueToStack(obj);
            return 1;
        }
        else
        {
            vm.ThrowError("Invalid quaternion values!");
        }

        return 0;
    }

    [LuauCallableFunction]
    public static int Euler(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);

        var pitch = vm.ReadNumber(1);
        var yaw = vm.ReadNumber(2);
        var roll = vm.ReadNumber(3);
        
        if (pitch.HasValue && yaw.HasValue && roll.HasValue)
        {
            var q = new Vector3((float)pitch, (float)yaw, (float)roll).ToQuaternion();
            var luaQuat = new LuaQuat(q.X, q.Y, q.Z, q.W);
            vm.PushValueToStack(luaQuat);
            return 1;
        }
        else
        {
            vm.ThrowError("Invalid Euler angles. Please provide valid numbers for pitch, yaw, and roll.");
        }

        return 0;
    }
}