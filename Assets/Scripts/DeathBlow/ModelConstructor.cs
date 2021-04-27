using System;
using System.Collections.Generic;
using System.Linq;
using DeathBlow.Components;
using InfectedRose.Nif;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using UVector3 = UnityEngine.Vector3;
using UVector4 = UnityEngine.Vector4;

namespace DeathBlow
{
    public class ModelConstructor
    {
        public NiFile File { get; set; }

        public string Name { get; set; }

        public string NifPath { get; set; }

        public bool CreatePrefab { get; set; }
        
        public string PrefabPath { get; set; }

        public Dictionary<NiAVObject, Transform> Nodes { get; set; } = new Dictionary<NiAVObject, Transform>();

        public List<(GameObject parent, NiSkinInstance skinInstance)> Skinned { get; set; } =
            new List<(GameObject parent, NiSkinInstance skinInstance)>();

        public List<(GameObject parent, NiTexturingProperty texturingProperty)> ApplyTextures { get; set; } =
            new List<(GameObject parent, NiTexturingProperty texturingProperty)>();

        public GameObject Construct()
        {
            var prefabPath = PrefabPath + ".prefab";

            var existing = (GameObject) AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
            
            GameObject root;
            
            if (existing != null)
            {
                root = (GameObject) PrefabUtility.InstantiatePrefab(existing);
                
                return root;
            }
            
            root = new GameObject(Name);

            foreach (var block in File.Blocks)
            {
                if (block is NiNode node)
                {
                    BuildHierarchy(node, root);

                    break;
                }
            }

            foreach (var (a, b) in Skinned)
            {
                BuildRig(a, b);
            }

            foreach (var (a, b) in ApplyTextures)
            {
                foreach (var child in a.GetComponentsInChildren<MeshRenderer>())
                {
                    ApplyTexturing(b, child.gameObject);
                }
                
                foreach (var child in a.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    ApplyTexturing(b, child.gameObject);
                }
            }

            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

            PrefabUtility.SaveAsPrefabAssetAndConnect(root, prefabPath, InteractionMode.AutomatedAction);
            
            return root;
        }

        public void BuildHierarchy(NiNode node, GameObject parent)
        {
            var instance = SpawnAvObject(node, parent);

            foreach (var ptr in node.Children)
            {
                var child = ptr.Get(File);

                switch (child)
                {
                    case NiNode childNode:
                        BuildHierarchy(childNode, instance);
                        break;
                    case { }:
                        SpawnAvObject(child, instance);
                        break;
                    default:
                        throw new NotImplementedException($"Child Node type {child.GetType().Name} is not implemented");
                }
            }
        }

        public GameObject SpawnAvObject(NiAVObject avObject, GameObject parent)
        {
            var name = avObject.Name.Get(File);

            var instance = new GameObject(name);

            var position = avObject.Translation;

            instance.transform.SetParent(parent.transform);
            instance.transform.localPosition = new UVector3(position.x, position.y, position.z);
            instance.transform.localScale = UVector3.one * avObject.Scale;
            instance.transform.localEulerAngles = avObject.Rotation.ToEulerAngles();

            switch (avObject)
            {
                case NiTriShape triShape:
                    BuildTriShape(triShape, instance);
                    break;
                case NiTriStrips triStrips:
                    BuildTriStrips(triStrips, instance);
                    break;
                case NiCamera camera:
                    BuildCamera(camera, instance);
                    break;
                case NiLODNode lodGroup:
                    BuildLOD(lodGroup, instance);
                    break;
                case NiAmbientLight ambientLight:
                    BuildAmbientLight(ambientLight, parent);
                    break;
                case NiNode _:
                    break;
                default:
                    throw new NotImplementedException($"Node type {avObject.GetType().Name} is not implemented");
            }

            Nodes[avObject] = instance.transform;

            BuildProperties(avObject, parent);

            return instance;
        }

        public void BuildProperties(NiAVObject avObject, GameObject parent)
        {
            foreach (var ptr in avObject.Properties)
            {
                var property = ptr.Get(File);

                if (property == null)
                {
                    continue;
                }

                var component = parent.AddComponent<NiPropertyComponent>();

                component.SetProperty(property);
                
                switch (property)
                {
                    case NiTexturingProperty texturingProperty:
                        ApplyTexturing(texturingProperty, parent);
                        break;
                }
            }
        }

