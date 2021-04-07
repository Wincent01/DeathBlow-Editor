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

    public static GameObject Scene { get; set; }


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

        Scene = (GameObject)EditorGUILayout.ObjectField(
                        Scene,
                        typeof(GameObject),
                        true
        );

        if (Scene == null)
        {
            GUILayout.Label("No scene selected.");

            return;
        }

        GUILayout.Label("Search:");

        var query = GUILayout.TextField(Query);

        if (query != Query)
        {
            Results.Clear();

            Query = query;

            query = query.ToLower();

            var objects = WorkspaceControl.Database["Objects"];

            if (objects == null)
            {
                return;
            }

            foreach (var obj in objects)
            {
                var name = (string)obj[1].Value;

                if (name == null)
                {
                    continue;
                }

                if (name.ToLower().Contains(query) || obj.Key.ToString() == query)
                {
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
            GUILayout.Space(3);
            
            if (GUILayout.Button($"[{result.Lot}] {result.Name}"))
            {
                var template = ObjectInterface.Import(result.Lot, out var error);

                if (template == null)
                {
                    Debug.LogError(error);

                    continue;
                }

                var zoneObject = new GameObject($"[{result.Lot}] {result.Name}");

                var objectDetails = zoneObject.AddOrGetComponent<ObjectDetails>();

                objectDetails.Lot = result.Lot;

                zoneObject.transform.parent = Scene.transform;

                template.transform.parent = zoneObject.transform;

                Close();
            }
        }
    }
}
