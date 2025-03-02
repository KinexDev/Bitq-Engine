using System.Numerics;
using Bitq.Rendering;
using Hexa.NET.ImGui;
using Newtonsoft.Json;
using Silk.NET.OpenGL;

namespace Bitq.Core;

[ReloadEditor]
public class MeshRenderer : Component
{
    public string meshPath = "Assets/Models/cube.fbx";
    public Material material = new();
    [JsonIgnore] public Model model;
    
    public override void OnLoad()
    {
        ReloadMesh();
    }

    public void ReloadMesh()
    {
        if (File.Exists(meshPath))
        {
            if (model != null)
            {
                model.Dispose();
            }
            model = new Model(meshPath);
        }
    }

    public override void OnDraw(double deltaTime)
    {
        if (material == null || model == null || Camera.main == null)
            return;

        if (material.transparent)
            return;
        
        RenderMeshes();
    }

    public override void OnLateDraw(double deltaTime)
    {
        if (material == null || model == null || Camera.main == null)
            return;
        
        if (!material.transparent)
            return;

        RenderMeshes();
    }

    public void RenderMeshes()
    {
        foreach (var mesh in model.Meshes)
        {
            mesh.Bind();
            material.shader.Use();
            material.Bind();
            if (material.baseTexture != null)
            {
                material.baseTexture.Bind(TextureUnit.Texture0);
                material.shader.SetUniform("baseTexture", 0);
            }
            
            material.shader.SetUniform("hasBaseTexture", material.baseTexture != null);
            material.shader.SetUniform("baseColor",  material.baseColor);
            material.shader.SetUniform("unlit",  material.unlit);
            material.shader.SetUniform("uModel", GameObject.Transform.GlobalMatrix);
            material.shader.SetUniform("lightDir", new Vector3(1,0, 0.5f));
            material.shader.SetUniform("uView", Camera.main.View);
            material.shader.SetUniform("uProjection", Camera.main.Projection);
            Engine.Gl.DrawArrays(Silk.NET.OpenGL.PrimitiveType.Triangles, 0, (uint)mesh.Vertices.Length);
            
            mesh.Unbind();
        }
    }

    public override void OnDispose()
    {
        model.Dispose();
    }

    public override void OnInspectorGUI(float deltaTime)
    {
        var path = meshPath;
        ImGui.InputText("path", ref path, 50);

        if (model == null && File.Exists(path))
        {
            model = new Model(path);
            meshPath = path;
        } else if (File.Exists(path) && path != meshPath)
        {
            meshPath = path;
            model.Dispose();
            model = new Model(path);
        }
        
        material.Draw();
    }
}