        public void BuildTriShape(NiTriShape shape, GameObject parent)
        {
            var geometry = shape.Data.Get(File);

            if (!(geometry is NiTriShapeData data))
            {
                throw new NotImplementedException("Invalid NiTriShape data type");
            }

            var mesh = new Mesh();

            mesh.name = shape.Name.Get(File);

            mesh.vertices = data.HasVertices
                ? data.Vertices.Select(
                    v => v.ToUVector()
                ).ToArray()
                : null;

            mesh.triangles = data.HasTriangles
                ? data.Triangles.SelectMany(
                    t => new int[] {t.v1, t.v2, t.v3}
                ).ToArray()
                : new int[0];

            mesh.normals = data.HasNormals
                ? data.Normals.Select(
                    v => v.ToUVector()
                ).ToArray()
                : null;

            mesh.colors = data.HasVertexColors
                ? data.VertexColors.Select(
                    c => new Color(c.r, c.g, c.b, c.a)
                ).ToArray()
                : null;

            mesh.tangents = data.Tangents?.Select(t => (UVector4) t.ToUVector()).ToArray();

            mesh.bounds = new Bounds(data.Center.ToUVector(), UVector3.one * data.Radius);

            for (var i = 0; i < data.UVSets.GetLength(0); i++)
            {
                var channel = new Vector2[data.UVSets.GetLength(1)];

                for (var j = 0; j < data.UVSets.GetLength(1); j++)
                {
                    var texCoord = data.UVSets[i, j];

                    channel[j] = new Vector2(texCoord.u, texCoord.v);
                }

                mesh.SetUVs(i, channel);
            }

            var filter = parent.AddOrGetComponent<MeshFilter>();

            var renderer = parent.AddOrGetComponent<MeshRenderer>();

            RegisterRenderer(renderer);

            renderer.sharedMaterial = WorkspaceControl.CurrentWorkspace.VertexMaterial;

            filter.mesh = mesh;

            var skinned = shape.SkinInstance.Get(File);

            if (skinned != null)
            {
                Skinned.Add((parent, skinned));
            }
            Debug.Log(PrefabPath + ".asset");
            Debug.Log(AssetDatabase.GenerateUniqueAssetPath(PrefabPath + ".asset"));
            
            AssetDatabase.CreateAsset(mesh, AssetDatabase.GenerateUniqueAssetPath(PrefabPath + ".asset"));
        }

        public void BuildTriStrips(NiTriStrips shape, GameObject parent)
        {
            var geometry = shape.Data.Get(File);

            if (!(geometry is NiTriStripsData data))
            {
                throw new NotImplementedException("Invalid NiTriStrips data type");
            }

            var triangles = new List<int>();

            foreach (var point in data.Points)
            {
                var index = 1;

                var flip = false;

                while (index + 1 < point.Length)
                {
                    var tris = new List<int>
                    {
                        point[index - 1],
                        point[index],
                        point[index + 1]
                    };

                    if (flip)
                    {
                        tris.Reverse();
                    }

                    triangles.AddRange(tris);

                    index++;

                    flip = !flip;
                }
            }

            var mesh = new Mesh();

            mesh.name = shape.Name.Get(File);

            mesh.vertices = data.HasVertices
                ? data.Vertices.Select(
                    v => v.ToUVector()
                ).ToArray()
                : null;

            mesh.triangles = triangles.ToArray();

            mesh.normals = data.HasNormals
                ? data.Normals.Select(
                    v => v.ToUVector()
                ).ToArray()
                : null;

            mesh.colors = data.HasVertexColors
                ? data.VertexColors.Select(
                    c => new Color(c.r, c.g, c.b, c.a)
                ).ToArray()
                : null;

            mesh.tangents = data.Tangents?.Select(t => (UVector4) t.ToUVector()).ToArray();

            mesh.bounds = new Bounds(data.Center.ToUVector(), UVector3.one * data.Radius);

            for (var i = 0; i < data.UVSets.GetLength(0); i++)
            {
                var channel = new Vector2[data.UVSets.GetLength(1)];

                for (var j = 0; j < data.UVSets.GetLength(1); j++)
                {
                    var texCoord = data.UVSets[i, j];

                    channel[j] = new Vector2(texCoord.u, texCoord.v);
                }

                mesh.SetUVs(i, channel);
            }

            var filter = parent.AddOrGetComponent<MeshFilter>();

            var renderer = parent.AddOrGetComponent<MeshRenderer>();

            RegisterRenderer(renderer);

            renderer.sharedMaterial = WorkspaceControl.CurrentWorkspace.VertexMaterial;

            filter.mesh = mesh;
            
            AssetDatabase.CreateAsset(mesh, AssetDatabase.GenerateUniqueAssetPath(PrefabPath + ".asset"));
        }

        public void BuildLOD(NiLODNode node, GameObject parent)
        {
            var data = node.LODLevelData.Get(File);

            var lod = parent.AddOrGetComponent<LODGroup>();

            switch (data)
            {
                case NiRangeLODData rangeLODData:
                    lod.SetLODs(rangeLODData.LODLevels.Select(
                        l => new LOD(1f / l.FarExtent, new Renderer[0])
                    ).ToArray());
                    break;
                case NiScreenLODData screenLODData:
                    throw new NotImplementedException($"Scene LOD data is not supported");
            }
        }

        public void RegisterRenderer(Renderer renderer)
        {
            LODGroup lod = null;

            var transform = renderer.transform;
        
            var parent = transform.parent;

            var origin = transform;

            while (parent != null && lod == null)
            {
                lod = parent.GetComponent<LODGroup>();

                if (lod != null)
                {
                    break;
                }

                origin = parent;

                parent = parent.parent;
            }

            if (lod == null)
            {
                return;
            }

            var lods = lod.GetLODs();

            var index = origin.GetSiblingIndex();

            if (index >= lods.Length)
            {
                return;
            }
        
            var level = lods[index];

            var renderers = level.renderers.Where(r => r != null).ToList();

            renderers.Add(renderer);

            level.renderers = renderers.ToArray();

            lods[index] = level;

            lod.SetLODs(lods);
        }

