namespace Bitq.Core;

[AttributeUsage(AttributeTargets.Class)]
public class ToBeInherited : Attribute {}

[AttributeUsage(AttributeTargets.Class)]
public class ReloadEditor : Attribute {}

[AttributeUsage(AttributeTargets.Class)]
public class RequireComponent : Attribute
{
    public Type type;

    public RequireComponent(Type type)
    {
        this.type = type;
    }
}