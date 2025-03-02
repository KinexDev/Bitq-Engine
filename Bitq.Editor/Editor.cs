using System.Reflection;
using Bitq.Core;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using ImGuiWindow = Bitq.Rendering.ImGuiWindow;

namespace Bitq.Editor;

public static class Editor
{
    private static ImGuiController _controller;
    private static List<ImGuiWindow> _imGuiWindows = new();
    public static EditorCamera EditorCamera;

    public static void Main()
    {
        Engine.OnLoad += OnLoad;
        Engine.OnUpdate += OnUpdate;
        Engine.OnRender += OnRender;
        Engine.Editor = true;
        Engine.Main();
    }
    

    public static void OnLoad()
    {
        _controller = new ImGuiController(Engine.Gl, Engine.Window, Engine.Input);
        _controller.UpdateContexts();
        EditorCamera = new();
        Camera.main = EditorCamera;
        Assembly currentAssembly = Assembly.GetEntryAssembly();

        var imguiWindowTypes = currentAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ImGuiWindow)))
            .ToList();
        foreach (var type in imguiWindowTypes)
        {
            ImGuiWindow instance = (ImGuiWindow)Activator.CreateInstance(type);
            _imGuiWindows.Add(instance);
        }

        Engine.Editor = true;
    }
    
    public static void OnUpdate(double deltaTime)
    {
        EditorCamera.OnUpdate(deltaTime);
    }

    public static void OnRender(double deltaTime)
    {
        _controller.Update((float)deltaTime);

        ImGuizmo.SetDrawlist(ImGui.GetBackgroundDrawList());
        ImGuizmo.SetRect(0, 0, Engine.Window.Size.X, Engine.Window.Size.Y);

        ImGui.DockSpaceOverViewport(ImGui.GetWindowDockID(), ImGui.GetMainViewport(),
            ImGuiDockNodeFlags.PassthruCentralNode);
        
        foreach (var window in _imGuiWindows)
        {
            window.Draw((float)deltaTime);
        }
        _controller.Render();
    }
}