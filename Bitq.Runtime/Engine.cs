using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Bitq.Audio;
using Bitq.Core;
using Bitq.Rendering;
using Newtonsoft.Json;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Debug = Bitq.Core.Debug;
using Shader = Bitq.Rendering.Shader;
using Texture = Bitq.Rendering.Texture;

namespace Bitq;

public class Engine
{
    public static IKeyboard Keyboard
    {
        get
        {
            if (Input.Keyboards.Count > 0)
                return Input.Keyboards[0];
            return null;
        }
    }

    public static IMouse Mouse
    {
        get
        {
            if (Input.Mice.Count > 0)
                return Input.Mice[0];
            return null;
        }
    }

    public static IWindow Window;
    public static IInputContext Input;
    public static GL Gl;
    public static bool Editor;
    public static Action OnLoad = delegate { };
    public static Action<double> OnUpdate = delegate { };
    public static Action<double> OnRender = delegate { };
    public static AudioSystem System;
    
    public static void Main()
    {
        string name = "Bitq";
        
        if (File.Exists("Assets/Project.settings"))
        {
            var settings = JsonConvert.DeserializeObject<ProjectSettings>(File.ReadAllText("Assets/Project.settings"));
            
            if (!Editor)
            {
                name = settings.projectName;
            }
        }
        else
        {
            var settings = new ProjectSettings(name);
            var json = JsonConvert.SerializeObject(settings);
            File.WriteAllText("Assets/Project.settings", json);
        }
        
        Window = Silk.NET.Windowing.Window.Create(WindowOptions.Default with
        {
            Size = new Vector2D<int>(1920, 1080),
            Title = name
        });
        
        Window.Position = new Vector2D<int>(0,0);
        Window.Load += WindowOnLoad;
        Window.Update += WindowOnUpdate;
        Window.Render += WindowOnRender;
        Window.FramebufferResize += WindowOnResize;
        Window.VSync = false;
        Window.Run();
        Window.Dispose();
        
        if (Editor)
            File.WriteAllText("Assets/Scene.scene", SceneManager.SaveScene());
    }


    public const float fixedTimeStep = (1f / 60f);
    public static int howMuchTimesToSimualte = 0;
    
    public static void PhysicsLoop()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        
        while (true)
        { 
            if (sw.Elapsed.TotalSeconds >= fixedTimeStep)
            {
                howMuchTimesToSimualte += 1;
                sw.Restart();
            }
        }
    }

    private static void WindowOnLoad()
    {
        Gl = Window.CreateOpenGL();
        Input = Window.CreateInput();
        System = new AudioSystem();
        Shader.defaultShader = new Shader("Assets/Shaders/default.vert", "Assets/Shaders/default.frag");
        Gl.ClearColor(Color.CornflowerBlue);
        Gl.Enable(EnableCap.DepthTest);
        
        Gl.Enable(GLEnum.CullFace);
        Gl.CullFace(GLEnum.Back);
        Gl.FrontFace(GLEnum.Ccw);  
        
        //SceneManager.NewScene();
        SceneManager.LoadScene("Assets/Scene.scene");
        //currentScene = JsonConvert.DeserializeObject<Scene>(File.ReadAllText("Assets/Scene.scene"));
        OnLoad.Invoke();
        
        Window.WindowState = Editor ? WindowState.Maximized : WindowState.Maximized;
        
        if (Editor)
            return;

        var thread = new Thread(PhysicsLoop);
        thread.IsBackground = true;
        thread.Start();
    }
    
    private static void WindowOnUpdate(double deltaTime)
    {
        OnUpdate.Invoke(deltaTime);
        if (Editor)
            return;
        SceneManager.OnUpdate(deltaTime * Physics.timeStep);
        SceneManager.OnLateUpdate(deltaTime * Physics.timeStep);

        for (int i = 0; i < howMuchTimesToSimualte; i++)
        {
            Physics.Simulate(fixedTimeStep);
        }

        howMuchTimesToSimualte = 0;
    }

    private static void WindowOnRender(double deltaTime)
    {
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        SceneManager.OnDraw(deltaTime * Physics.timeStep);
        SceneManager.OnLateDraw(deltaTime * Physics.timeStep);
        OnRender.Invoke(deltaTime * Physics.timeStep);
    }
    
    private static void WindowOnResize(Vector2D<int> obj)
    {
        Gl.Viewport(obj);
    }
}