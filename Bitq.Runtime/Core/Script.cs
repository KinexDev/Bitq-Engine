using System.Numerics;
using Bitq.Scripting;
using Bitq.Scripting.Api;
using Hexa.NET.ImGui;
using Newtonsoft.Json;

namespace Bitq.Core;

public class Script : Component
{
    public string ScriptPath = "";
    private bool hasUpdate;
    private bool hasFixedUpdate;
    [JsonIgnore] public VM vm;

    public static int Quit(IntPtr L)
    {
        Engine.Window.Close();
        return 0;
    }

    public override void OnLoad()
    {
        if (File.Exists(ScriptPath))
        {
            vm = new VM(log => { Debug.Log(log); });
            vm.RegisterUserdataType<LuaInputs>("Input");
            vm.RegisterUserdataType<LuaGameObject>("GameObject");
            vm.RegisterUserdataType<LuaVec3>("Vec3");
            vm.RegisterUserdataType<LuaVec2>("Vec2");
            vm.RegisterUserdataType<LuaQuat>("Quat");
            vm.RegisterUserdataType<LuaPhysics>();
            vm.RegisterUserdataType<LuaCharacterController>("CharacterController");
            vm.RegisterUserdataType<LuaAudioSource>("AudioSource");
            vm.RegisterUserdataType<LuaRaycastHit>("RayCastHit");
            vm.PushGlobalUserdata("Physics", new LuaPhysics());
            vm.PushGlobalFunction("quit", Quit);

            try
            {
                vm.DoString(File.ReadAllText(ScriptPath));

                hasUpdate = vm.GetGlobalType("_OnUpdate") == Luau.LuaType.Function;
                hasFixedUpdate = vm.GetGlobalType("_OnFixedUpdate") == Luau.LuaType.Function;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message, LogType.Error);
                Debug.Log(e.StackTrace, LogType.Error);
            }
        }
        else
        {
            Debug.Log($"Valid script not found in path {ScriptPath}", LogType.Error);
        }
    }

    public override void OnUpdate(double deltaTime)
    {
        if (hasUpdate)
        {
            try
            {
                vm.ExecuteFunction("_OnUpdate", 0, deltaTime);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message, LogType.Error);
                Debug.Log(e.StackTrace, LogType.Error);
            }
        }
    }

    public override void OnFixedUpdate(double deltaTime)
    {
        if (hasFixedUpdate)
        {
            try
            {
                vm.ExecuteFunction("_OnFixedUpdate", 0, deltaTime);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message, LogType.Error);
                Debug.Log(e.StackTrace, LogType.Error);
            }
        }
    }

    public override void OnDispose()
    {
        if (vm != null)
            vm.Dispose();
    }

    public override void OnInspectorGUI(float deltaTime)
    {
        ImGui.InputText("Script Path", ref ScriptPath, 50);
    }
}