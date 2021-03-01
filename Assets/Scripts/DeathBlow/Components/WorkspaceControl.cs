using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DeathBlow.Components.Game;
using DeathBlow.Workspace;
using InfectedRose.Database;
using InfectedRose.Database.Concepts;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components
{
    [ExecuteAlways]
    public class WorkspaceControl : MonoBehaviour
    {
        [SerializeField]
        private WorkspaceConfiguration _currentWorkspace;

        private static WorkspaceControl Singleton { get; set; }
        
        public static bool LoadingDatabase { get; set; }
        
        public static bool SavingDatabase { get; set; }
        
        public static AccessDatabase Database { get; set; }
        
        public static List<string> SqlOutput { get; set; } = new List<string>();
        
        public static Dictionary<ComponentId, Type> ComponentRegistry { get; set; }

        public static bool Ok
        {
            get
            {
                if (Singleton == null)
                {
                    return false;
                }
                
                if (Database == null && CurrentWorkspace != null && !LoadingDatabase)
                {
                    UpdateWorkspace(CurrentWorkspace);
                }
                
                return Database != null && CurrentWorkspace != null;
            }
        }

        public static WorkspaceConfiguration CurrentWorkspace
        {
            get => Singleton._currentWorkspace;
            set => Singleton._currentWorkspace = value;
        }

        [MenuItem("Death Blow/Workspace Controller")]
        public static void CreateController()
        {
            var instance = new GameObject("Workspace Controller");

            instance.AddComponent<WorkspaceControl>();
            
            instance.transform.SetAsFirstSibling();

            Selection.activeGameObject = instance;
        }
        
        public void Update()
        {
            if (Singleton != null && Singleton != this)
            {
                Debug.LogError("Only one Workspace asset is allowed!");
            }

            Singleton = this;
            
            if (Database == null && CurrentWorkspace != null && !LoadingDatabase)
            {
                UpdateWorkspace(CurrentWorkspace);
            }
        }

        public static void SaveWorkspace(WorkspaceConfiguration configuration)
        {
            if (!Ok || SavingDatabase) return;

            SavingDatabase = true;
            
            Task.Run(async () =>
            {
                Debug.Log("Saving database...");

                try
                {
                    await Database.SaveAsync(CurrentWorkspace.Database + ".new");
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    
                    Debug.Log("Failed to save database...");
                        
                    SavingDatabase = false;
                    
                    throw;
                }

                Debug.Log("Finished saving database...");
                
                Debug.Log($"Saving sql... [{SqlOutput.Count}]");
                
                File.WriteAllLines(CurrentWorkspace.Database + ".sql", SqlOutput.ToArray());
                
                Debug.Log("Finished saving sql...");
                
                SavingDatabase = false;
            });
        }
        
        public static void UpdateWorkspace(WorkspaceConfiguration configuration)
        {
            if (configuration == null)
            {
                Debug.LogError("A Workspace always has to be selected!");
            }

            CurrentWorkspace = configuration;

            if (CurrentWorkspace == null)
            {
                return;
            }

            if (LoadingDatabase) return;
            
            LoadingDatabase = true;

            Database = null;

            if (string.IsNullOrEmpty(configuration.AssetPath))
            {
                var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(configuration));
                
                var assets = AssetDatabase.CreateFolder(path, "Assets");
                
                CurrentWorkspace.AssetPath = AssetDatabase.GUIDToAssetPath(assets);
                
                var assetsModels = AssetDatabase.CreateFolder(CurrentWorkspace.AssetPath, "Models");
                
                CurrentWorkspace.AssetModelsPath = AssetDatabase.GUIDToAssetPath(assetsModels);
                
                var assetsObjects = AssetDatabase.CreateFolder(CurrentWorkspace.AssetPath, "Objects");

                CurrentWorkspace.AssetObjectsPath = AssetDatabase.GUIDToAssetPath(assetsObjects);
            }

            Task.Run(async () =>
            {
                Debug.Log("Loading database...");

                try
                {
                    Database = await AccessDatabase.OpenAsync(CurrentWorkspace.Database);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                        
                    Debug.Log("Failed to load database...");
                        
                    LoadingDatabase = false;
                        
                    throw;
                }

                ComponentRegistry = new Dictionary<ComponentId, Type>();

                foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    var attribute = type.GetCustomAttribute<GameComponentAttribute>();
                    
                    if (attribute == null) continue;

                    ComponentRegistry[attribute.ComponentId] = type;
                }

                Debug.Log("Finished loading database...");
                    
                LoadingDatabase = false;
                
                SqlOutput.Clear();

                Database.OnSql += SqlOutput.Add;
            });
        }
    }
}