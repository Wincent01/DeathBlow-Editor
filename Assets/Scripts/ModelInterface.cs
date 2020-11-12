#if UNITY_EDITOR
using System;
using System.IO;
using InfectedRose.Nif;
using RakDotNet.IO;
using UnityEditor;
using UnityEngine;

public class ModelInterface : EditorWindow
{
    public string Notice { get; set; }
    
    public string Model { get; set; }
    
    public GameObject Instance { get; set; }
    
    public Color NoticeColor { get; set; }
    
    public Material Material { get; set; }
    
    [MenuItem("Death Blow/Model Interface")]
    public static void Initialize()
    {
        var window = GetWindow<ModelInterface>("Model Interface");
        
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

        GUILayout.Label("Working file");

        var selected = !string.IsNullOrWhiteSpace(Model);
        
        var display = !selected ? "Select..." : Model;
        
        if (GUILayout.Button(display))
        {
            Model = EditorUtility.OpenFilePanelWithFilters(
                "Select Gamebyro model...",
                selected ? Path.GetDirectoryName(Model) : "",
                new[] {"Gamebyro (*.nif)", "nif"}
            );
        }

        if (!selected)
        {
            return;
        }

        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Label("Material");
        
        Material = (Material) EditorGUILayout.ObjectField(
            Material,
            typeof(Material),
            true
        );
        
        EditorGUILayout.EndHorizontal();
        
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
            NoticeColor = Color.red;
            Notice = "Failed to find working file";
            
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
        catch (Exception e)
        {
            NoticeColor = Color.red;
            Notice = e.Message;
            
            return;
        }

        ConstructModel(nif);
        
        NoticeColor = Color.green;
        Notice = "Successfully imported model";
    }

    public void ConstructModel(NiFile file)
    {
        var constructor = new ModelConstructor();

        constructor.Path = Path.GetDirectoryName(Model);
        constructor.Name = Path.GetFileNameWithoutExtension(Model);
        constructor.File = file;
        constructor.Material = Material;
        
        constructor.Construct();
    }
    
    private void Export()
    {
        
    }
}

#endif