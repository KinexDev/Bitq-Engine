using Bitq.Rendering;

namespace Bitq.Core;

public enum PrimitiveType
{
    Empty = 0,
    Cube = 1,
    Sphere = 2
}

public class GameObject : Object
{
    private bool active = true;

    public bool isActive
    {
        get => active;
        set
        {
            active = value;
            OnActiveChanged(active);
        }
    }
    public string name;
    public Transform Transform = new();
    public List<Component> components = new();

    public static GameObject Create(string name)
    {
        var go = new GameObject();
        go.name = name;
        go.Transform.GameObject = go;
        SceneManager.GetActiveScene().gameObjects.Add(go);
        return go;
    }

    public static GameObject CreatePrimitive(PrimitiveType type)
    {
        GameObject go = Create(type.ToString());
        switch (type)
        {
            case PrimitiveType.Cube:
                go.AddComponent<MeshRenderer>();
                go.AddComponent<BoxCollider>();
                break;
            case PrimitiveType.Sphere:
                var meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.meshPath = "Assets/Models/sphere.fbx";
                meshRenderer.ReloadMesh();
                go.AddComponent<SphereCollider>();
                break;
        }

        return go;
    }

    public static GameObject Find(string name)
    {
        var currentScene = SceneManager.GetActiveScene();

        foreach (var go in currentScene.gameObjects)
        {
            if (go.name == name)
                return go;
        }

        return null;
    }
    
    public static GameObject Find(Guid guid)
    {
        var currentScene = SceneManager.GetActiveScene();

        foreach (var go in currentScene.gameObjects)
        {
            if (go.guid.ToString() == guid.ToString())
                return go;
        }

        return null;
    }
    
    public void OnLoad()
    {
        if (!isActive)
            return;

        foreach (var component in components)
            component.OnLoad();
    }

    public void OnActiveChanged(bool active)
    {
        foreach (var component in components)
            component.OnActiveChanged(active);
    }
    
    public void OnUpdate(double deltaTime)
    {
        if (!isActive)
            return;

        foreach (var component in components)
            component.OnUpdate(deltaTime);
    }
    
    public void OnLateUpdate(double deltaTime)
    {
        if (!isActive)
            return;

        foreach (var component in components)
            component.OnLateUpdate(deltaTime);
    }

    public void OnFixedUpdate(double deltaTime)
    {
        if (!isActive)
            return;

        foreach (var component in components)
            component.OnFixedUpdate(deltaTime);
    }
    
    public void OnLateFixedUpdate(double deltaTime)
    {
        if (!isActive)
            return;

        foreach (var component in components)
            component.OnLateFixedUpdate(deltaTime);
    }

    public void OnDraw(double deltaTime)
    {
        if (!isActive)
            return;
        
        foreach (var component in components)
            component.OnDraw(deltaTime);
    }

    public void OnLateDraw(double deltaTime)
    {
        if (!isActive)
            return;
        
        foreach (var component in components)
            component.OnLateDraw(deltaTime);
    }
    
    public T AddComponent<T>() where T : Component
    {
        var component = (Component)Activator.CreateInstance<T>();
        component.GameObject = this;
        components.Add(component);
        if (!Engine.Editor)
            component.OnLoad();
        return (T)component;
    }
    
    public Component AddComponent(Type type)
    {
        if (!type.IsSubclassOf(typeof(Component)))
        {
            Debug.Log($"type {type.Name} is not a component");
            return null;
        }
        
        var component = (Component)Activator.CreateInstance(type);
        component.GameObject = this;
        components.Add(component);
        if (!Engine.Editor)
            component.OnLoad();
        return component;
    }
    
    public T GetComponent<T>() where T : Component
    {
        var type = typeof(T);
        return (T)components.Find(x => x.GetType().FullName == type.FullName || x.GetType().IsSubclassOf(type));
    }

    public List<T> GetComponents<T>() where T : Component
    {
        var type = typeof(T);
        return components.FindAll(x => x.GetType().FullName == type.FullName || x.GetType().IsSubclassOf(type)).Cast<T>().ToList();
    }

    public Component GetComponent(Type type)
    {
        return components.Find(x => x.GetType().FullName == type.FullName || x.GetType().IsSubclassOf(type));
    }

    public bool HasComponent(Type type)
    {
        return components.Find(x => x.GetType().FullName == type.FullName || x.GetType().IsSubclassOf(type)) != null;
    }

    public override void OnDispose()
    {
        Transform.SetParent(null);
        
        foreach (var component in components)
        {
            component.OnDispose();
        }

        var children = new List<Guid>(Transform.children);
        
        foreach (var child in children)
        {
            GameObject.Find(child).OnDispose();
        }
    }

    public void LoadEditor()
    {
        foreach (var component in components)
        {
            if (component.GetType().IsDefined(typeof(ReloadEditor), true))
                component.OnLoad();
        }
    }
}