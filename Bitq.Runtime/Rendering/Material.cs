using System.Drawing;
using System.Numerics;
using Bitq.Core;
using Hexa.NET.ImGui;
using Newtonsoft.Json;
using Silk.NET.OpenGL;

namespace Bitq.Rendering;

public class Material
{
    public Material()
    {
        shader = Shader.DefaultShader();
        baseColor = Vector4.One;
    }

    [JsonIgnore] public Shader shader;
    public Vector4 baseColor;
    public Texture baseTexture;
    public bool doubleSided;
    public bool transparent;
    public bool unlit;
    private string _texturePath = "";

    public void Bind()
    {
        if (!doubleSided)
        {
            Engine.Gl.Enable(GLEnum.CullFace);
            Engine.Gl.CullFace(GLEnum.Back);
            Engine.Gl.FrontFace(GLEnum.Ccw);
        }
        else
        {
            Engine.Gl.Disable(GLEnum.CullFace);
        }
        
        if (transparent)
        {
            Engine.Gl.Enable(EnableCap.Blend);
            Engine.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }
        else
        {
            Engine.Gl.Disable(EnableCap.Blend);
        }   
    }
    public void Draw()
    {
        ImGui.Separator();
        
        var path = _texturePath;

        ImGui.Text("Material");

        ImGui.InputText("Texture Path", ref path, 50);
        
        ImGui.ColorEdit4("Color", ref baseColor);
        
        ImGui.Checkbox("Double Sided?", ref doubleSided);
        
        ImGui.Checkbox("Transparent?", ref transparent);

        ImGui.Checkbox("Unlit?", ref unlit);

        if (baseTexture == null && File.Exists(path))
        {
            baseTexture = new Texture(path);
            _texturePath = path;
            Debug.Log("Loaded new texture!");
        } else if (File.Exists(path) && path != _texturePath)
        {
            _texturePath = path;
            baseTexture.Dispose();
            baseTexture = new Texture(path);
            Debug.Log("Loaded new texture!");
        }
    }
}