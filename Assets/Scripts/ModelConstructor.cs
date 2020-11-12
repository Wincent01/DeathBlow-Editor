using System;
using System.Collections.Generic;
using System.Linq;
using InfectedRose.Nif;
using UnityEngine;
using UVector3 = UnityEngine.Vector3;
using UVector4 = UnityEngine.Vector4;

public class ModelConstructor
{
    public Material Material { get; set; }
    
    public NiFile File { get; set; }
    
    public string Name { get; set; }
    
    public string Path { get; set; }
    
    public GameObject Construct()
    {
        var root = new GameObject(Name);
        
        foreach (var block in File.Blocks)
        {
            if (block is NiNode node)
            {
                BuildHierarchy(node, root);
                
                break;
            }
        }

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
            case NiNode _:
                break;
            default:
                throw new NotImplementedException($"Node type {avObject.GetType().Name} is not implemented");
        }

        BuildProperties(avObject, parent);

        return instance;
    }

    public void BuildProperties(NiAVObject avObject, GameObject parent)
    {
        foreach (var ptr in avObject.Properties)
        {
            var property = ptr.Get(File);
            
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
        
        mesh.vertices = data.HasVertices ? data.Vertices.Select(
            v => v.ToUVector()
            ).ToArray() : null;
        
        mesh.triangles = data.HasTriangles ? data.Triangles.SelectMany(
            t =>  new int[]{t.v1, t.v2, t.v3}
            ).ToArray() : new int[0];
        
        mesh.normals = data.HasNormals ? data.Normals.Select(
            v => v.ToUVector()
            ).ToArray() : null;
        
        mesh.colors = data.HasVertexColors ? data.VertexColors.Select(
            c => new Color(c.r, c.g, c.b, c.a)
            ).ToArray() : null;

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
        
        renderer.sharedMaterial = Material;

        filter.mesh = mesh;
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
        
        mesh.vertices = data.HasVertices ? data.Vertices.Select(
            v => v.ToUVector()
        ).ToArray() : null;
        
        mesh.triangles = triangles.ToArray();
        
        /*
        mesh.triangles = data.HasTriangles ? data.Triangles.SelectMany(
            t =>  new int[]{t.v1, t.v2, t.v3}
        ).ToArray() : new int[0];
        */
        
        mesh.normals = data.HasNormals ? data.Normals.Select(
            v => v.ToUVector()
        ).ToArray() : null;
        
        mesh.colors = data.HasVertexColors ? data.VertexColors.Select(
            c => new Color(c.r, c.g, c.b, c.a)
        ).ToArray() : null;

        //mesh.SetIndices(data.Points.SelectMany(p => p.Select(t => (int) t)).ToArray(), MeshTopology.LineStrip, 0);
        
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
        
        renderer.sharedMaterial = Material;

        filter.mesh = mesh;
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

    public void RegisterRenderer(MeshRenderer renderer)
    {
        LODGroup lod = null;

        var parent = renderer.transform.parent;

        var origin = parent;

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

        var level = lods[index];

        var renderers = level.renderers;

        Array.Resize(ref renderers, renderers.Length + 1);

        renderers[renderers.Length - 1] = renderer;

        level.renderers = renderers;

        lods[index] = level;

        lod.SetLODs(lods);
    }

    public void ApplyTexturing(NiTexturingProperty property, GameObject parent)
    {
        var renderer = parent.AddOrGetComponent<MeshRenderer>();

        if (property.HasBaseTexture)
        {
            var baseTexture = property.BaseTexture;
            
            var source = baseTexture.Source.Get(File);

            if (source.UseExternal != 0)
            {
                var contents = ResourceUtilities.ReadFrom(Path, source.FileName.Get(File));

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

                var material = new Material(Material.shader)
                {
                    name = texture.name
                };

                renderer.sharedMaterial = material;
            }
        }
    }
    
    public void BuildCamera(NiCamera info, GameObject parent)
    {
        var camera = parent.AddOrGetComponent<Camera>();

        camera.rect = new Rect(info.ViewportRight, info.ViewportRight, info.FrustumLeft, info.ViewportBottom);
    }
    
}