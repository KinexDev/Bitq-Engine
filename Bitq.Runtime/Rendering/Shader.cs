using System.Drawing;
using System.Numerics;
using Bitq.Core;
using Silk.NET.OpenGL;

namespace Bitq.Rendering;

public class Shader : IDisposable
{
    private uint _program;
    
    public Shader(string vertexShaderPath, string fragmentShaderPath)
    {
        string vertexShaderSource = File.ReadAllText(vertexShaderPath);

        uint vertexShader = Engine.Gl.CreateShader(ShaderType.VertexShader);
        Engine.Gl.ShaderSource(vertexShader, vertexShaderSource);

        Engine.Gl.CompileShader(vertexShader);

        Engine.Gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
            Debug.Log("Vertex shader failed to compile: " + Engine.Gl.GetShaderInfoLog(vertexShader), LogType.Error);

        string fragmentShaderSource = File.ReadAllText(fragmentShaderPath);

        uint fragmentShader = Engine.Gl.CreateShader(ShaderType.FragmentShader);
        Engine.Gl.ShaderSource(fragmentShader, fragmentShaderSource);

        Engine.Gl.CompileShader(fragmentShader);

        Engine.Gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
            Debug.Log("Fragment shader failed to compile: " + Engine.Gl.GetShaderInfoLog(fragmentShader), LogType.Error);

        _program = Engine.Gl.CreateProgram();

        Engine.Gl.AttachShader(_program, vertexShader);
        Engine.Gl.AttachShader(_program, fragmentShader);

        Engine.Gl.LinkProgram(_program);

        Engine.Gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
        {
            
        }

        Engine.Gl.DetachShader(_program, vertexShader);
        Engine.Gl.DetachShader(_program, fragmentShader);
        Engine.Gl.DeleteShader(vertexShader);
        Engine.Gl.DeleteShader(fragmentShader);

        const uint positionLoc = 0;
        Engine.Gl.EnableVertexAttribArray(positionLoc);
        unsafe
        {
            Engine.Gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);
        }

        const uint texCoordLoc = 1;
        Engine.Gl.EnableVertexAttribArray(texCoordLoc);
        unsafe
        {
            Engine.Gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float),
                (void*)(3 * sizeof(float)));
        }
    }
    
    public static Shader defaultShader;

    public static Shader DefaultShader()
    {
        return defaultShader;
    }

    public void SetUniform(string name, int value)
    {
        int location = Engine.Gl.GetUniformLocation(_program, name);
        if (location == -1)
        {
            return;
        }
        Engine.Gl.Uniform1(location, value);
    }

    public void SetUniform(string name, bool value)
    {
        int location = Engine.Gl.GetUniformLocation(_program, name);
        if (location == -1)
        {
            return;
        }
        Engine.Gl.Uniform1(location, Convert.ToInt32(value));
    }

    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        //A new overload has been created for setting a uniform so we can use the transform in our shader.
        int location = Engine.Gl.GetUniformLocation(_program, name);
        if (location == -1)
        {
            return;
        }
        Engine.Gl.UniformMatrix4(location, 1, false, (float*) &value);
    }

    public unsafe void SetUniform(string name, Vector4 value)
    {
        //A new overload has been created for setting a uniform so we can use the transform in our shader.
        int location = Engine.Gl.GetUniformLocation(_program, name);
        if (location == -1)
        {
            return;
        }

        Engine.Gl.Uniform4(location, value);
    }
    
    public unsafe void SetUniform(string name, Vector3 value)
    {
        //A new overload has been created for setting a uniform so we can use the transform in our shader.
        int location = Engine.Gl.GetUniformLocation(_program, name);
        if (location == -1)
        {
            return;
        }

        Engine.Gl.Uniform3(location, value);
    }
    
    public unsafe void SetUniform(string name, Color value)
    {
        //A new overload has been created for setting a uniform so we can use the transform in our shader.
        int location = Engine.Gl.GetUniformLocation(_program, name);
        if (location == -1)
        {
            return;
        }

        Engine.Gl.Uniform4(location, new Vector4(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f));
    }

    public void SetUniform(string name, float value)
    {
        int location = Engine.Gl.GetUniformLocation(_program, name);
        if (location == -1)
        {
            return;
        }
        Engine.Gl.Uniform1(location, value);
    }
    public void Use()
    {
        Engine.Gl.UseProgram(_program);
    }

    public void Dispose()
    {
        Engine.Gl.DeleteProgram(_program);
    }
}