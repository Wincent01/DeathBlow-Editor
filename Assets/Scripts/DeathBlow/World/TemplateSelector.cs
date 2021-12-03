using UnityEngine;
using System.Collections;
using UnityEditor;
using DeathBlow.Components;
using System.Collections.Generic;
using InfectedRose.Database.Generic;
using DeathBlow;

public class TemplateSelector : EditorWindow
{
    public string Query { get; set; }

    private List<Result> Results { get; set; } = new List<Result>();

    public static SceneDetails Scene { get; set; }
    
    public int StartIndex { get; set; }
    
    public bool Spawner { get; set; }

    [MenuItem("Death Blow/Template Selector")]
    public static void Initialize()
    {
        var window = GetWindow<TemplateSelector>("Template Selector");

        window.Show();
    }

    internal struct Result
    {
        public int Lot;

        public string Name;
    }

    private void OnGUI()
    {
        if (!WorkspaceControl.Ok)
        {
            GUILayout.Label("Workspace not loaded.");

            return;
        }

        GUILayout.Label("Scene:");

        Scene = (SceneDetails)EditorGUILayout.ObjectField(
                        Scene,
                        typeof(SceneDetails),
                        true
        );

        if (Scene == null)
        {
            GUILayout.Label("No scene selected.");

            Scene = FindObjectOfType<SceneDetails>();

            return;
        }

        Spawner = GUILayout.Toggle(Spawner, "Initialize as spawner template");
        
        GUILayout.Space(5);

        GUILayout.Label("Search:");

        var query = GUILayout.TextField(Query);
        
        GUILayout.Space(5);
        
        GUILayout.BeginHorizontal();

        var startIndex = StartIndex;

        if (!(startIndex > 0))
        {
            EditorGUI.BeginDisabledGroup(true);
        }
        
        if (GUILayout.Button("<<"))
        {
            startIndex -= 25;

            if (startIndex <= 0) startIndex = 0;
        }
        
        if (!(startIndex > 0))
        {
            EditorGUI.EndDisabledGroup();
        }
        
        if (Results.Count != 25)
        {
            EditorGUI.BeginDisabledGroup(true);
        }
        
        if (GUILayout.Button(">>"))
        {
            startIndex += 25;
        }
        
        if (Results.Count != 25)
        {
            EditorGUI.EndDisabledGroup();
        }

        GUILayout.EndHorizontal();

        if (query != Query || startIndex != StartIndex)
        {
            if (query != Query)
            {
                StartIndex = 0;
            }
            
            Results.Clear();

            Query = query;

            StartIndex = startIndex;

            query = query.ToLower();

            var objects = WorkspaceControl.Database["Objects"];

            if (objects == null)
            {
                return;
            }

            var skip = StartIndex;
            
            foreach (var obj in objects)
            {
                var name = (string)obj[1].Value;

                if (name == null)
                {
                    continue;
                }

                if (name.ToLower().Contains(query) || obj.Key.ToString() == query)
                {
                    if (skip > 0)
                    {
                        --skip;
                        
                        continue;
                    }
                    
                    var result = new Result
                    {
                        Lot = obj.Key,
                        Name = name
                    };

                    Results.Add(result);

                    if (Results.Count >= 25)
                    {
                        break;
                    }
                }
            }
        }

        foreach (var result in Results)
        {
            GUILayout.Space(5);
            
            if (GUILayout.Button($"[{result.Lot}] {result.Name}"))
            {
                var details = ImportTemplate(result.Lot, $"[{result.Lot}] {result.Name}");

                if (Spawner)
                {
                    var spawner = ImportTemplate(176, $"[Spawner] {result.Name}");

                    spawner.SpawnerTemplate = details.gameObject;

                    details.transform.parent = spawner.transform;

                    details.IsSpawned = true;

                    Selection.activeGameObject = spawner.gameObject;
                }

                //Close();
            }
        }
    }

    public ObjectDetails ImportTemplate(int lot, string objectName)
    {
        var template = ObjectInterface.Import(lot, out var error);

        if (template == null)
        {
            Debug.LogError(error);
        }

        var zoneObject = new GameObject(objectName);

        var objectDetails = zoneObject.AddOrGetComponent<ObjectDetails>();

        objectDetails.Lot = lot;

        zoneObject.transform.parent = Scene.transform;

        template.transform.parent = zoneObject.transform;

        return objectDetails;
    }
}
