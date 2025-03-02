using System;
using System.Collections;
using System.Collections.Generic;
using Bitq.Scripting;
using Silk.NET.Input;

namespace Bitq.Scripting.Api;

public class LuaInputs
{
    [LuauCallableFunction]
    public static int GetKey(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadString(1);
        var keyboard = Engine.Input.Keyboards[0];
        
        if (value != null && keyboard != null && Enum.TryParse(typeof(Key), value, out var result))
        {
            var obj = keyboard.IsKeyPressed((Key)result);
            vm.PushValueToStack(obj);
            return 1;
        }
        
        vm.PushValueToStack(false);
        return 1;
    }
    
    [LuauCallableFunction]
    public static int LockCursor(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadBoolean(1);

        if (value.HasValue)
        {
            if (value.Value)
                Engine.Mouse.Cursor.CursorMode = CursorMode.Raw;
            else
                Engine.Mouse.Cursor.CursorMode = CursorMode.Normal;
        }
        return 0;
    }

    [LuauCallableFunction]
    public static int GetMousePosition(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        vm.PushValueToStack(new LuaVec2(Engine.Mouse.Position.X, Engine.Mouse.Position.Y));
        return 1;
    }
}
