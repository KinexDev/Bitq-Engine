using System.Drawing;
using System.Numerics;
using Bitq.Core;
using Hexa.NET.ImGui;
using ImGuiWindow = Bitq.Rendering.ImGuiWindow;

namespace Bitq.Editor;

public class Console : ImGuiWindow
{
    public struct ConsoleLog
    {
        public string message;
        public LogType type;
    }
    
    public Console()
    {
        Debug.OnLog += NewLog;
    }
    
    public ConsoleLog[] Logs = new ConsoleLog[20];

    public Dictionary<LogType, Color> colors = new Dictionary<LogType, Color>()
    {
        { LogType.Error, Color.Red },
        { LogType.Warning, Color.Yellow },
        { LogType.Log, Color.White }
    };
    public void NewLog(string message, LogType type)
    {
        var log = new ConsoleLog();
        log.message = message;
        log.type = type;
        
        Array.Resize(ref Logs, Logs.Length);
        for (int i = Logs.Length - 1; i > 0; i--)
        {
            Logs[i] = Logs[i - 1];
        }

        Logs[0] = log;
    }
    
    public override void Draw(float deltaTime)
    {
        ImGui.Begin("Console", ImGuiWindowFlags.MenuBar);
    
        ImGui.BeginMenuBar();
        ImGui.Text("Logs");
    
        ImGui.SameLine(ImGui.GetWindowWidth() - 70);
        if (ImGui.Button("Clear"))
        {
            Logs = new ConsoleLog[20];
        }
        ImGui.EndMenuBar();

        for (int i = Logs.Length - 1; i >= 0; i--)
        {
            var log = Logs[i];
            if (string.IsNullOrEmpty(log.message)) 
            {
                continue;
            }
            
            Color color = colors[log.type];
            
            ImGui.TextColored(new System.Numerics.Vector4(
                color.R / 255f, 
                color.G / 255f, 
                color.B / 255f, 
                color.A / 255f
            ), log.message);
        }
        ImGui.End();
        base.Draw(deltaTime);
    }

}