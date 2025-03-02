using Hexa.NET.ImGui;

namespace Bitq.Core;

public class StaticBody : Rigidbody
{
    public override void OnLoad()
    {
        mass = 0;
        base.OnLoad();
    }

    public override void OnInspectorGUI(float deltaTime)
    {
        ImGui.Text("Body Settings");
        ImGui.DragFloat("Resitution", ref Resitution, 0.1f);
        ImGui.DragFloat("Friction", ref Friction, 0.1f);
    }
}