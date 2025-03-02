using System.Numerics;
using Bitq.Rendering;
using Hexa.NET.ImGui;
using Newtonsoft.Json;

namespace Bitq.Core;

public class Camera : Component
{
    [JsonIgnore] public static Camera main;
    public float Fov = 90f;
    public float Near = 0.1f;
    public float Far = 10000f;

    [JsonIgnore] public virtual Matrix4x4 View
    {
        get
        {
            return Matrix4x4.CreateLookAt(GameObject.Transform.Position,
                GameObject.Transform.Position + GameObject.Transform.Rotation.Forward(), GameObject.Transform.Rotation.Up());
        }
    }

    [JsonIgnore] public virtual Matrix4x4 Projection 
    {
        get
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), (float)Engine
                .Window.FramebufferSize.X / Engine.Window.FramebufferSize.Y, Near, Far);
        }
    }

    public override void OnLoad()
    {
        main = this;
    }

    public override void OnActiveChanged(bool active)
    {
        if (Engine.Editor)
            return;
        
        if (active)
        {
            main = this;
        }
        else
        {
            main = null;
        }
    }

    public override void OnInspectorGUI(float deltaTime)
    {
        ImGui.DragFloat("Fov", ref Fov);
        ImGui.DragFloat("Near", ref Near);
        ImGui.DragFloat("Far", ref Far);
    }
}