using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Bitq.Core;
using Bitq.Scripting;

namespace Bitq.Scripting.Api;

public class LuaGameObject
{
    public GameObject go;
    [LuauVariable] public string name {
        get
        {
            return go.name;
        }
    }
    
    [LuauVariable] public bool active {
        get
        {
            return go.isActive;
        }
        set
        {
            go.isActive = value;
        }
    }

    [LuauVariable]
    public LuaVec3 position
    {
        get
        {
            return new LuaVec3(go.Transform.Position.X, go.Transform.Position.Y, go.Transform.Position.Z);
        }
        set
        {
            go.Transform.Position = new Vector3((float)value.x, (float)value.y, (float)value.z);
        }
    }
    
    [LuauVariable]
    public LuaQuat rotation
    {
        get
        {
            return new LuaQuat(go.Transform.Rotation.X, go.Transform.Rotation.Y, go.Transform.Rotation.Z, go.Transform.Rotation.W);
        }
        set
        {
            go.Transform.Rotation = new Quaternion((float)value.x, (float)value.y, (float)value.z, (float)value.w);
        }
    }
        
    [LuauVariable]
    public LuaVec3 scale
    {
        get
        {
            return new LuaVec3(go.Transform.Scale.X, go.Transform.Scale.Y, go.Transform.Scale.Z);
        }
    }
    
    [LuauVariable]
    public LuaVec3 localPosition
    {
        get
        {
            return new LuaVec3(go.Transform.LocalPosition.X, go.Transform.LocalPosition.Y, go.Transform.LocalPosition.Z);
        }
        set
        {
            go.Transform.LocalPosition = new Vector3((float)value.x, (float)value.y, (float)value.z);
        }
    }
    
    [LuauVariable]
    public LuaQuat localRotation
    {
        get
        {
            return new LuaQuat(go.Transform.LocalRotation.X, go.Transform.LocalRotation.Y, go.Transform.LocalRotation.Z, go.Transform.LocalRotation.W);
        }
        set
        {
            go.Transform.LocalRotation = new Quaternion((float)value.x, (float)value.y, (float)value.z, (float)value.w);
        }
    }
        
    [LuauVariable]
    public LuaVec3 localScale
    {
        get
        {
            return new LuaVec3(go.Transform.LocalScale.X, go.Transform.LocalScale.Y, go.Transform.LocalScale.Z);
        }
        set
        {
            go.Transform.LocalScale = new Vector3((float)value.x, (float)value.y, (float)value.z);
        }
    }

    public LuaGameObject(string name)
    {
        go = GameObject.Find(name);
    }

    public LuaGameObject(GameObject go)
    {
        this.go = go;
    }

    [LuauMetamethod(LuauOperators.ToString)]
    public static int ToString(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadUserdata<LuaGameObject>(1);
        vm.PushValueToStack(value == null ? "null" : value.ToString());
        return 1;
    }
    
    [LuauCallableFunction]
    public static int Get(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadString(1);
        if (value != null)
        {
            var obj = new LuaGameObject(value);
            vm.PushValueToStack(obj);
            return 1;
        }
        return 0;
    }
}