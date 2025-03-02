using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using StbImageSharp;
using File = System.IO.File;
using TextureWrapMode = Silk.NET.OpenGL.TextureWrapMode;

namespace Bitq.Rendering;

public class Texture : IDisposable
{
    public static Dictionary<string, ImageResult> textures = new Dictionary<string, ImageResult>();
    private uint _texture;
    
    public string Path { get; set; }
    public TextureType Type { get; }
    
    public Texture(string path, TextureType type = TextureType.BaseColor, bool flipImage = true, bool mipmap = true)
    {
        
        _texture = Engine.Gl.GenTexture();
        Engine.Gl.ActiveTexture(TextureUnit.Texture0);
        Engine.Gl.BindTexture(TextureTarget.Texture2D, _texture);
        Path = path;
        //StbImage.stbi_set_flip_vertically_on_load(Convert.ToInt32(flipImage));

        ImageResult result = null;

        if (!textures.ContainsKey(path))
        {
            result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);
            textures.Add(path, result);
        }
        else
            result = textures[path];
        
        unsafe
        {
            fixed (byte* ptr = result.Data)
                Engine.Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)result.Width,
                    (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        }

        Engine.Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        Engine.Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Engine.Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.Linear);
        Engine.Gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        if (mipmap)
            Engine.Gl.GenerateMipmap(TextureTarget.Texture2D);
        
        Engine.Gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Bind(TextureUnit unit)
    {
        Engine.Gl.ActiveTexture(unit);
        Engine.Gl.BindTexture(TextureTarget.Texture2D, _texture);
    }

    public void Dispose()
    {
        Engine.Gl.DeleteTexture(_texture);
    }
}