using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeathBlow.Components;
using DeathBlow.Components.Game;
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
                                selected ? Path.GetDirectoryName(WorkingFile) : Path.Combine(ResourceUtilities.SearchRoot, "maps/"),
                                new[] {"Zone (*.luz)", "luz"}
                );
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Actions");

            if (GUILayout.Button("New"))
            {
                Instance = New();
            }

            if (!selected) return;

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

        public static GameObject New()
        {
            var zoneInstance = new GameObject($"New Zone");

            var spawnPoint = new GameObject("Zone Spawnpoint");

            spawnPoint.transform.parent = zoneInstance.transform;

            var zoneDetails = zoneInstance.AddOrGetComponent<ZoneDetails>();

            zoneDetails.ZoneName = "New Zone";
            
            zoneDetails.ZoneId = 5000;
            
            zoneDetails.SpawnPoint = spawnPoint.transform;

            var sceneInstance = new GameObject($"Global Scene");

            sceneInstance.transform.parent = zoneInstance.transform;

            var sceneDetails = sceneInstance.AddOrGetComponent<SceneDetails>();

            sceneDetails.SceneName = "Global Scene";

            sceneDetails.SceneLayer = 0;

            var terrainInstance = new GameObject($"Terrain");

            terrainInstance.transform.parent = zoneInstance.transform;

            var terrainDetails = terrainInstance.AddOrGetComponent<TerrainDetails>();

            terrainDetails.Initalize();

            return zoneInstance;
        }

        public static GameObject Import(string workingFile)
        {
            const float scale = 3.125f;
            
            using var stream = File.OpenRead(workingFile);
            using var reader = new BitReader(stream);
            
            var zone = new LuzFile();
            
            zone.Deserialize(reader);

            var zoneInstance = new GameObject($"Zone {Path.GetFileName(workingFile)}");

            var zoneDetails = zoneInstance.AddOrGetComponent<ZoneDetails>();

            zoneDetails.ZoneName = Path.GetFileName(workingFile);

            zoneDetails.ZoneId = zone.WorldId;

            var spawnPoint = new GameObject("Zone Spawnpoint");

            spawnPoint.transform.parent = zoneInstance.transform;

            spawnPoint.transform.position = new Vector3(zone.SpawnPoint.X, zone.SpawnPoint.Y, zone.SpawnPoint.Z);
            spawnPoint.transform.rotation = new Quaternion(zone.SpawnRotation.X, zone.SpawnRotation.Y, zone.SpawnRotation.Z, zone.SpawnRotation.W);

            zoneDetails.SpawnPoint = spawnPoint.transform;

            var sourceDir = Path.GetDirectoryName(workingFile) ?? WorkspaceControl.CurrentWorkspace.AssetPath;
            
            var terrain = TerrainInterface.Import(Path.Combine(
                            sourceDir,
                            zone.TerrainFileName)
            );

            terrain.transform.parent = zoneInstance.transform;

            foreach (var sceneInfo in zone.Scenes)
            {
                var scenePath = Path.Combine(sourceDir, sceneInfo.FileName);
                
                var sceneInstance = new GameObject($"Scene {sceneInfo.SceneName} ({sceneInfo.SceneId}, {sceneInfo.LayerId})");

                var sceneDetails = sceneInstance.AddOrGetComponent<SceneDetails>();

                sceneInstance.transform.parent = zoneInstance.transform;

                using var lvlStream = File.OpenRead(scenePath);
                using var lvlReader = new BitReader(lvlStream);
                
                var lvl = new LvlFile();

                lvl.Deserialize(lvlReader);

                sceneDetails.SceneName = sceneInfo.SceneName;

                sceneDetails.SceneLayer = sceneInfo.LayerId;

                sceneDetails.SkyBox = lvl.LevelSkyConfig == null ? sceneDetails.SkyBox : lvl.LevelSkyConfig.Skybox;
                
                if (lvl.LevelObjects == null) continue;

                foreach (var info in lvl.LevelObjects.Templates)
                {
                    Transform root = null;
                    ObjectDetails details = null;

                    if (info.Lot == 176)
                    {
                        var spawnTemplate = (int) info.LegoInfo["spawntemplate"];
                        
                        var spawner = ImportTemplate(sceneInstance.transform, 176, $"[Spawner] {spawnTemplate}");
                        
                        details = ImportTemplate(sceneInstance.transform, spawnTemplate, $"[{spawnTemplate}]");

                        spawner.SpawnerTemplate = details.gameObject;

                        details.transform.parent = spawner.transform;

                        details.IsSpawned = true;

                        spawner.SetEntry("spawntemplate", ObjectDataType.Int32, info.Lot.ToString());
                        spawner.SetEntry("respawn", ObjectDataType.Float32, info.LegoInfo.TryGetValue("respawn", out var respawn) ? respawn.ToString() : "0");

                        root = spawner.transform;
                    }
                    else
                    {
                        details = ImportTemplate(sceneInstance.transform, info.Lot, $"[{info.Lot}]");

                        root = details.transform;
                    }

                    root.localScale = new Vector3(info.Scale, info.Scale, info.Scale);

                    foreach (var pair in info.LegoInfo)
                    {
                        if (pair.Key == "spawntemplate" || pair.Key == "respawn")
                        {
                            continue;
                        }
                        
                        var value = pair.Value;
                        
                        var type = value switch
                        {
                            int _ => 1,
                            float _ => 3,
                            double _ => 4,
                            uint _ => 5,
                            bool _ => 7,
                            long _ => 8,
                            byte[] _ => 13,
                            _ => 0
                        };

                        details.SetEntry(pair.Key, (ObjectDataType) type, value.ToString());
                    }

                    root.position = new Vector3(info.Position.X, info.Position.Y, info.Position.Z);
                    root.rotation = new Quaternion(info.Rotation.X, info.Rotation.Y, info.Rotation.Z, info.Rotation.W);

                    if (info.LegoInfo.TryGetValue("renderDisabled", out var renderDisabled) &&
                        (bool) renderDisabled)
                    /*if (info.LegoInfo.TryGetValue("loadOnClientOnly", out var loadOnClientOnly) &&
                        (bool) loadOnClientOnly ||
                        info.LegoInfo.TryGetValue("loadSrvrOnly", out var loadSrvrOnly) && (bool) loadSrvrOnly ||
                        info.LegoInfo.TryGetValue("trigger_id", out var triggerId))*/
                    {
                        var temp = details.GetComponentInChildren<GameTemplate>();

                        if (temp != null)
                        {
                            temp.gameObject.SetActive(false);
                        }
                    }
                }
            }

            var map = new List<(Transform, Vector3, Quaternion)>();
            
            var zoneScale = zoneInstance.transform.localScale;
            zoneScale.z *= -1;
            zoneInstance.transform.localScale = zoneScale;

            foreach (var sceneDetails in zoneDetails.GetComponentsInChildren<SceneDetails>())
            {
                var objects = sceneDetails.GetComponentsInChildren<ObjectDetails>().Where(o => !o.IsSpawned).ToArray();
                
                foreach (var obj in objects)
                {
                    var root = obj.transform;

                    var position = root.position;
                    var rotation = root.rotation;
                    
                    map.Add((root, position, rotation));
                }
            }
            
            zoneScale = zoneInstance.transform.localScale;
            zoneScale.z *= -1;
            zoneInstance.transform.localScale = zoneScale;
            
            foreach (var (transform, vector3, quaternion) in map)
            {
                transform.SetPositionAndRotation(vector3, quaternion);
                
                var objectScale = transform.localScale;
                objectScale.z *= -1;
                transform.localScale = objectScale;
                
                var position = transform.position;
                var rotation = transform.rotation;
                
                transform.SetPositionAndRotation(position, rotation);
            }
            
            return zoneInstance;
        }
        
        public static ObjectDetails ImportTemplate(Transform scene, int lot, string objectName)
        {
            try
            {
                var template = ObjectInterface.Import(lot, out var error);

                if (template == null)
                {
                    Debug.LogError(error);
                }

                var zoneObject = new GameObject(objectName);

                var objectDetails = zoneObject.AddOrGetComponent<ObjectDetails>();

                objectDetails.Lot = lot;

                zoneObject.transform.parent = scene.transform;

                template.transform.parent = zoneObject.transform;

                return objectDetails;
            }
            catch (Exception e)
            {
                Debug.LogError(e);

                var zoneObject = new GameObject(objectName);
                
                var template = new GameObject("Failed to load");

                var objectDetails = zoneObject.AddComponent<ObjectDetails>();
                
                objectDetails.Lot = lot;

                zoneObject.transform.parent = scene.transform;

                template.transform.parent = zoneObject.transform;

                return objectDetails;
            }
        }
        
        public void Export()
        {
            
        }
    }
}