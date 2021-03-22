using System;
using System.IO;
using System.Linq;
using DeathBlow.Components;
using InfectedRose.Luz;
using InfectedRose.Lvl;
using InfectedRose.Terrain;
using InfectedRose.Terrain.Pipeline;
using RakDotNet.IO;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.World
{
    public class ZoneInterface : EditorWindow
    {
        public string Notice { get; set; }

        public string WorkingFile { get; set; }

        public GameObject Instance { get; set; }

        public Color NoticeColor { get; set; }

        [MenuItem("Death Blow/Zone Interface")]
        public static void Initialize()
        {
            var window = GetWindow<ZoneInterface>("Zone Interface");

            window.Show();
        }
        
        private void OnGUI()
        {
            if (!WorkspaceControl.Ok)
            {
                GUILayout.Label("Workspace not loaded.");

                return;
            }

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
                                "Select zone file...",
                                selected ? Path.GetDirectoryName(WorkingFile) : ResourceUtilities.SearchRoot,
                                new[] {"Zone (*.luz)", "luz"}
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
                Export();
            }

            EditorGUILayout.EndHorizontal();
        }

        public static GameObject Import(string workingFile)
        {
            const float scale = 3.125f;
            
            using var stream = File.OpenRead(workingFile);
            using var reader = new BitReader(stream);
            
            var zone = new LuzFile();
            
            zone.Deserialize(reader);
            
            var zoneInstance = new GameObject($"Zone {Path.GetFileName(workingFile)}");

            var sourceDir = Path.GetDirectoryName(workingFile) ?? WorkspaceControl.CurrentWorkspace.AssetPath;
            
            var terrain =  TerrainInterface.Import(Path.Combine(
                            sourceDir,
                            zone.TerrainFileName)
            );

            terrain.transform.parent = zoneInstance.transform;

            foreach (var sceneInfo in zone.Scenes)
            {
                var scenePath = Path.Combine(sourceDir, sceneInfo.FileName);
                
                var sceneInstance = new GameObject($"Scene {sceneInfo.SceneName} ({sceneInfo.SceneId}, {sceneInfo.LayerId})");

                sceneInstance.transform.parent = zoneInstance.transform;
                
                using var lvlStream = File.OpenRead(scenePath);
                using var lvlReader = new BitReader(lvlStream);
                
                var lvl = new LvlFile();

                lvl.Deserialize(lvlReader);
                
                if (lvl.LevelObjects == null) continue;

                foreach (var template in lvl.LevelObjects.Templates)
                {
                    GameObject lwoObject;
                    
                    try
                    {
                        lwoObject = ObjectInterface.Import(template.Lot, out var error);

                        if (lwoObject == null)
                        {
                            Debug.LogError(error);
                        
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        
                        continue;
                    }

                    lwoObject.transform.parent = sceneInstance.transform;

                    lwoObject.transform.position = new Vector3(template.Position.X, template.Position.Y, template.Position.Z);
                    lwoObject.transform.rotation = new Quaternion(template.Rotation.X, template.Rotation.Y, template.Rotation.Z, template.Rotation.W);
                }
            }

            return zoneInstance;
        }
        
        public void Export()
        {
            
        }
    }
}