        public void ApplyTexturing(NiTexturingProperty property, GameObject parent)
        {
            var renderer = parent.GetComponent<Renderer>();

            if (renderer == null)
            {
                ApplyTextures.Add((parent, property));
                
                return;
            }

            if (property.HasBaseTexture)
            {
                var baseTexture = property.BaseTexture;

                var source = baseTexture.Source.Get(File);

                if (source.UseExternal != 0)
                {
                    byte[] contents;
                    
                    try
                    {
                        contents = ResourceUtilities.ReadFrom(NifPath, source.FileName.Get(File));
                    }
                    catch (Exception e)
                    { 
                        Debug.LogError(e);
                        
                        return;
                    }

                    Texture texture;

                    switch (source.AlphaFormat)
                    {
                        case AlphaFormat.ALPHA_NONE:
                            texture = ResourceUtilities.LoadTextureDxt(contents, TextureFormat.DXT1);
                            break;
                        case AlphaFormat.ALPHA_BINARY:
                        case AlphaFormat.ALPHA_SMOOTH:
                        case AlphaFormat.ALPHA_DEFAULT:
                            texture = ResourceUtilities.LoadTextureDxt(contents, TextureFormat.DXT5);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    texture.name = source.Name.Get(File);

                    var mat = new Material(WorkspaceControl.CurrentWorkspace.NormalMaterial)
                    {
                        name = texture.name
                    };

                    renderer.sharedMaterial = mat;

                    renderer.sharedMaterial.mainTexture = texture;

                    renderer.sharedMaterial.SetTextureScale(Shader.PropertyToID("_MainTex"), new Vector2(1, -1));
                }
            }
        }

        public void BuildRig(GameObject parent, NiSkinInstance skinInstance)
        {
            var mesh = parent.GetComponent<MeshFilter>().sharedMesh;

            var partitions = skinInstance.SkinPartition.Get(File);

            var oldRenderer = parent.GetComponent<MeshRenderer>();
            var mat = new Material(oldRenderer.sharedMaterial);
            Object.DestroyImmediate(oldRenderer);

            var renderer = parent.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMaterial = mat;
            renderer.sharedMesh = mesh;
            renderer.rootBone = Nodes[skinInstance.SkeletonRoot.Get(File)];

            var boneWeights = new BoneWeight[mesh.vertices.Length];
        
            RegisterRenderer(renderer);

            foreach (var partition in partitions.SkinPartitionBlocks)
            {
                for (var i = 0; i < partition.VertexMap.Length; i++)
                {
                    var index = partition.VertexMap[i];
                    var weight = new BoneWeight();

                    for (var j = 0; j < partition.NumWeightsPerVertex; j++)
                    {
                        var value = partition.BoneIndices[i, j];

                        switch (j)
                        {
                            case 0:
                                weight.boneIndex0 = value;
                                break;
                            case 1:
                                weight.boneIndex1 = value;
                                break;
                            case 2:
                                weight.boneIndex2 = value;
                                break;
                            case 3:
                                weight.boneIndex3 = value;
                                break;
                            default:
                                break;
                        }

                        var vertexWeight = partition.VertexWeights[i, j];
                        switch (j)
                        {
                            case 0:
                                weight.weight0 = vertexWeight;
                                break;
                            case 1:
                                weight.weight1 = vertexWeight;
                                break;
                            case 2:
                                weight.weight2 = vertexWeight;
                                break;
                            case 3:
                                weight.weight3 = vertexWeight;
                                break;
                            default:
                                break;
                        }
                    }

                    boneWeights[index] = weight;
                }
            }

            mesh.boneWeights = boneWeights;

            var bones = new Transform[skinInstance.NumBones];
            var poses = new Matrix4x4[skinInstance.NumBones];

            for (var index = 0; index < skinInstance.NumBones; index++)
            {
                var boneData = skinInstance.Bones[index].Get(File);
                var unityObject = Nodes[boneData].transform;
                bones[index] = unityObject;
                poses[index] = unityObject.worldToLocalMatrix * renderer.rootBone.localToWorldMatrix;
            }

            mesh.bindposes = poses;
            renderer.bones = bones;
        }

        public void BuildAmbientLight(NiAmbientLight info, GameObject parent)
        {
            /*
            var light = parent.AddOrGetComponent<Light>();

            var color = info.AmbientColor;
        
            light.color = new Color(color.r, color.g, color.b);
            light.type = LightType.Directional;
            */
        }

        public void BuildCamera(NiCamera info, GameObject parent)
        {
            /*
            var camera = parent.AddOrGetComponent<Camera>();

            camera.rect = new Rect(info.ViewportRight, info.ViewportRight, info.FrustumLeft, info.ViewportBottom);
            */
        }
    }
}