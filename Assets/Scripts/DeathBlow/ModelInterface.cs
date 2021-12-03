using System;
using System.IO;
using DeathBlow.Components;
using InfectedRose.Nif;
using RakDotNet.IO;
using UnityEditor;
using UnityEngine;

namespace DeathBlow
{
    public class ModelInterface : EditorWindow
    {
        public string Notice { get; set; }

        public string Model { get; set; }

        public GameObject Instance { get; set; }

        public Color NoticeColor { get; set; }

        public bool SavePrefab { get; set; } = true;

        [MenuItem("Death Blow/Model Interface")]
        public static void Initialize()
        {
            var window = GetWindow<ModelInterface>("Model Interface");

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

            var selected = !string.IsNullOrWhiteSpace(Model);

            var display = !selected ? "Select..." : Model;

            if (GUILayout.Button(display))
            {
                Model = EditorUtility.OpenFilePanelWithFilters(
                                "Select Gamebyro model...",
                                selected ? Path.GetDirectoryName(Model) : ResourceUtilities.SearchRoot,
                                new[] {"Gamebyro (*.nif)", "nif"}
                );
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button($"Save prefab: {SavePrefab}"))
            {
                SavePrefab = !SavePrefab;
            }

            if (!selected)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Working GameObject");

            Instance = (GameObject) EditorGUILayout.ObjectField(
                            Instance,
                            typeof(GameObject),
                            true
            );

            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Actions");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Import"))
            {
                Import();
            }

            if (GUILayout.Button("Export"))
            {
                Export();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void Import()
        {
            Debug.Log("Importing...");

            if (!File.Exists(Model))
            {
                NoticeColor = Color.red;
                Notice = "Failed to find working file";

                return;
            }

            var stream = File.OpenRead(Model);

            var reader = new ByteReader(stream);

            var nif = new NiFile();

            nif.Deserialize(reader);

            try
            {
                nif.ReadBlocks(reader);
            }
            catch (Exception e)
            {
                NoticeColor = Color.red;
                Notice = $"Import error: [{e.GetType().Name}] {e.Message}";

                return;
            }

            ConstructModel(nif);

            NoticeColor = Color.green;
            Notice = "Successfully imported model";
        }

        public void ConstructModel(NiFile file)
        {
            Debug.Log("Working...");

            var constructor = new ModelConstructor();

            constructor.NifPath = Path.GetDirectoryName(Model);
            constructor.Name = Path.GetFileNameWithoutExtension(Model);
            constructor.File = file;
            constructor.CreatePrefab = SavePrefab;
            constructor.PrefabPath = Path.Combine(
                            WorkspaceControl.CurrentWorkspace.AssetModelsPath,
                            constructor.Name
            );

            Debug.Log(constructor.PrefabPath);

            try
            {
                Instance = constructor.Construct();
            }
            catch (Exception e)
            {
                NoticeColor = Color.red;
                Notice = e.Message;
                throw;
            }
        }

        private void Export()
        {

        }
    }
}
