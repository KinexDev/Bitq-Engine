using Bitq.Rendering;
using Newtonsoft.Json;

namespace Bitq.Core;

public class Component : Object
{
    [JsonIgnore] public GameObject GameObject;
    [JsonIgnore] public Transform Transform => GameObject.Transform;
    public virtual void OnLoad()
    {
        
    }

    public virtual void OnUpdate(double deltaTime)
    {
        
    }
    
    public virtual void OnActiveChanged(bool active)
    {
    }

    public virtual void OnLateUpdate(double deltaTime)
    {
        
    }
    
    public virtual void OnFixedUpdate(double deltaTime)
    {
        
    }
    
    public virtual void OnLateFixedUpdate(double deltaTime)
    {
        
    }
    
    public virtual void OnDraw(double deltaTime)
    {
        
    }
    
    public virtual void OnLateDraw(double deltaTime)
    {
        
    }
    
    public virtual void OnInspectorGUI(float deltaTime)
    {
        
    }
}