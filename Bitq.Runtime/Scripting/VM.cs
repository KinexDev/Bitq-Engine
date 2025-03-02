using System.Reflection;
using System.Runtime.InteropServices;
using Bitq.Core;

namespace Bitq.Scripting;

public class VM : IDisposable
{
    private Action<object> print;
    private bool loaded;
    private string name;
    public IntPtr L;
    private Dictionary<IntPtr, GCHandle> objectHandles = new();
    //approach suggested in the comments of uh luau thing i was reading
    private static Dictionary<IntPtr, IntPtr> userdataToVMRefs = new();
    public static Dictionary<IntPtr, VM> vmRefs = new();
    public Dictionary<Type, CachedType> CachedTypes = new();
    
    public struct LuaRef
    {
        public int idx;
    }
    
    //cache type so we don't use reflection all the time especially on update
    public class CachedType
    {
        public Dictionary<string, Luau.LuaFunction> metamethods = new();
        public Dictionary<string, Luau.LuaFunction> functions = new();
        public Dictionary<string, FieldInfo> fields = new();
        public Dictionary<string, PropertyInfo> properties = new();
    }

    public static VM GetVMInstance(IntPtr L)
    {
        return vmRefs.GetValueOrDefault(L);
    }
    
    //Forwards a usuable type in luau, this allows all static methods to be accessed in luau.
    public void RegisterUserdataType<T>(string name = null) where T : class
    {
        var type = typeof(T);
        
        if (CachedTypes.ContainsKey(typeof(T)))
            throw new Exception("Type already registered");
        
        //var indexFunction = (Luau.LuaFunction)(_ => LuaIndexFunction(this, typeof(T), true));
        //GCHandle idxgch = GCHandle.Alloc(indexFunction);
        //objectHandles.Add((IntPtr)idxgch, idxgch);

        //var newIndexFunction = (Luau.LuaFunction)(_ => LuaNewIndexFunction(this, typeof(T), true));
        //GCHandle newgch = GCHandle.Alloc(newIndexFunction);
        //objectHandles.Add((IntPtr)newgch, newgch);

        
        if (!string.IsNullOrEmpty(name))
        {
            Luau.Luau_newtable(L);   
        }
        //Luau.Luau_pushcfunction(L, indexFunction, typeof(T).FullName! + "__index");
        //Luau.Luau_setfield(L, -2, "__index");
        //Luau.Luau_pushcfunction(L, newIndexFunction, typeof(T).FullName! + "__newindex");
        //Luau.Luau_setfield(L, -2, "__newindex");
        //Luau.Luau_setmetatable(L, -2);
        // it broke for me so i resorted to doing this, static variables do not work here. also with the new stuff
        // im setting up the type caching here.
        var userdataType = new CachedType();
        var metamethods = type.GetMethods()
            .Where(m => m.IsDefined(typeof(LuauMetamethod), false));

        foreach (var metamethod in metamethods)
        {
            //these are supposed to last for the lifetime of the VM for obvious reasons, so i will just pin it
            var _delegate = (Luau.LuaFunction)Delegate.CreateDelegate(typeof(Luau.LuaFunction), metamethod);
            var metamethodattribute = (LuauMetamethod)metamethod.GetCustomAttribute(typeof(LuauMetamethod), false);
            var metamethodName = $"__{metamethodattribute.Operator.ToString().ToLower()}";
            userdataType.metamethods.Add(metamethodName, _delegate);
        }

        var methods = type.GetMethods()
            .Where(m => m.IsDefined(typeof(LuauCallableFunction), false));

        foreach (var method in methods)
        {
            var _delegate = (Luau.LuaFunction)Delegate.CreateDelegate(typeof(Luau.LuaFunction), method);
            userdataType.functions.Add(method.Name, _delegate);
            var attribute = (LuauCallableFunction)method.GetCustomAttribute(typeof(LuauCallableFunction), false);
            if (!attribute._static)
                continue;
            if (!string.IsNullOrEmpty(name))
            {
                Luau.Luau_pushcfunction(L, _delegate, method.Name);
                Luau.Luau_setfield(L, -2, method.Name);
            }
        }

        var fields = type.GetFields()
            .Where(m => m.IsDefined(typeof(LuauVariable), false));

        foreach (var field in fields)
        {
            userdataType.fields.Add(field.Name, field);
        }

        var properties = type.GetProperties()
            .Where(m => m.IsDefined(typeof(LuauVariable), false));

        foreach (var property in properties)
        {
            userdataType.properties.Add(property.Name, property);
        }
        
        CachedTypes.Add(type, userdataType);
        if (!string.IsNullOrEmpty(name))
        {
            Luau.Luau_setglobal(L, name);
        }
    }
    
