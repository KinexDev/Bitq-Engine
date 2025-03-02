namespace Bitq.Scripting;

public enum LuauOperators
{
    Add,
    Sub,
    Mul,
    Div,
    ToString
}

[AttributeUsage(AttributeTargets.Method)]
public class LuauMetamethod : Attribute
{
    public LuauOperators Operator;

    public LuauMetamethod(LuauOperators op)
    {
        Operator = op;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class LuauCallableFunction : Attribute
{
    //public string Name;
    public bool _static = true;

    public LuauCallableFunction()
    {
    }
    
    public LuauCallableFunction(bool _static)
    {
        this._static = _static;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class LuauVariable : Attribute
{
    //public string Name { get; }
    public LuauVariable()
    {
        //Name = name;
    }
}