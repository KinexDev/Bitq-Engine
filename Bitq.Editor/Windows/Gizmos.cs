using System.Numerics;
using Bitq.Core;
using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Newtonsoft.Json;
using Silk.NET.Input;
using Camera = Bitq.Core.Camera;
using ImGuiWindow = Bitq.Rendering.ImGuiWindow;
using Object = Bitq.Core.Object;

namespace Bitq.Editor;

public class Gizmos : ImGuiWindow
{
    private static readonly Queue<Action> _actions = new();
    private static readonly object _lock = new();
    
    public ImGuizmoOperation Operation = ImGuizmoOperation.Translate;
    public ImGuizmoMode Mode = ImGuizmoMode.Local;
    public bool pasted;
    
    public override void Draw(float deltaTime)
    {
        lock (_lock)
        {
            while (_actions.Count > 0)
            {
                _actions.Dequeue().Invoke();
            }
        }
        
        var keyboard = Engine.Input.Keyboards[0];
        var mouse = Engine.Input.Mice[0];

        if (keyboard == null || mouse == null)
            return;
        
        if (!mouse.IsButtonPressed(MouseButton.Right) && Selection.selectedGameObject != null)
        {
            if (keyboard.IsKeyPressed(Key.W))
                Operation = ImGuizmoOperation.Translate;
            if (keyboard.IsKeyPressed(Key.E))
                Operation = ImGuizmoOperation.Rotate;
            if (keyboard.IsKeyPressed(Key.R))
                Operation = ImGuizmoOperation.Scale;

            var view = Camera.main.View;
            var projection = Camera.main.Projection;
            var uModelMatrix = Selection.selectedGameObject.Transform.GlobalMatrix;
            
            ImGuizmo.Manipulate(
                ref view.M11,
                ref projection.M11,
                Operation,
                Mode,
                ref uModelMatrix.M11
            );    
            
            if (ImGuizmo.IsUsing())
            {
                Matrix4x4 parentInverse = Matrix4x4.Identity;
                if (Selection.selectedGameObject.Transform.parentGo != null)
                    Matrix4x4.Invert(Selection.selectedGameObject.Transform.parentGo.Transform.GlobalMatrix, out parentInverse);

                Matrix4x4 newLocalMatrix = uModelMatrix * parentInverse;

                Matrix4x4.Decompose(newLocalMatrix, out Vector3 newScale, out Quaternion newRotation, out Vector3 newPosition);
                
                Selection.selectedGameObject.Transform.LocalPosition = newPosition;
                Selection.selectedGameObject.Transform.LocalRotation = newRotation;
                Selection.selectedGameObject.Transform.LocalScale = newScale;
            }
        }

        if (keyboard.IsKeyPressed(Key.Escape))
        {
            Selection.selectedGameObject = null;
        }

        if ((keyboard.IsKeyPressed(Key.Delete) || (keyboard.IsKeyPressed(Key.ControlLeft) && keyboard.IsKeyPressed(Key.D))) && Selection.selectedGameObject != null)
        {
            Object.Destroy(Selection.selectedGameObject);
            Selection.selectedGameObject = null;
        }
        
        if (keyboard.IsKeyPressed(Key.ControlLeft) && keyboard.IsKeyPressed(Key.C) && Selection.selectedGameObject != null)
        {
            var json = JsonConvert.SerializeObject(Selection.selectedGameObject, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
            Thread thread = new Thread(() => Clipboard.SetText(json));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(); 
            thread.Join();
        }

        if (keyboard.IsKeyPressed(Key.ControlLeft) && keyboard.IsKeyPressed(Key.V))
        {
            if (pasted)
                return;
            pasted = true;
            
            Thread thread = new Thread(() =>
            {
                var json = Clipboard.GetText();
                
                _actions.Enqueue((() =>
                {
                    try
                    {
                        var go = JsonConvert.DeserializeObject<GameObject>(json, new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.All
                        });

                        go.guid = Guid.NewGuid();
                        SceneManager.GetActiveScene().gameObjects.Add(go);
                        SceneManager.GetActiveScene().SetComponentsForEachGameObject();
                        foreach (var mesh in go.GetComponents<MeshRenderer>())
                        {
                            mesh.ReloadMesh();
                        }

                        go.LoadEditor();

                        Selection.selectedGameObject = go;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }));
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(); 
            thread.Join();
        }
        else
            pasted = false;
    }
}