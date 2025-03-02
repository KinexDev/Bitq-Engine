using System.Numerics;
using Bitq.Core;
using Newtonsoft.Json;

namespace Bitq.Core;
public class Transform
{
    [JsonIgnore] public GameObject GameObject;
    public Guid parent = Guid.Empty;
    public List<Guid> children = new();
    [JsonIgnore] public GameObject parentGo => GameObject.Find(parent);
    
    [JsonIgnore]
    public Matrix4x4 GlobalMatrix
    {
        get
        {
            if (parentGo == null)
                return LocalMatrix;
            else
                return LocalMatrix * parentGo.Transform.GlobalMatrix;
        }
    }
    
    [JsonIgnore]
    public Vector3 Position
    {
        get
        {
            Matrix4x4.Decompose(GlobalMatrix, out _, out _, out Vector3 translation);
            return translation;
        }
        set
        {
            if (parentGo == null)
                LocalPosition = value;
            else
            {
                Matrix4x4.Invert(parentGo.Transform.GlobalMatrix, out var parentInverse);
                LocalPosition = Vector3.Transform(value, parentInverse);   
            }
        }
    }

    [JsonIgnore]
    public Quaternion Rotation
    {
        get
        {
            Matrix4x4.Decompose(GlobalMatrix, out _, out Quaternion rotation, out _);
            return rotation;
        }
        set
        {
            if (parentGo == null)
                LocalRotation = value;
            else
            {
                Quaternion parentRot = parentGo.Transform.Rotation;
                LocalRotation = Quaternion.Inverse(parentRot) * value;
            }
        }
    }

    [JsonIgnore]
    public Vector3 Scale
    {
        get
        {
            Matrix4x4.Decompose(GlobalMatrix, out Vector3 scale, out _, out _);
            return scale;
        }
    }

    public void SetParent(Transform parent)
    {
        if (parent == null)
        {
            if (GameObject.Find(this.parent) != null)
            {
                GameObject.Find(this.parent).Transform.children.Remove(GameObject.guid);
            }

            LocalPosition = Position;
            LocalRotation = Rotation;
            this.parent = Guid.Empty;
            return;
        }

        if (parent.GameObject.guid == GameObject.guid)
        {
            Debug.Log("Cannot parent the same GameObject");
            return;
        }

        if (parentGo != null)
        {
            parentGo.Transform.children.Remove(GameObject.guid);
        }

        LocalPosition = Vector3.Transform(Position, Matrix4x4.Invert(parent.GlobalMatrix, out var parentInverse) ? parentInverse : Matrix4x4.Identity);
        LocalRotation = Quaternion.Inverse(parent.Rotation) * Rotation;
        
        this.parent = parent.GameObject.guid;
        parent.children.Add(GameObject.guid);
    }
        
    public Vector3 TransformPoint(Vector3 localPoint)
    {
        return Vector3.Transform(localPoint, GlobalMatrix);
    }

    public Vector3 InverseTransformPoint(Vector3 worldPoint)
    {
        Matrix4x4.Invert(GlobalMatrix, out var inverse);
        return Vector3.Transform(worldPoint, inverse);
    }
    
    public Vector3 LocalPosition = new Vector3(0, 0, 0);

    public Vector3 LocalScale = new Vector3(1, 1, 1);

    public Quaternion LocalRotation = Quaternion.Identity;
    
    [JsonIgnore] public Matrix4x4 LocalMatrix =>
        Matrix4x4.CreateScale(LocalScale) *
        Matrix4x4.CreateFromQuaternion(LocalRotation) *
        Matrix4x4.CreateTranslation(LocalPosition);
}