using System;
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
                BuildTriShape(File, triShape, instance);
                break;
            case NiCamera camera:
                break;
            case NiNode _:
                break;
            default:
                throw new NotImplementedException($"Node type {avObject.GetType().Name} is not implemented");
        }

        return instance;
    }

    public void BuildTriShape(NiFile file, NiTriShape shape, GameObject parent)
    {
        var geometry = shape.Data.Get(file);

        if (!(geometry is NiTriShapeData data))
        {
            throw new NotImplementedException("Invalid NiTriShape data type");
        }
        
        var mesh = new Mesh();

        mesh.name = shape.Name.Get(file);
        
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

        var filter = parent.GetComponent<MeshFilter>();

        if (filter == null)
        {
            filter = parent.AddComponent<MeshFilter>();
        }

        var renderer = parent.GetComponent<MeshRenderer>();

        if (renderer == null)
        {
            renderer = parent.AddComponent<MeshRenderer>();
        }

        renderer.material = Material;

        filter.mesh = mesh;
    }
}