// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;

namespace Bitq.Rendering
{
    public class Mesh : IDisposable
    {
        public Mesh(float[] vertices, uint[] indices, List<Texture> textures, BufferUsageARB usage)
        {
            Vertices = vertices;
            Indices = indices;
            Textures = textures;
            SetupMesh(usage);
        }

        public float[] Vertices { get; private set; }
        public uint[] Indices { get; private set; }
        public IReadOnlyList<Texture> Textures { get; private set; }
        public VertexArrayObject<float, uint> VAO { get; set; }
        public BufferObject<float> VBO { get; set; }
        public BufferObject<uint> EBO { get; set; }
        public unsafe void SetupMesh(BufferUsageARB usage)
        {
            EBO = new BufferObject<uint>(Engine.Gl, Indices, BufferTargetARB.ElementArrayBuffer, usage);
            VBO = new BufferObject<float>(Engine.Gl, Vertices, BufferTargetARB.ArrayBuffer, usage);
            VAO = new VertexArrayObject<float, uint>(Engine.Gl, VBO, EBO);
            VAO.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 8, 0);
            VAO.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 8, 3);
            VAO.VertexAttributePointer(2, 3, VertexAttribPointerType.Float, 8, 5);
        }

        public void Bind()
        {
            VAO.Bind();
        }
        
        public void Unbind()
        {
            Engine.Gl.BindVertexArray(0);
            Engine.Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            Engine.Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        public void Dispose()
        {
            Textures = null;
            VAO.Dispose();
            VBO.Dispose();
            EBO.Dispose();
        }
    }
}
