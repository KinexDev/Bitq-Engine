#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTextureCoord;
layout (location = 2) in vec3 aNormals;

out vec2 frag_texCoords;
out vec3 frag_normals;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
    
    frag_texCoords = aTextureCoord;
    
    mat3 normalMatrix = transpose(inverse(mat3(uModel)));
    frag_normals = normalize(normalMatrix * aNormals);
}
