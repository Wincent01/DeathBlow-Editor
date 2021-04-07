using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DeathBlow.Components;
using DeathBlow.Components.Game;
using InfectedRose.Database.Concepts;
using InfectedRose.Database.Fdb;
using InfectedRose.Database.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DeathBlow
{
    public class ObjectInterface : EditorWindow
    {
        public string Notice { get; set; }

        public int Lot { get; set; }

        public GameObject Instance { get; set; }

        public Color NoticeColor { get; set; }

        public bool SavePrefab { get; set; } = true;

        [MenuItem("Death Blow/Object Interface")]
        public static void Initialize()
        {
            var window = GetWindow<ObjectInterface>("Object Interface");

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

            Lot = EditorGUILayout.IntField("LOT", Lot);
            
            GUILayout.Label("Actions");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Import"))
            {
                Instance = Import(Lot, out var error);
                
                if (Instance == null)
                {
                    Notice = error;
                    NoticeColor = Color.red;
                }
            }

            if (GUILayout.Button("Export"))
            {
                Export();
            }

            EditorGUILayout.EndHorizontal();
        }

        public static GameObject Import(int lot, out string error)
        {
            error = "";

            Directory.CreateDirectory(WorkspaceControl.CurrentWorkspace.AssetObjectsPath);

            var prefabPath = $"{WorkspaceControl.CurrentWorkspace.AssetObjectsPath}/{lot}.prefab";

            Debug.Log(prefabPath);

            var existing = (GameObject) AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));

            GameObject instance;

            if (existing != null)
            {
                instance = (GameObject) PrefabUtility.InstantiatePrefab(existing);

                return instance;
            }
            
            var template = WorkspaceControl.Database.LoadObject(lot);

            if (template == null)
            {
                error = $"Failed to find object with template {lot}";
                
                return null;
            }

            var templateName = template.Row.Value<string>("name");
            
            instance = new GameObject(templateName);

            var templateComponent = instance.AddComponent<GameTemplate>();

            templateComponent.Lot = template.Row.Key;
            
            var gameComponents = new List<GameComponent>();
            
            foreach (var component in template)
            {
                if (!WorkspaceControl.ComponentRegistry.TryGetValue(component.Id, out var type))
                {
                    error = $"Failed to find game component type with id {component.Id}";

                    Debug.LogError(error);

                    //DestroyImmediate(instance);

                    //return null;

                    continue;
                }
                
                var componentInstance = (GameComponent) instance.AddComponent(type);

                var componentAttribute = type.GetCustomAttribute<GameComponentAttribute>();
                
                if (componentAttribute == null) return null;

                var table = WorkspaceControl.Database[componentAttribute.Table];

                if (table != null)
                {
                    if (table.Seek((int) component.Entry[2].Value, out var row))
                    {
                        foreach (var field in type.GetFields())
                        {
                            var loadAttribute = field.GetCustomAttribute<LoadFieldAttribute>();

                            if (loadAttribute == null)
                            {
                                continue;
                            }

                            var fieldValue = row[loadAttribute.Name];
                            
                            if (fieldValue.Type == DataType.Nothing) continue;

                            field.SetValue(componentInstance, fieldValue.Value);
                        }
                    }
                }
                
                gameComponents.Add(componentInstance);
            }

            foreach (var gameComponent in gameComponents)
            {
                gameComponent.OnLoad();
            }

            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

            PrefabUtility.SaveAsPrefabAssetAndConnect(instance, prefabPath, InteractionMode.AutomatedAction);

            return instance;
        }

        public void Export()
        {
            
        }
    }
}
