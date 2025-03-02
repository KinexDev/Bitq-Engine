using Bitq.Runtime.Core;

namespace Bitq.Scripting.Api;

public class LuaAudioSource
{
    private AudioSource source;
    public LuaAudioSource(LuaGameObject go)
    {
        source = go.go.GetComponent<AudioSource>();
    }
    
    [LuauCallableFunction]
    public static int Get(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadUserdata<LuaGameObject>(1);
        if (value != null)
        {
            var characterController = new LuaAudioSource(value);
            vm.PushValueToStack(characterController);
            return 1;
        }
        return 0;
    }

    [LuauCallableFunction]
    public static int Play(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadUserdata<LuaAudioSource>(1);
        if (value != null)
            value.source.Play();
        return 0;
    }
    
    [LuauCallableFunction]
    public static int Stop(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadUserdata<LuaAudioSource>(1);
        if (value != null)
            value.source.Play();
        return 0;
    }

    [LuauCallableFunction]
    public static int Replay(IntPtr L)
    {
        var vm = VM.GetVMInstance(L);
        var value = vm.ReadUserdata<LuaAudioSource>(1);
        if (value != null)
        {
            value.source.Stop();
            value.source.Play();
        }
        return 0;
    }

}