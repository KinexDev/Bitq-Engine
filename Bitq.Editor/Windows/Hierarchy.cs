using System;
using Bitq.Core;
using Hexa.NET.ImGui;
using ImGuiWindow = Bitq.Rendering.ImGuiWindow;

namespace Bitq.Editor
{
    public class Hierarchy : ImGuiWindow
    {
        public string payloadName = "Hierarchy";

        public Hierarchy()
        {
            objects = Enum.GetNames(typeof(PrimitiveType));
        }

        public int currentObj;
        private string[] objects;

        public override void Draw(float deltaTime)
        {
            ImGui.Begin("Hierarchy", ImGuiWindowFlags.MenuBar);

            ImGui.BeginMenuBar();

            ImGui.Text(" Scene");

            ImGui.SameLine(ImGui.GetWindowWidth() - 70);

            if (ImGui.Button("Create"))
            {
                ImGui.OpenPopup("Create");
            }

            if (ImGui.BeginPopup("Create"))
            {
                ImGui.Combo("Type", ref currentObj, objects, objects.Length);

                if (ImGui.Button("Create"))
                {
                    var type = (PrimitiveType)Enum.Parse(typeof(PrimitiveType), objects[currentObj]);
                    var go = GameObject.CreatePrimitive(type);
                    Selection.selectedGameObject = go;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            ImGui.EndMenuBar();

            if (ImGui.TreeNode("Root"))
            {
                AcceptPayload(null);
                
                foreach (var gameObject in SceneManager.GetActiveScene().gameObjects)
                {
                    if (gameObject.Transform.parent != Guid.Empty)
                    {
                        continue;
                    }
                    
                    DrawGo(gameObject);
                }
                
                ImGui.TreePop();
            }

            ImGui.End();
        }

        public void DrawGo(GameObject go)
        {
            ImGui.PushID(go.guid.ToString());

            // Add a custom flag for the selected gameObject
            var treeNodeFlags = go == Selection.selectedGameObject
                ? ImGuiTreeNodeFlags.Selected
                : ImGuiTreeNodeFlags.None;

            if (ImGui.TreeNodeEx(go.name, treeNodeFlags))
            {
                var list = new List<Guid>(go.Transform.children);
                foreach (var obj in list)
                {
                    DrawGo(GameObject.Find(obj));
                }

                ImGui.TreePop();
            }
            else
            {
                if (ImGui.IsItemClicked())
                {
                    Selection.selectedGameObject = go;
                }

                if (ImGui.BeginDragDropSource())
                {
                    unsafe
                    {
                        var str = go.guid.ToString();
                        fixed (char* strPtr = str)
                        {
                            var payloadSize = new UIntPtr((uint)((str.Length + 1) * sizeof(char)));
                            ImGui.SetDragDropPayload(payloadName, strPtr, payloadSize);
                        }
                        ImGui.Text(go.name);
                        ImGui.EndDragDropSource();
                    }
                }

                AcceptPayload(go);   
            }

            if (go.Transform.children.Count == 0)
            {
                if (ImGui.IsItemClicked())
                {
                    Selection.selectedGameObject = go;
                }

                if (ImGui.BeginDragDropSource())
                {
                    unsafe
                    {
                        var str = go.guid.ToString();
                        fixed (char* strPtr = str)
                        {
                            var payloadSize = new UIntPtr((uint)((str.Length + 1) * sizeof(char)));
                            ImGui.SetDragDropPayload(payloadName, strPtr, payloadSize);
                        }
                        ImGui.Text(go.name);
                        ImGui.EndDragDropSource();
                    }
                }

                AcceptPayload(go);   
            }
            
            ImGui.PopID();
        }


        public void AcceptPayload(GameObject parentGo)
        {
            if (ImGui.BeginDragDropTarget())
            {
                var ptr = ImGui.AcceptDragDropPayload(payloadName);

                if (!ptr.IsNull)
                {
                    unsafe
                    {
                        char* payloadPtr = (char*)ptr.Data;
                        int charCount = ptr.DataSize / sizeof(char) - 1;
                        string receivedString = new string(payloadPtr, 0, charCount);

                        var go = GameObject.Find(new Guid(receivedString));

                        if (parentGo == null)
                            go.Transform.SetParent(null);
                        else
                            go.Transform.SetParent(parentGo.Transform);
                    }
                }

                ImGui.EndDragDropTarget();
            }
        }
    }
}