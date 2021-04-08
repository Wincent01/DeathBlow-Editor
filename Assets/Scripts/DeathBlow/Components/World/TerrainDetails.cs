using UnityEngine;
using System.Collections;
using InfectedRose.Terrain.Editing;
using DeathBlow.World;
using UnityEditor;
using InfectedRose.Terrain;
using System.Linq;
using System.IO;
using RakDotNet.IO;

[CustomEditor(typeof(TerrainDetails))]
public class TerrainDetailsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var terrainDetails = (TerrainDetails) serializedObject.targetObject;

        var terrainProperty = serializedObject.FindProperty("_terrain");
        var defaultHeightProperty = serializedObject.FindProperty("_defaultHeight");
        var sizeProperty = serializedObject.FindProperty("_size");

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(terrainProperty);
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(5);
        GUILayout.Label("New Terrain");

        defaultHeightProperty.floatValue = EditorGUILayout.FloatField("Default Height", defaultHeightProperty.floatValue);
        sizeProperty.intValue = EditorGUILayout.IntField("Size", sizeProperty.intValue);

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Generate"))
        {
            terrainDetails.Initalize();
        }

        if (GUILayout.Button("Export"))
        {
            terrainDetails.Export();
        }
    }
}

[ExecuteInEditMode]
public class TerrainDetails : MonoBehaviour
{
    [SerializeField] private GameObject _terrain;

    [SerializeField] private float _defaultHeight;

    [SerializeField] private int _size;

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

        terrain.Load();

        for (var chunkX = 0; chunkX < source.Weight; ++chunkX)
        {
            for (var chunkY = 0; chunkY < source.Height; ++chunkY)
            {
                var ochunkX = chunkX; //source.Weight - chunkX - 1;
                var ochunkY = chunkY; //source.Height - chunkY - 1;

                var chunk = chunks[ochunkY * source.Weight + ochunkX];

                var terrainChunk = source.Chunks[chunkX * source.Weight + chunkY];

                var mesh = chunk.sharedMesh;

                var heights = terrainChunk.HeightMap.Data;
                var verticies = mesh.vertices.ToArray();

                var height = 64;
                var width = 64;

                Debug.Log($"Sqrt({heights.Length}) = {Mathf.Sqrt(heights.Length)}");

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

                        var ox = x;
                        var oy = y;

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

        terrain.Apply();

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
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Debug.Log("Left Mouse was pressed");

            Vector3 screenPosition = Event.current.mousePosition;
            screenPosition.y = Camera.current.pixelHeight - screenPosition.y;
            Ray ray = Camera.current.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction, Color.red, 10);
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

                        if (Vector3.Distance(vertex + offset, hit.point) < 10)
                        {
                            Debug.Log("Hit");

                            vertex.y += 3;

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