    private Luau.LuaFunction PinnedLuaDelegate(Luau.LuaFunction function)
    {
        GCHandle gch = GCHandle.Alloc(function);
        
        objectHandles.Add((IntPtr)gch, gch);

        return function;
    }
    
    
    public VM(Action<object> printAc, bool openStandardLibs = true, string scriptName = null)
    {
        if (string.IsNullOrEmpty(scriptName))
            this.name = $"Script {vmRefs.Count}";
        else
            this.name = name;
        
        print = printAc;
        L = Luau.Luau_newstate();
        Luau.Luau_setsafeenv(L, Luau.LUA_ENVIRONINDEX, 1);
        if (openStandardLibs)
            Luau.Luau_openlibs(L);
        vmRefs.Add(L, this);
        PushGlobalFunction("print", LuaPrint);
    }
    
    private static int LuaPrint(IntPtr L)
    {
        var vm = GetVMInstance(L);
        int nargs = Luau.Luau_gettop(vm.L);
        for (int i = 1; i <= nargs; i++)
        {
            if (Luau.Luau_isstring(vm.L, i) == 1) {
                var s = Luau.Luau_tostring(vm.L, i);
                vm.print.Invoke(Luau.ptr_tostring(s));
            }
            else
            {
                var t = Luau.Luau_type(vm.L, i);
                switch ((Luau.LuaType)t)
                {
                    case Luau.LuaType.Boolean:
                        var b = Luau.Luau_toboolean(vm.L, i);
                        vm.print.Invoke(b == 0 ? "false" : "true");
                        break;
                    case Luau.LuaType.Number:
                        var n = Luau.Luau_tonumber(vm.L, i);
                        vm.print.Invoke(n);
                        break;
                    default:
                        vm.print.Invoke((Luau.LuaType)t);
                        break;
                }
            }
        }
        return 0;
    }

    public IntPtr Compile(string source, out byte[] bytecode)
    {
        var size = UIntPtr.Zero;
        var res = Luau.Luau_compile(source, ref size);

        //export bytecode
        var managedSize = (int)size.ToUInt32();
        bytecode = new byte[managedSize];
        Marshal.Copy(res, bytecode, 0, managedSize);
        return res;
    }
    
    public IntPtr Compile(string source, out UIntPtr size)
    {
        size = UIntPtr.Zero;
        var res = Luau.Luau_compile(source, ref size);
        return res;
    }

    public int Load(IntPtr bytecode, UIntPtr size)
    {
        if (loaded)
            throw new Exception("VM is already loaded");
        loaded = true;
        return Luau.Luau_load(L, name, bytecode, size, 0);
    }

    public void Execute(int results = 0, params object[] args)
    {
        foreach (var arg in args)
        {
            PushValueToStack(arg);
        }
        if (Luau.Luau_pcall(L, args.Length, results, 0) != 0)
        {
            var error = $"{Luau.ptr_tostring(Luau.Luau_tostring(L, -1))}";
            Luau.Luau_pop(L, 1);
            throw new Exception(error);
        }
    }
    
    public void ExecuteRef(LuaRef luaRef, int results = 0, params object[] args)
    {
        LoadRefToStack(luaRef);
        foreach (var arg in args)
        {
            PushValueToStack(arg);
        }
        
        if (Luau.Luau_pcall(L, args.Length, results, 0) != 0)
        {
            var error = $"{Luau.ptr_tostring(Luau.Luau_tostring(L, -1))}";
            Luau.Luau_pop(L, 1);
            throw new Exception(error);
        }

        Pop(1);
    }

    public void Pop(int index)
    {
        Luau.Luau_pop(L, index);
    }

    public void ExecuteFunction(string name, int results = 0, params object[] args)
    {
        Luau.Luau_getglobal(L, name);
        
        foreach (var arg in args)
        {
            PushValueToStack(arg);
        }
        
        if (Luau.Luau_pcall(L, args.Length, results, 0) != 0)
        {
            var error = $"{Luau.ptr_tostring(Luau.Luau_tostring(L, -1))}";
            Luau.Luau_pop(L, 1);
            throw new Exception(error);
        }
    }
    
    public Luau.LuaType GetGlobalType(string name)
    {
        Luau.Luau_getglobal(L, name);
        var type = (Luau.LuaType)Luau.Luau_type(L, -1);
        Luau.Luau_pop(L, 1);
        return type;
    }
    
