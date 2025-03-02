using System.Diagnostics;
using System.Numerics;
using Bitq.Core;
using Hexa.NET.ImGui;
using Debug = Bitq.Core.Debug;
using ImGuiWindow = Bitq.Rendering.ImGuiWindow;

namespace Bitq.Editor
{
    public class MenuBar : ImGuiWindow
    {
        private Process gameProcess;
        
        public override void Draw(float deltaTime)
        {
            ImGui.BeginMainMenuBar();

            if (ImGui.Button("Save"))
            {
                ImGui.OpenPopup("Save");
            }
            
            if (ImGui.BeginPopup("Save"))
            {
                ImGui.EndPopup();
            }
            
            ImGui.SameLine(ImGui.GetWindowWidth() - 90);

            if (ImGui.Button("Play"))
            {
                File.WriteAllText("Assets/Scene.scene",SceneManager.SaveScene());
                
                var psi = new ProcessStartInfo()
                {
                    FileName = "Bitq.Runtime.exe",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                gameProcess = Process.Start(psi);

                gameProcess.BeginOutputReadLine();
                gameProcess.BeginErrorReadLine();

                gameProcess.OutputDataReceived += (sender, args) =>
                {
                    if (string.IsNullOrEmpty(args.Data))
                        return;
                    Debug.LogRaw(args.Data);
                };

                gameProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (string.IsNullOrEmpty(args.Data))
                        return;
                    Debug.Log(args.Data, LogType.Error);
                };   
            }
            
            
            ImGui.SameLine(ImGui.GetWindowWidth() - 50);

            if (ImGui.Button("Stop"))
            {
                if (gameProcess != null)
                {
                    gameProcess.Kill();
                    gameProcess.Dispose();
                }
                gameProcess = null;
            }

            ImGui.EndMainMenuBar();
        }
    }
}