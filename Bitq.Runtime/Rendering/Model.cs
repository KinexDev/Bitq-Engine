// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using System.Numerics;
using Bitq.Core;
using AssimpMesh = Silk.NET.Assimp.Mesh;
using Scene = Silk.NET.Assimp.Scene;

namespace Bitq.Rendering;
    public class Model : IDisposable
    {
        public static Dictionary<string, List<Mesh>> meshes = new();
        
        public Model(string path, BufferUsageARB usage = BufferUsageARB.StaticDraw)
        {
            var assimp = Assimp.GetApi();
            _assimp = assimp;

            if (meshes.ContainsKey(path))
            {
                Meshes = meshes[path];
            }
            else
                LoadModel(path, usage);
        }

        private Assimp _assimp;
        public string Directory { get; protected set; } = string.Empty;
        public List<Mesh> Meshes { get; protected set; } = new List<Mesh>();
        
        private unsafe void LoadModel(string path, BufferUsageARB usage)
        {
            var scene = _assimp.ImportFile(path, (uint)PostProcessSteps.PreTransformVertices);
            if (scene == null || scene->MFlags == Silk.NET.Assimp.Assimp.SceneFlagsIncomplete || scene->MRootNode == null)
            {
                var error = _assimp.GetErrorStringS();
                throw new Exception(error);
            }

            Directory = path;

            ProcessNode(scene->MRootNode, scene, usage);
            
            meshes.Add(path, Meshes);
        }

        private unsafe void ProcessNode(Node* node, Scene* scene, BufferUsageARB usage)
        {
            for (var i = 0; i < node->MNumMeshes; i++)
            {
                var mesh = scene->MMeshes[node->MMeshes[i]];
                Meshes.Add(ProcessMesh(mesh, usage));
            }

            for (var i = 0; i < node->MNumChildren; i++)
            {
                ProcessNode(node->MChildren[i], scene, usage);
            }
        }

        private unsafe Mesh ProcessMesh(AssimpMesh* mesh, BufferUsageARB usage)
        {
            // data to fill
            List<Vertex> vertices = new List<Vertex>();
            List<uint> indices = new List<uint>();
            List<Texture> textures = new List<Texture>();

            // walk through each of the mesh's vertices
            for (uint i = 0; i < mesh->MNumVertices; i++)
            {
                Vertex vertex = new Vertex();
                vertex.BoneIds = new int[Vertex.MAX_BONE_INFLUENCE];
                vertex.Weights = new float[Vertex.MAX_BONE_INFLUENCE];

                vertex.Position = mesh->MVertices[i];

                // normals
                if (mesh->MNormals != null)
                    vertex.Normal = mesh->MNormals[i];
                // tangent
                if (mesh->MTangents != null)
                    vertex.Tangent = mesh->MTangents[i];
                // bitangent
                if (mesh->MBitangents != null)
                    vertex.Bitangent = mesh->MBitangents[i];
                
                // texture coordinates
                if (mesh->MTextureCoords[0] != null) // does the mesh contain texture coordinates?
                {
                    // a vertex can contain up to 8 different texture coordinates. We thus make the assumption that we won't 
                    // use models where a vertex can have multiple texture coordinates so we always take the first set (0).
                    Vector3 texcoord3 = mesh->MTextureCoords[0][i];
                    vertex.TexCoords = new Vector2(texcoord3.X, texcoord3.Y);
                }

                vertices.Add(vertex);
            }

            // now wak through each of the mesh's faces (a face is a mesh its triangle) and retrieve the corresponding vertex indices.
            for (uint i = 0; i < mesh->MNumFaces; i++)
            {
                Face face = mesh->MFaces[i];
                // retrieve all indices of the face and store them in the indices vector
                for (uint j = 0; j < face.MNumIndices; j++)
                    indices.Add(face.MIndices[j]);
            }
            // return a mesh object created from the extracted mesh data
            var result = new Mesh(BuildVertices(vertices), BuildIndices(indices), textures, usage);
            result.Unbind();
            return result;
        }

        private float[] BuildVertices(List<Vertex> vertexCollection)
        {
            var vertices = new List<float>();

            foreach (var vertex in vertexCollection)
            {
                vertices.Add(vertex.Position.X / 100f);
                vertices.Add(vertex.Position.Y / 100f);
                vertices.Add(vertex.Position.Z / 100f);
                vertices.Add(vertex.TexCoords.X);
                vertices.Add(vertex.TexCoords.Y);
                vertices.Add(vertex.Normal.X);
                vertices.Add(vertex.Normal.Y);
                vertices.Add(vertex.Normal.Z);
            }

            return vertices.ToArray();
        }

        private uint[] BuildIndices(List<uint> indices)
        {
            return indices.ToArray();
        }

        public void Dispose()
        {
            //foreach (var mesh in Meshes)
            //{
            //    mesh.Dispose();
            //}
        }
    }