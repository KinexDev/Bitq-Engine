using System.Numerics;
using Bitq.Core;

namespace Bitq.Scripting.Api;

public class LuaCharacterController
{
    private CharacterController characterController;
    
    public LuaCharacterController(LuaGameObject go)
    {
        characterController = go.go.GetComponent<CharacterController>();
    }

    [LuauVariable] public bool IsGrounded {
        get
        {
            return characterController.IsGrounded;
        }
    }

    [LuauCallableFunction]
    public static int Get(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadUserdata<LuaGameObject>(1);
        if (value != null)
        {
            var characterController = new LuaCharacterController(value);
            vm.PushValueToStack(characterController);
            return 1;
        }
        return 0;
    }


    [LuauCallableFunction(false)]
    public static int Move(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var controller = vm.ReadUserdata<LuaCharacterController>(1);
        var dir = vm.ReadUserdata<LuaVec3>(2);
        var dt = vm.ReadNumber(3);

        if (controller == null || dir == null || !dt.HasValue)
            return 0;

        controller.characterController.Move(new Vector3((float)dir.x, (float)dir.y, (float)dir.z), (float)dt);
        return 0;
    }
    
    [LuauCallableFunction(false)]
    public static int Warp(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var controller = vm.ReadUserdata<LuaCharacterController>(1);
        var dir = vm.ReadUserdata<LuaVec3>(2);

        if (controller == null || dir == null)
            return 0;

        controller.characterController.Warp(new Vector3((float)dir.x, (float)dir.y, (float)dir.z));
        return 0;
    }
        
    [LuauCallableFunction(false)]
    public static int Jump(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var controller = vm.ReadUserdata<LuaCharacterController>(1);

        if (controller == null)
            return 0;

        controller.characterController.Jump();
        return 0;
    }
}