using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InfectedRose.Terrain.Editing;
using DeathBlow.World;
using UnityEditor;
using InfectedRose.Terrain;
using System.Linq;
using System.IO;
using RakDotNet.IO;
using UnityEngine.EventSystems;
using DeathBlow;
using NativeColor = System.Drawing.Color;

[CustomEditor(typeof(TerrainDetails))]
public class TerrainDetailsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var terrainDetails = (TerrainDetails) serializedObject.targetObject;

        var terrainProperty = serializedObject.FindProperty("_terrain");
        var defaultHeightProperty = serializedObject.FindProperty("_defaultHeight");
        var sizeProperty = serializedObject.FindProperty("_size");
        var editingProperty = serializedObject.FindProperty("_editing");
        var lightmapProperty = serializedObject.FindProperty("_lightmap");
        var blendmapProperty = serializedObject.FindProperty("_blendmap");
        var radiusProperty = serializedObject.FindProperty("_radius");
        var powerProperty = serializedObject.FindProperty("_power");
        var smoothFactorProperty = serializedObject.FindProperty("_smoothFactor");
        var colorProperty = serializedObject.FindProperty("_color");
        var modeProperty = serializedObject.FindProperty("_mode");
        var setHeightProperty = serializedObject.FindProperty("_setHeight");

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(terrainProperty);
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(5);
        GUILayout.Label("New Terrain");

        defaultHeightProperty.floatValue = EditorGUILayout.FloatField("Default Height", defaultHeightProperty.floatValue);
        sizeProperty.intValue = EditorGUILayout.IntField("Size", sizeProperty.intValue);

        if (GUILayout.Button("Generate"))
        {
            terrainDetails.Initalize();
        }
        
        GUILayout.Space(5);
        GUILayout.Label("Editing");
        EditorGUILayout.PropertyField(modeProperty);
        EditorGUILayout.PropertyField(editingProperty);
        EditorGUILayout.PropertyField(radiusProperty);
        EditorGUILayout.PropertyField(powerProperty);
        EditorGUILayout.PropertyField(smoothFactorProperty);
        EditorGUILayout.PropertyField(colorProperty);
        EditorGUILayout.PropertyField(setHeightProperty);

        GUILayout.Space(5);
        GUILayout.Label("Export settings");

        EditorGUILayout.PropertyField(lightmapProperty);
        EditorGUILayout.PropertyField(blendmapProperty);

        if (GUILayout.Button("Export"))
        {
            terrainDetails.Export();
        }

        serializedObject.ApplyModifiedProperties();
    }
}

[ExecuteInEditMode]
public class TerrainDetails : MonoBehaviour
{
    public enum EditMode
    {
        Terrain,
        Color,
        SetHeight,
        Smooth
    }
    
    [SerializeField] private GameObject _terrain;

    [SerializeField] private float _defaultHeight;

    [SerializeField] private int _size;

    [SerializeField] private bool _editing;

    [SerializeField] private string _lightmap;

    [SerializeField] private string _blendmap;

    [SerializeField] private EditMode _mode;
    
    [SerializeField] private float _radius;

    [SerializeField] private float _power;
    
    [SerializeField] private float _smoothFactor;

    [SerializeField] private Color _color;

    [SerializeField] private float _setHeight;

    public TerrainEditor Editor { get; set; }

    public GameObject Terrain { get => _terrain; set => _terrain = value; }
    
    public float DefaultHeight { get => _defaultHeight; set => _defaultHeight = value; }

    public int Size { get => _size; set => _size = value; }

    public void Initalize()
    {
        if (_terrain != null)
        {
            DestroyImmediate(_terrain);
        }

        var settings = new TerrainSettings();

        settings.DefaultHeight = _defaultHeight;
        settings.Size = _size;

        var terrain = TerrainEditor.OpenEmptyAsync(settings).Result;

        var terrainInstance = TerrainInterface.Import(terrain.Source);

        terrainInstance.transform.parent = transform;

        _terrain = terrainInstance;

        var chunks = GetComponentsInChildren<MeshFilter>();

        for (var i = 0; i < chunks.Length; i++)
        {
            var chunk = chunks[i];

            chunk.gameObject.AddComponent<MeshCollider>();
        }
    }

