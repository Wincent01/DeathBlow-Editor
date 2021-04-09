using UnityEngine;
using System.Collections;
using InfectedRose.Terrain.Editing;
using DeathBlow.World;
using UnityEditor;
using InfectedRose.Terrain;
using System.Linq;
using System.IO;
using RakDotNet.IO;
using UnityEngine.EventSystems;

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

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(terrainProperty);
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(5);
        GUILayout.Label("New Terrain");

        defaultHeightProperty.floatValue = EditorGUILayout.FloatField("Default Height", defaultHeightProperty.floatValue);
        sizeProperty.intValue = EditorGUILayout.IntField("Size", sizeProperty.intValue);
        EditorGUILayout.PropertyField(editingProperty);

        if (GUILayout.Button("Generate"))
        {
            terrainDetails.Initalize();
        }

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
    [SerializeField] private GameObject _terrain;

    [SerializeField] private float _defaultHeight;

    [SerializeField] private int _size;

    [SerializeField] private bool _editing;

    [SerializeField] private string _lightmap;

    [SerializeField] private string _blendmap;

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

        for (int i = 0; i < chunks.Length; i++)
        {
            var chunk = chunks[i];

            chunk.gameObject.AddComponent<MeshCollider>();
        }
    }

    public TerrainEditor Export()
    {
        var settings = new TerrainSettings();

        settings.DefaultHeight = _defaultHeight;
        settings.Size = _size;

        var terrain = TerrainEditor.OpenEmptyAsync(settings).Result;

        var source = terrain.Source;

        var chunks = GetComponentsInChildren<MeshFilter>();

        terrain.HeightLayer.LoadHeightMap();

        for (var chunkX = 0; chunkX < source.Weight; ++chunkX)
        {
            for (var chunkY = 0; chunkY < source.Height; ++chunkY)
            {
                var ochunkX = chunkX; //source.Weight - chunkX - 1;
                var ochunkY = chunkY; //source.Height - chunkY - 1;

                var chunk = chunks[ochunkY * source.Weight + ochunkX];

                var terrainChunk = source.Chunks[chunkX * source.Weight + chunkY];

                terrainChunk.ColorRelatedArray = new byte[1024];
                terrainChunk.Colormap0.Data = Enumerable.Repeat(System.Drawing.Color.FromArgb(255, 82, 18, 18), 1024).ToArray();
                terrainChunk.Colormap1.Size = 128;
                terrainChunk.Colormap1.Data = Enumerable.Repeat(System.Drawing.Color.FromArgb(255, 0, 0, 0), 128 * 128).ToArray();
                terrainChunk.TextureSetting = 9;
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

                        var value = verticies[(x * 6) * width + (y * 6)].y;

                        terrain.HeightLayer.SetHeight(new System.Numerics.Vector2(x + (chunkX * 64), y + (chunkY * 64)), value);

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

        terrain.HeightLayer.ApplyHeightMap();

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

        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Debug.Log("Left Mouse was pressed");

            /*
            Vector3 screenPosition = Event.current.mousePosition;
            screenPosition.y = Camera.current.pixelHeight - screenPosition.y;
            Ray ray = Camera.current.ScreenPointToRay(screenPosition);
            */
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            Debug.Log(ray.origin);

            if (Physics.Raycast(ray, out hit))
            {
                /*
                Editor.HeightLayer.LoadHeightMap();

                var brush = new HeightBrush(Editor);

                brush.Apply(new Vector2(hit.point.x, hit.point.y));
                */

                Debug.Log("Instantiated Primitive " + hit.point);
                //Do something, ---Example---

                var chunks = GetComponentsInChildren<MeshFilter>();

                for (int i = 0; i < chunks.Length; i++)
                {
                    var chunk = chunks[i];

                    var mesh = chunk.sharedMesh;

                    var vertices = mesh.vertices;

                    var offset = chunk.transform.position;

                    var any = false;

                    for (var j = 0; j < vertices.Length; ++j)
                    {
                        var vertex = vertices[j];

                        var a = vertex + offset;
                        var b = hit.point;

                        a.y = b.y;

                        if (Vector3.Distance(a, b) < 10)
                        {
                            Debug.Log("Hit");

                            if (e.shift)
                            {
                                vertex.y -= 3;
                            }
                            else
                            {
                                vertex.y += 3;
                            }

                            chunk.GetComponent<MeshCollider>().sharedMesh = mesh;

                            any = true;

                            vertices[j] = vertex;
                        }
                    }

                    if (any)
                    {
                        mesh.vertices = vertices;
                    }
                }
            }

            e.Use();
        }
    }

    public void EndEdit()
    {

    }
}
