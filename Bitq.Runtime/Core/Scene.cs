using Newtonsoft.Json;

namespace Bitq.Core;

public class Scene : Object
{
    public List<GameObject> gameObjects = new List<GameObject>();
    
    public void OnLoad()
    {
        Physics.Initialize();
        foreach (var go in gameObjects)
        {
            go.OnLoad();
        }
    }

    public void LoadEditor()
    {
        foreach (var go in gameObjects)
        {
            go.LoadEditor();
        }
    }

    public void OnUpdate(double deltaTime)
    {
        foreach (var go in gameObjects)
        {
            go.OnUpdate(deltaTime);
        }
    }

    public void OnLateUpdate(double deltaTime)
    {
        foreach (var go in gameObjects)
        {
            go.OnLateUpdate(deltaTime);
        }
    }

    public void OnFixedUpdate(double deltaTime)
    {
        foreach (var go in gameObjects)
        {
            go.OnFixedUpdate(deltaTime);
        }
    }
    public void OnLateFixedUpdate(double deltaTime)
    {
        foreach (var go in gameObjects)
        {
            go.OnLateFixedUpdate(deltaTime);
        }
    }

    public void OnDraw(double deltaTime)
    {
        foreach (var go in gameObjects)
        {
            go.OnDraw(deltaTime);
        }
    }

    public void OnLateDraw(double deltaTime)
    {
        foreach (var go in gameObjects)
        {
            go.OnLateDraw(deltaTime);
        }
    }

    
    public void SetComponentsForEachGameObject()
    {
        foreach (var go in gameObjects)
        {
            foreach (var component in go.components)
            {
                component.GameObject = go;
            }

            go.Transform.GameObject = go;
        }
    }

    public override void OnDispose()
    {
        var copiedGameObjects = new List<GameObject>(gameObjects);
        foreach (var go in copiedGameObjects)
        { 
            Destroy(go);
        }
    }
}