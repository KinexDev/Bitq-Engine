using System.Reflection;
using Bitq.Core;
using Bitq.Rendering;
using Hexa.NET.ImGui;
using ImGuiWindow = Bitq.Rendering.ImGuiWindow;
using Object = Bitq.Core.Object;

namespace Bitq.Editor;

public class Inspector : ImGuiWindow
{
    public Inspector()
    {
        Assembly currentAssembly = Assembly.GetAssembly(typeof(Component));
        
        var components = currentAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Component)) && !t.IsDefined(typeof(ToBeInherited), false))
            .ToList();
        
        foreach (var component in components)
        {
            componentTypes.Add(component.Name, component);
        }
    }
    
    private int currentComponent;
    private Dictionary<string, Type> componentTypes = new Dictionary<string, Type>();
    
    public override void Draw(float deltaTime)
    {
        ImGui.Begin("Inspector");
        
        if (Selection.selectedGameObject != null)
        {
            var go = Selection.selectedGameObject;

            if (ImGui.CollapsingHeader("GameObject", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var goIsActive = go.isActive;
                ImGui.Checkbox("Active", ref goIsActive);
                go.isActive = goIsActive;
                ImGui.InputText("Name", ref go.name, 50);
                if (string.IsNullOrEmpty(go.name))
                {
                    go.name = "New GameObject";
                }
                
                if (ImGui.Button("Delete GameObject"))
                {
                    Object.Destroy(Selection.selectedGameObject);
                    Selection.selectedGameObject = null;
                    ImGui.EndChild();
                    return;
                }
            }

            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.DragFloat3("Position", ref go.Transform.LocalPosition);
                var euler = go.Transform.LocalRotation.ToEulerAngles();
                ImGui.DragFloat3("Rotation", ref euler);
                go.Transform.LocalRotation = euler.ToQuaternion();
                ImGui.DragFloat3("Scale", ref go.Transform.LocalScale);
            }

            ImGui.Separator();
            ImGui.Spacing();

            var components = new List<Component>(go.components);
            foreach (var component in components)
            {
                ImGui.PushID(component.guid.ToString());
                if (ImGui.CollapsingHeader(component.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    component.OnInspectorGUI(deltaTime);
                }
                
                if (ImGui.Button("Remove Component"))
                {
                    Object.Destroy(component);
                }
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.PopID();
            }

            if (ImGui.Button("Add Component"))
            {
                ImGui.OpenPopup("AddComponent");
            }
            
            if (ImGui.BeginPopup("AddComponent"))
            {
                var keys = componentTypes.Keys.ToArray();
                ImGui.Combo("Components", ref currentComponent, keys, keys.Length);
                
                if (ImGui.Button("Add"))
                {
                    var type = componentTypes[keys[currentComponent]];
                    if (type == null)
                        Debug.Log("Component not found!", LogType.Error);
                    else
                    {
                        if (type.IsDefined(typeof(RequireComponent), true))
                        {
                            var requiredComponent = ((RequireComponent)type.GetCustomAttribute(typeof(RequireComponent), true)).type;

                            if (!go.HasComponent(type) && go.HasComponent(requiredComponent))
                                go.AddComponent(type);
                            else
                                Debug.Log($"GameObject does not have required component {requiredComponent.Name}", LogType.Error);
                        }
                        else
                        { 
                            go.AddComponent(type);
                        }
                    }
                    
                    ImGui.CloseCurrentPopup();
                }
                
                ImGui.EndPopup();
            }
        }
        
        ImGui.End();
    }
}