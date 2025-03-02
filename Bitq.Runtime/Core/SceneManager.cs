using Newtonsoft.Json;

namespace Bitq.Core;

public static class SceneManager
{
    private static Action newSceneLoaded = delegate { };
    private static Scene currentScene;

    public static void NewScene()
    {
        currentScene = new Scene();
        newSceneLoaded.Invoke();
        if (!Engine.Editor)
            OnLoad();
        else
            LoadEditor();
    }

    public static Scene GetActiveScene()
    {
        return currentScene;
    }
    
    public static void LoadScene(string sceneName)
    {
        if (currentScene != null)
        {
            Object.Destroy(currentScene);
        }
        
        newSceneLoaded.Invoke();
        
        currentScene = JsonConvert.DeserializeObject<Scene>(File.ReadAllText("Assets/Scene.scene"), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });
        
        currentScene.SetComponentsForEachGameObject();
        
        if (!Engine.Editor)
            OnLoad();
        else
            LoadEditor();
    }

    public static string SaveScene()
    {
        return JsonConvert.SerializeObject(currentScene, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        });
    }
    
    public static void OnLoad()
    {
        currentScene.OnLoad();
    }

    public static void LoadEditor()
    {
        currentScene.LoadEditor();
    }

    public static void OnUpdate(double deltaTime)
    {
        currentScene.OnUpdate(deltaTime);
    }

    public static void OnLateUpdate(double deltaTime)
    {
        currentScene.OnLateUpdate(deltaTime);
    }

    public static void OnFixedUpdate(double deltaTime)
    {
        currentScene.OnFixedUpdate(deltaTime);
    }
    
    public static void OnLateFixedUpdate(double deltaTime)
    {
        currentScene.OnLateFixedUpdate(deltaTime);
    }

    public static void OnDraw(double deltaTime)
    {
        currentScene.OnDraw(deltaTime);
    }

    public static void OnLateDraw(double deltaTime)
    {
        currentScene.OnLateDraw(deltaTime);
    }
}