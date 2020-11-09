#if UNITY_EDITOR
using System;
using System.IO;
using InfectedRose.Nif;
using RakDotNet.IO;
using UnityEditor;
using UnityEngine;

public class ModelInterface : EditorWindow
{
    public string Error { get; set; }
    
    public string Model { get; set; }
    
    public GameObject Instance { get; set; }
    
    [MenuItem("Death Blow/Model Interface")]
    public static void Initialize()
    {
        var window = GetWindow<ModelInterface>();
        
        window.Show();
    }

    private void OnGUI()
    {
        if (!string.IsNullOrWhiteSpace(Error))
        {
            var style = new GUIStyle(EditorStyles.textField);
            style.normal.textColor = Color.red;

            if (GUILayout.Button($"{Error}!", style))
            {
                Error = "";
            }
        }

        GUILayout.Label("Working file");

        var selected = !string.IsNullOrWhiteSpace(Model);
        
        var display = !selected ? "Select..." : Model;
        
        if (GUILayout.Button(display))
        {
            Model = EditorUtility.OpenFilePanelWithFilters(
                "Select Gamebyro model...",
                selected ? Path.GetDirectoryName(Model) : "",
                new[] {"Gamebyro", "nif"}
            );
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
        if (!File.Exists(Model))
        {
            Error = "Failed to find working file";
            
            return;
        }

        var stream = File.OpenRead(Model);

        var reader = new BitReader(stream);
        
        var nif = new NiFile();
        
        nif.Deserialize(reader);

        try
        {
            nif.ReadBlocks(reader);
        }
        catch (NotImplementedException e)
        {
            Error = e.Message;
            
            return;
        }
    }
    
    private void Export()
    {
        
    }
}

#endif