    public TerrainEditor Export()
    {
        var settings = new TerrainSettings();

        //settings.ChunkSpacing = 3.2f * settings.ChunkSize;
        settings.DefaultHeight = _defaultHeight;
        settings.Size = _size;

        var terrain = TerrainEditor.OpenEmptyAsync(settings).Result;

        var source = terrain.Source;

        var chunks = GetComponentsInChildren<MeshFilter>();

        terrain.HeightLayer.LoadHeightMap();
        //terrain.ColorLayer.LoadColorMap();

        var maxWidth = terrain.Source.Weight * 64;

        for (var chunkX = 0; chunkX < source.Weight; ++chunkX)
        {
            for (var chunkY = 0; chunkY < source.Height; ++chunkY)
            {
                var ochunkX = chunkX; //source.Weight - chunkX - 1;
                var ochunkY = chunkY; //source.Height - chunkY - 1;

                var chunk = chunks[ochunkY * source.Weight + ochunkX];

                var terrainChunk = source.Chunks[chunkX * source.Weight + chunkY];

                terrainChunk.ColorRelatedArray = Enumerable.Repeat((byte) 0, 1024).ToArray(); //new byte[1024];
                terrainChunk.Colormap0.Size = 32;
                terrainChunk.Colormap0.Data = Enumerable.Repeat(/*System.Drawing.Color.FromArgb(255, 82, 18, 18)*/
                                System.Drawing.Color.FromArgb(255, 0, 0, 0), 32 * 32).ToArray();
                terrainChunk.Colormap1.Size = 128;
                terrainChunk.Colormap1.Data = Enumerable.Repeat(NativeColor.FromArgb(255, 0, 0, 0), 128 * 128).ToArray();
                terrainChunk.TextureSetting = 1;
                terrainChunk.UnknownByteArray1 = new byte[32];
                for (var i = 0; i < 32; i += 2)
                {
                    terrainChunk.UnknownByteArray1[i] = 5;
                    terrainChunk.UnknownByteArray1[i + 1] = 0;
                }
                terrainChunk.UnknownShortArray = new short[16][];
                for (var i = 0; i < 16; ++i)
                {
                    terrainChunk.UnknownShortArray[i] = new short[9] { 3, 0, 4, 1, 2, 0, 4, 0, 2 };
                }

                if (File.Exists(_lightmap))
                {
                    terrainChunk.Lightmap.Data = File.ReadAllBytes(_lightmap);
                }

                if (File.Exists(_blendmap))
                {
                    terrainChunk.Blendmap.Data = File.ReadAllBytes(_blendmap);
                }
                /*
                terrainChunk.ShortMap.Data = new short[4225];
                terrainChunk.ShortMap.Data[0] = -1;*/

                var mesh = chunk.sharedMesh;

                var heights = terrainChunk.HeightMap.Data;
                var verticies = mesh.vertices.ToArray();
                var colors = mesh.colors.ToArray();

                var height = 64;
                var width = 64;

                for (var x = 0; x < width; ++x)
                {
                    for (var y = 0; y < height; ++y)
                    {
                        // "WTF" Wonderful Triangulation Function 

                        /*
                        var ox = x + 1;
                        var oy = y + 1;

                        var value = verticies[(x * 6) * width + (y * 6)].y;

                        heights[ox * width + oy + x] = value;

                        if (ox == 64)
                        {
                            heights[(ox + 1) * width + oy + x] = value;
                        }
                        if (oy == 64)
                        {
                            heights[ox * width + (oy + 1) + x] = value;
                        }
                        if (ox == 64 && oy == 64)
                        {
                            heights[(ox + 1) * width + (oy + 1) + x] = value;
                        }
                        */

                        var index = ((x) * 6) * width + ((y) * 6);
                        if (index < 0 || index >= verticies.Length) continue;
                        var value = verticies[index].y;
                        var sourceColor = colors[(x * 6) * width + (y * 6)];
                        var color = NativeColor.FromArgb((int) sourceColor.a, (int) sourceColor.r, (int) sourceColor.g, (int) sourceColor.b);

                        terrain.HeightLayer.SetHeight(new System.Numerics.Vector2(width - (x + 1) + (chunkX * 64),  (y) + (chunkY * 64)), value);

                        /*
                        if (x == 63)
                        {
                            terrain.HeightLayer.SetHeight(new System.Numerics.Vector2(width - (x + 1) + (chunkX * 64),  y + (chunkY * 64)), value);
                        }
                        
                        if (y == 63)
                        {
                            terrain.HeightLayer.SetHeight(new System.Numerics.Vector2(width - x + (chunkX * 64),  (y + 1) + (chunkY * 64)), value);
                        }

                        if (x == 63 && y == 63)
                        {
                            terrain.HeightLayer.SetHeight(new System.Numerics.Vector2(width - (x + 1) + (chunkX * 64),  (y + 1) + (chunkY * 64)), value);
                        }
                        */
                        
                        /*if (x % 2 == 0 && y % 2 == 0)
                        {
                            terrainChunk.Colormap0.Data[(x / 2) + (y / 2) * height / 2] = color;
                        }*/
                        //terrain.ColorLayer.SetColor(new System.Numerics.Vector2(x + (chunkX * 64), y + (chunkY * 64)), color);

                        /*
                        heights[ox * width + oy + x] = value;

                        if (x == 63)
                        {
                            heights[64 * width + oy + x] = value;
                        }
                        if (y == 63)
                        {
                            heights[ox * width + 64 + x] = value;
                        }
                        if (x == 63 && y == 63)
                        {
                            heights[64 * width + 64 + x] = value;
                        }
                        */
                    }
                }

                //terrainChunk.HeightMap.Data = heights;
            }
        }

        /*
        var map = Utilities.CloneDictionary(terrain.HeightLayer.Heights);

        foreach (var pair in map)
        {
            var pos = pair.Key;
            pos.X = maxWidth - pos.X;

            terrain.HeightLayer.SetHeight(pos, pair.Value);
        }
        */

        terrain.HeightLayer.ApplyHeightMap();
        //terrain.ColorLayer.ApplyColorMap();

        using var stream = File.Create("./tmp.raw");

        using var writer = new BitWriter(stream);

        source.Serialize(writer);

        Debug.Log("Exported terrain.");

        return terrain;
    }

