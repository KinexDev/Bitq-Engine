#version 330 core

in vec2 frag_texCoords;
in vec3 frag_normals;

out vec4 out_color;

uniform sampler2D baseTexture;
uniform bool hasBaseTexture;
uniform bool unlit;
uniform vec4 baseColor;
uniform vec3 lightDir;

void main()
{
    float shading = mix(dot(frag_normals, lightDir), 1, 0.7);
    
    vec4 color = baseColor;

    if (hasBaseTexture)
    {
        color = texture(baseTexture, frag_texCoords) * baseColor;
    }

    if (!unlit)
    {
        out_color = color * vec4(shading, shading, shading, 1);
    } else {
        out_color = color;
    }
}