    public void PushGlobalFunction(string name, Luau.LuaFunction fn)
    {
        Luau.Luau_pushvalue(L, Luau.LUA_GLOBALSINDEX);
        Luau.Luau_pushcfunction(L, PinnedLuaDelegate(fn), name);
        Luau.Luau_setglobal(L, name);
        Luau.Luau_pop(L, 1);
    }
    
    public void PushFunctionToStack(Luau.LuaFunction fn)
    {
        Luau.Luau_pushcfunction(L, PinnedLuaDelegate(fn), name);
    }

    public void PushGlobalUserdata<T>(string name, T obj) where T : class
    {
        if (!CachedTypes.ContainsKey(obj.GetType()))
            throw new Exception("Userdata type not registered");
        
        Luau.Luau_pushvalue(L, Luau.LUA_GLOBALSINDEX);

        PushLightUserdataToStack(obj);
        
        Luau.Luau_setglobal(L, name);
        Luau.Luau_pop(L, 1);
    }
    
    public void PushLightUserdataToStack<T>(T obj) where T : class
    {
        if (!CachedTypes.ContainsKey(obj.GetType()))
            throw new Exception("Userdata type not registered");
        
        GCHandle handle = GCHandle.Alloc(obj);
        IntPtr ptr = (IntPtr)handle;
        objectHandles[ptr] = handle;
        Luau.Luau_pushlightuserdata(L, ptr);

        RegisterMetatable(obj);
    }
    
    public void PushGlobalValue(string name, object obj)
    {
        Luau.Luau_pushvalue(L, Luau.LUA_GLOBALSINDEX);
        PushValueToStack(obj);
        Luau.Luau_setglobal(L, name);
        Luau.Luau_pop(L, 1);
    }
    
    //ion even know how this works its a miracle because it does get GC'd too i think at least from me creating 10000 objects on tick.
    public void PushUserdataToStack(object obj)
    {
        if (!CachedTypes.ContainsKey(obj.GetType()))
            throw new Exception("Userdata type not registered");
        
        GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Normal);
        
        IntPtr userdata = Luau.Luau_newuserdatadtor(
            L,
            UIntPtr.Zero,
            UserdataDestructor
        );
        
        userdataToVMRefs.Add(userdata, L);

        //Luau.Luau_getglobal(L, registeredTypes[typeof(T)]);
        RegisterMetatable(obj);

