using Bitq.Audio;
using Bitq.Core;
using Hexa.NET.ImGui;
using Newtonsoft.Json;

namespace Bitq.Runtime.Core;

public class AudioSource : Component
{
    public string path = "";
    public bool loop = false;
    public bool playOnLoad = true;
    [JsonIgnore] public Bitq.Audio.AudioSource audioSource;
    [JsonIgnore] public AudioClip audioClip;
    public override void OnLoad()
    {
        if (!File.Exists(path))
            return;
        
        audioClip = Engine.System.LoadWav(path);
        audioSource = Engine.System.CreateSource();
        if (playOnLoad)
            Play();
    }

    public void Play()
    {
        audioSource.Play(audioClip, loop);
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    public override void OnActiveChanged(bool active)
    {
        if (!active)
        {
            Stop();
        }
    }

    public override void OnDispose()
    {
        audioSource.Stop();
        audioClip.Dispose();
        audioSource.Dispose();
    }

    public override void OnInspectorGUI(float deltaTime)
    {
        ImGui.InputText("path", ref path, 50);
        ImGui.Checkbox("Loop?", ref loop);
        ImGui.Checkbox("Play On Load?", ref playOnLoad);
    }
}