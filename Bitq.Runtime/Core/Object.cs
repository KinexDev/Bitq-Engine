namespace Bitq.Core;

public class Object : IDisposable
{
    public Guid guid;

    public Object()
    {
        if (guid == Guid.Empty)
            guid = Guid.NewGuid();
    }
    
    public void Dispose()
    {
        OnDispose();
    }

    public static void Destroy(Object obj)
    {
        obj.Dispose();

        if (obj is GameObject go)
        {
            SceneManager.GetActiveScene().gameObjects.Remove(go);
        } else if (obj is Component component)
        {
            component.GameObject.components.Remove(component);
        }
    }

    public virtual void OnDispose()
    {
        
    }
}