    public void BeginEdit()
    {
    }

    private void OnEnable()
    {
        if (!Application.isEditor)
        {
            Destroy(this);
        }

        SceneView.duringSceneGui += OnScene;
    }

    void OnScene(SceneView scene)
    {
        if (!_editing)
        {
            return;
        }

        var e = Event.current;

        if (e.button == 0)
        {
            if (e.type != EventType.MouseDown && e.type != EventType.Used)
            {
                return;
            }
            
            //Debug.Log("Left Mouse was pressed");

            /*
            Vector3 screenPosition = Event.current.mousePosition;
            screenPosition.y = Camera.current.pixelHeight - screenPosition.y;
            Ray ray = Camera.current.ScreenPointToRay(screenPosition);
            */
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            //Debug.Log(ray.origin);

            if (Physics.Raycast(ray, out hit))
            {
                /*
                Editor.HeightLayer.LoadHeightMap();

                var brush = new HeightBrush(Editor);

                brush.Apply(new Vector2(hit.point.x, hit.point.y));
                */

                Debug.Log(hit.point);
                //Do something, ---Example---

                var chunks = GetComponentsInChildren<MeshFilter>();

                for (var i = 0; i < chunks.Length; i++)
                {
                    var chunk = chunks[i];

                    var mesh = chunk.sharedMesh;

                    var vertices = mesh.vertices;
                    var colors = mesh.colors;

                    var offset = chunk.transform.position;

                    var any = false;

                    var maxX = vertices.Max(v => v.x);

                    var localToWorld = chunk.transform.localToWorldMatrix;

                    var selected = new List<int>();

                    var total = 0.0f;
                    
                    for (var j = 0; j < vertices.Length; ++j)
                    {
                        var vertex = vertices[j];

                        var a = localToWorld.MultiplyPoint3x4(vertex);
                        var b = hit.point;

                        a.y = b.y;

                        var distance = Vector3.Distance(a, b);

                        if (distance < _radius)
                        {
                            if (_mode == EditMode.Smooth)
                            {
                                total += vertex.y;
                                selected.Add(j);
                                
                                any = true;
                            
                                continue;
                            }

                            //Debug.Log("Hit");

                            var multiplier = (float) (1 / (Math.Pow(_smoothFactor, distance) + 1));

                            var power = _power * multiplier;

                            switch (_mode)
                            {
                                case EditMode.Terrain:
                                    if (e.shift)
                                    {
                                        vertex.y -= power;
                                    }
                                    else
                                    {
                                        vertex.y += power;
                                    }

                                    if (vertex.y > _setHeight)
                                    {
                                        vertex.y = _setHeight;
                                    }
                                    break;
                                case EditMode.Color:
                                    colors[j] = MoveTowards(colors[j], _color, power);
                                    break;
                                case EditMode.SetHeight:
                                    vertex.y = _setHeight;
                                    break;
                                case EditMode.Smooth:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            
                            any = true;

                            vertices[j] = vertex;
                        }
                    }

                    if (_mode == EditMode.Smooth)
                    {
                        var avg = total / selected.Count;
                        
                        foreach (var selectedIndex in selected)
                        {
                            var vertex = vertices[selectedIndex];

                            var a = localToWorld.MultiplyPoint3x4(vertex);
                            var b = hit.point;

                            a.y = b.y;

                            var distance = Vector3.Distance(a, b);

                            var multiplier = (float) (1 / (Math.Pow(_smoothFactor, distance) + 1));

                            var power = _power * multiplier;

                            vertex.y = MoveTowards(vertex.y, (avg + vertex.y) / 2, power);

                            vertices[selectedIndex] = vertex;
                        }
                    }

                    chunk.GetComponent<MeshCollider>().sharedMesh = mesh;

                    if (any)
                    {
                        mesh.vertices = vertices;
                        mesh.colors = colors;
                    }
                }
            }

            e.Use();
        }
    }
    
    private static Color MoveTowards(
                    Color current,
                    Color target,
                    float maxDistanceDelta)
    {
        var r = target.r - current.r;
        var g = target.g - current.g;
        var b = target.b - current.b;
        var d = (float) (r * (double) r + g * (double) g + b * (double) b);
        if (d == 0.0 || maxDistanceDelta >= 0.0 && d <= maxDistanceDelta * (double) maxDistanceDelta)
            return target;
        var num5 = (float) Math.Sqrt(d);
        return new Color(current.r + r / num5 * maxDistanceDelta, current.g + g / num5 * maxDistanceDelta, current.b + b / num5 * maxDistanceDelta, current.a);
    }

    private static float MoveTowards(float current, float target, float maxDistanceDelta)
    {
        var r = target - current;
        var d = r * r;
        if (d == 0.0 || maxDistanceDelta >= 0.0 && d <= maxDistanceDelta * (double) maxDistanceDelta)
            return target;
        var num5 = (float) Math.Sqrt(d);
        return current + r / num5 * maxDistanceDelta;
    }

    public void EndEdit()
    {

    }
}
