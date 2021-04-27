using System;
using System.IO;
using System.Linq;
using DeathBlow.Components;
using InfectedRose.Terrain;
using InfectedRose.Terrain.Pipeline;
using RakDotNet.IO;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.World
{
    public class TerrainInterface : EditorWindow
    {
        public string Notice { get; set; }

        public string WorkingFile { get; set; }

        public GameObject Instance { get; set; }

        public Color NoticeColor { get; set; }

        [MenuItem("Death Blow/Terrain Interface")]
        public static void Initialize()
        {
            var window = GetWindow<TerrainInterface>("Terrain Interface");

            window.Show();
        }
        
        private void OnGUI()
        {
            if (!string.IsNullOrWhiteSpace(Notice))
            {
                var style = new GUIStyle(EditorStyles.textField);
                style.normal.textColor = NoticeColor;

                if (GUILayout.Button($"{Notice}!", style))
                {
                    Notice = "";
                }
            }

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Working file");

            var selected = !string.IsNullOrWhiteSpace(WorkingFile);

            var display = !selected ? "Select..." : WorkingFile;

            if (GUILayout.Button(display))
            {
                WorkingFile = EditorUtility.OpenFilePanelWithFilters(
                                "Select terrain file...",
                                selected ? Path.GetDirectoryName(WorkingFile) : ResourceUtilities.SearchRoot,
                                new[] {"Terrain (*.raw)", "raw"}
                );
            }

            EditorGUILayout.EndHorizontal();
            
            if (!selected) return;
            
            GUILayout.Label("Actions");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Import"))
            {
                Instance = Import(WorkingFile);
            }

            if (GUILayout.Button("Export"))
            {
                Export(Instance);
            }

            EditorGUILayout.EndHorizontal();
        }


        public static GameObject Import(string workingFile)
        {
            using var stream = File.OpenRead(workingFile);
            using var reader = new BitReader(stream);

            var terrain = new TerrainFile();

            terrain.Deserialize(reader);

            return Import(terrain);
        }

        public static GameObject Import(TerrainFile terrain)
        {
            const float scale = 3.2f; //3.125f;

            var terrainInstance = new GameObject($"Terrain");
            
            var centerX = 204.8f / 2;
            var centerY = -204.8f / 2;

            for (var x = 0; x < terrain.Weight; x++)
            {
                for (var y = 0; y < terrain.Height; y++)
                {
                    var chunk = terrain.Chunks[x + y * terrain.Height];

                    if (Directory.Exists("lightmap/"))
                    {
                        File.WriteAllBytes($"lightmap/lightmap_{x}_{y}.dds", chunk.Lightmap.Data);
                    }

                    if (Directory.Exists("blendmap/"))
                    {
                        File.WriteAllBytes($"blendmap/blendmap_{x}_{y}.dds", chunk.Blendmap.Data);
                    }

                    var triangles = chunk.Triangulate();
                    
                    var instance = new GameObject($"Chunk ({x}, {y})");

                    instance.transform.parent = terrainInstance.transform;
                    
                    var mesh = new Mesh();
                    
                    mesh.vertices = triangles.SelectMany(t => t.ToArray.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray()).ToArray();

                    /*
                    var colors = new Color[mesh.vertices.Length];

                    for (var i = 0; i < mesh.vertices.Length; i++)
                    {
                        colors[i] = Color.green;
                    }
                    */

                    mesh.colors = Enumerable.Repeat(new Color(0, 1, 0), mesh.vertices.Length).ToArray();
                    
                    //chunk.GenerateColorArray(ref colors, true);
                    
                    mesh.triangles = Enumerable.Range(0, triangles.Length * 3).ToArray();
                    
                    mesh.RecalculateNormals();

                    /*
                    if (colors.Length == mesh.vertices.Length)
                    {
                        mesh.colors = colors.Select(c => ).ToArray();
                    }
                    else
                    {
                        Debug.Log($"{chunk.Colormap0.Size}/{chunk.Colormap0.Size} vs {chunk.HeightMap.Height}/{chunk.HeightMap.Width}");
                        mesh.colors = Enumerable.Repeat(new Color(0, 1 - UnityEngine.Random.value * 0.1f, 0), mesh.vertices.Length).ToArray();
                    }
                    */

                    //mesh.colors = colors;
                    
                    var filter = instance.AddComponent<MeshFilter>();

                    filter.mesh = mesh;

                    var render = instance.AddComponent<MeshRenderer>();

                    render.material = WorkspaceControl.CurrentWorkspace.TerrainMaterial;
                    
                    instance.transform.position += new Vector3(centerX + x * -204.8f + 3.2f / 4, 0, centerY + y * 204.8f);
                    
                    for (var index = 0; index < chunk.Foliage.Count; index++)
                    {
                        var foliage = chunk.Foliage[index];
                        
                        var foliageObject = new GameObject($"Foliage {foliage.Type} ({index})");
                        
                        foliageObject.transform.position = new Vector3(foliage.Position.X, foliage.Position.Y, foliage.Position.Z);
                        foliageObject.transform.rotation = new Quaternion(foliage.Rotation.X, foliage.Rotation.Y, foliage.Rotation.Z, foliage.Rotation.W);

                        foliageObject.transform.parent = instance.transform;
                    }
                }
            }
            
            terrainInstance.transform.position = new Vector3(0, 0, 0); //-new Vector3(centerX, 0, centerY) * scale;

            return terrainInstance;
        }
        
        public static void Export(GameObject terrainInstance)
        {
            
        }
    }
}