        objectHandles[userdata] = handle;
    }
    
    //approach suggested in the comments of uh luau thing i was reading
    private static void UserdataDestructor(IntPtr userdataPtr)
    {
        if (userdataToVMRefs.ContainsKey(userdataPtr))
        {
            VM vm = GetVMInstance(userdataToVMRefs[userdataPtr]);
            vm.ClearUserdata(userdataPtr);
            
            userdataToVMRefs.Remove(userdataPtr);
        }
    }
    
    private void RegisterMetatable(object obj)
    {
        Luau.Luau_newtable(L);
        Luau.Luau_pushcfunction(L, LuaIndexFunction,
            obj.GetType().FullName! + "__index");
        Luau.Luau_setfield(L, -2, "__index");
        Luau.Luau_pushcfunction(L, LuaNewIndexFunction, obj.GetType().FullName! + "__newindex");
        Luau.Luau_setfield(L, -2, "__newindex");

        if (CachedTypes.TryGetValue(obj.GetType(), out var cachedType))
        {
            foreach (var method in cachedType.metamethods)
            {
                Luau.Luau_pushcfunction(L, method.Value, "");
                Luau.Luau_setfield(L, -2, method.Key);
            }    
        }
        
        Luau.Luau_setmetatable(L, -2);
    }

    private static int LuaIndexFunction(IntPtr L)
    {
        var vm = GetVMInstance(L);
        object obj = vm.GetUserdataRaw(Luau.Luau_touserdata(vm.L, 1));
        string memberName = Luau.ptr_tostring(Luau.Luau_tostring(vm.L, 2));
        
        if (!vm.CachedTypes.TryGetValue(obj.GetType(), out var cachedType))
            throw new Exception("Userdata type not registered");
        
        if (memberName == null || obj == null)
            throw new Exception("Cannot get member name or obj is null!");
        
        if (cachedType.functions.TryGetValue(memberName, out var _delegate))
        {
            Luau.Luau_pushcfunction(L, _delegate, memberName);
            return 1;
        }

        if (cachedType.fields.TryGetValue(memberName, out var field))
        {
            vm.PushValueToStack(field.GetValue(obj));
            return 1;
        }
        
        if (cachedType.properties.TryGetValue(memberName, out var property))
        {
            vm.PushValueToStack(property.GetValue(obj));
            return 1;
        }

        //var method = obj.GetType().GetMethod(memberName);
        
        //if (method != null && method.IsDefined(typeof(LuauCallableFunction), false))
        //{
        //    var _delegate = (Luau.LuaFunction)Delegate.CreateDelegate(typeof(Luau.LuaFunction), method);
        //    Luau.Luau_pushcfunction(L, _delegate, method.Name);
        //    return 1;
        //}

        //var field = obj.GetType().GetField(memberName);
        //if (field != null &&
        //    field.IsDefined(typeof(LuauVariable), false))
        //{
        //    vm.PushValueToStack(field.GetValue(obj));
        //    return 1;
        //}

        //var property = obj.GetType().GetProperty(memberName);
        //if (property != null && property.CanRead &&
        //    property.IsDefined(typeof(LuauVariable), false))
        //{
        //    vm.PushValueToStack(property.GetValue(obj));
        //    return 1;
        //}

        return 0;
    }

    private static int LuaNewIndexFunction(IntPtr L)
    {
        var vm = GetVMInstance(L);
        var obj = vm.GetUserdataRaw(Luau.Luau_touserdata(L, 1));
        string memberName = Luau.ptr_tostring(Luau.Luau_tostring(L, 2));
        object newValue = GetLuaValue(vm, 3);
        if (!vm.CachedTypes.TryGetValue(obj.GetType(), out var cachedType))
            throw new Exception("Userdata type not registered");
        
        if (memberName == null || newValue == null || obj == null)
            throw new Exception("Cannot index new function because the obj, member, or value is null!");

        if (cachedType.fields.TryGetValue(memberName, out var field))
        {
            field.SetValue(obj, newValue);
            return 0;
        }
        
        if (cachedType.properties.TryGetValue(memberName, out var property))
        {
            property.SetValue(obj, newValue);
            return 0;
        }
        
        //var field = obj.GetType().GetField(memberName);
        //if (field != null && field.FieldType.IsInstanceOfType(newValue) && field.IsDefined(typeof(LuauVariable), false))
        //{
        //    field.SetValue(obj, newValue);
        //    return 0;
        //}

        //var property = obj.GetType().GetProperty(memberName);
        //if (property != null && property.CanWrite && property.PropertyType.IsInstanceOfType(newValue) && property.IsDefined(typeof(LuauVariable), false))
        //{
        //    property.SetValue(obj, newValue);
        //    return 0;
        //}
        return 0;
    }
    
    public static object GetLuaValue(VM vm, int index)
    {
        if (Luau.Luau_isnumber(vm.L, index) == 1)
            return Luau.Luau_tonumber(vm.L, index);
        if (Luau.Luau_isstring(vm.L, index) == 1)
            return Luau.ptr_tostring(Luau.Luau_tostring(vm.L, index));
        if (Luau.Luau_isboolean(vm.L, index) == 1)
            return Convert.ToBoolean(Luau.Luau_toboolean(vm.L, index));
        if (Luau.Luau_isfunction(vm.L, index) == 1)
            return Luau.Luau_tocfunction(vm.L, index);
        if (Luau.Luau_isuserdata(vm.L, index) == 1)
        {
            IntPtr userdataPtr = Luau.Luau_touserdata(vm.L, index);
            return vm.GetUserdataRaw(userdataPtr);
        }
        return null;
    }

    public string? ReadString(int index)
    {
        if (Luau.Luau_isstring(L, index) == 1)
        {
            return Luau.ptr_tostring(Luau.Luau_tostring(L, index));
        }
        return null;
    }
    
    public double? ReadNumber(int index)
    {
        if (Luau.Luau_isnumber(L, index) == 1)
        {
            return Luau.Luau_tonumber(L, index);
        }
        return null;
    }
    
    public bool? ReadBoolean(int index)
    {
        if (Luau.Luau_isboolean(L, index) == 1)
        {
            return Convert.ToBoolean(Luau.Luau_toboolean(L, index));
        }
        return null;
    }
    
    public T? ReadUserdata<T>(int index) where T : class
    {
        if (Luau.Luau_isuserdata(L, index) == 1)
        {
            var ptr = Luau.Luau_touserdata(L, index);
            return GetUserdata<T>(ptr);
        }
        return null;
    }

    public int GetArguments()
    {
        return Luau.Luau_gettop(L);
    }

    public bool IsFunction(int index)
    {
        return Luau.Luau_isfunction(L, index) == 1;
    }

    public bool IsTable(int index)
    {
        return Luau.Luau_istable(L, index) == 1;
    }

    public LuaRef StoreRef(int index)
    {
        return new LuaRef() { idx = Luau.Luau_ref(L, index) };
    }

    public int LoadRefToStack(LuaRef luaRef)
    {
        return Luau.Luau_getref(L, luaRef.idx);
    }

    public void DisposeRef(LuaRef luaRef)
    {
        Luau.Luau_unref(L, luaRef.idx);
    }

    public Luau.LuaType GetType(int index)
    {
        return (Luau.LuaType)Luau.Luau_type(L, index);
    }
    
    public void ClearUserdata(IntPtr ptr)
    {
        if (objectHandles.ContainsKey(ptr))
        {
            objectHandles[ptr].Free();
            objectHandles.Remove(ptr);
        }
    }

    public void ThrowError(string message)
    {
        Luau.Luau_pushstring(L, message);
        Luau.Luau_error(L);
    }

    public void PushValueToStack(object obj)
    {
        if (obj == null)
        {
            Luau.Luau_pushnil(L);
            return;
        }
        
        if (obj is float)
        {
            float floatValue = (float)obj;
            Luau.Luau_pushnumber(L, floatValue);
        } else if (obj is double)
        {
            double doubleValue = (double)obj;
            Luau.Luau_pushnumber(L, doubleValue);
        } else if (obj is int)
        {
            int intValue = (int)obj;
            Luau.Luau_pushnumber(L, intValue);
        } else if (obj is string)
        {
            string strValue = (string)obj;
            Luau.Luau_pushstring(L, strValue);
        } else if (obj is bool)
        {
            bool boolValue = (bool)obj;
            Luau.Luau_pushboolean(L, boolValue ? 1 : 0);
        } else if (obj is Luau.LuaFunction)
        {
            Luau.Luau_pushcfunction(L, (Luau.LuaFunction)obj, "");
        }
        else
        {
            PushUserdataToStack(obj);
        }
    }

    public object GetGlobal(string name)
    {
        Luau.Luau_getglobal(L, name);
        var returnValue = GetFromStack(-1);
        return returnValue;
    }

    public object GetFromStack(int idx)
    {
        var type = (Luau.LuaType)Luau.Luau_type(L, idx);

        switch (type)
        {
            case Luau.LuaType.Boolean:
                return Convert.ToBoolean(Luau.Luau_toboolean(L, idx));
                break;
            case Luau.LuaType.String:
                return Luau.Luau_tostring(L, idx);
                break;
            case Luau.LuaType.Number:
                return Luau.Luau_tonumber(L, idx);
                break;
            case Luau.LuaType.UserData or Luau.LuaType.LightUserData:
                return GetUserdataRaw(Luau.Luau_touserdata(L, idx));
        }
        
        return null;
    }

    private T GetUserdata<T>(IntPtr ptr) where T : class
    {
        if (!CachedTypes.ContainsKey(typeof(T)))
            throw new Exception("Userdata type not registered");
        
        if (objectHandles.TryGetValue(ptr, out GCHandle handle))
        {
            return (T)handle.Target;
        }
        return null;
    }
    
    private object GetUserdataRaw(IntPtr ptr)
    {
        if (objectHandles.TryGetValue(ptr, out GCHandle handle))
        {
            return handle.Target;
        }
        return null;
    }
    
    public void DoString(string source)
    {
        var bytecode = Compile(source, out UIntPtr size);
        if (Load(bytecode, size) != 0)
            throw new Exception("VM load failed");
        else
            Execute();
    }
    
    public void DoByteCode(byte[] bytecode)
    {
        GCHandle pinnedArray = GCHandle.Alloc(bytecode, GCHandleType.Pinned);
        UIntPtr size = new UIntPtr((uint)bytecode.Length);
        IntPtr pointer = pinnedArray.AddrOfPinnedObject();
        if (Load(pointer, size) != 0)
        {
            pinnedArray.Free();
            throw new Exception("VM load failed");
        }
        else
            Execute();
        pinnedArray.Free();
    }
    
    public void Dispose()
    {
        Luau.Luau_close(L);
        vmRefs.Remove(L);
        foreach (var objects in objectHandles)
        {
            if (objects.Value.IsAllocated)
                objects.Value.Free();
        }
    }
}
