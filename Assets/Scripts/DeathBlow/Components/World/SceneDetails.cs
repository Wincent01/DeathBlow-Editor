using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SceneDetails))]
public class SceneDetailsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("New Object"))
        {
            TemplateSelector.Initialize();

            TemplateSelector.Scene = ((SceneDetails) serializedObject.targetObject).gameObject;
        }

        base.OnInspectorGUI();
    }
}


public class SceneDetails : MonoBehaviour
{
    [SerializeField] private string _sceneName;

    [SerializeField] private uint _sceneLayer;

    [SerializeField] private string _skyBox;

    [SerializeField] private Transform _light;

    [SerializeField] private float _luminosity;

    [SerializeField] private float _fogIntensity = 0;
    
    [SerializeField] private float _fogStart = 1500;

    [SerializeField] private float _fogEnd = 1750;

    public string SceneName { get => _sceneName; set => _sceneName = value; }
    
    public uint SceneLayer { get => _sceneLayer; set => _sceneLayer = value; }

    public string SkyBox { get => _skyBox; set => _skyBox = value; }
    
    public Transform Light { get => _light; set => _light = value; }
    
    public float Luminosity { get => _luminosity; set => _luminosity = value; }
    
    public float FogIntensity { get => _fogIntensity; set => _fogIntensity = value; }
    
    public float FogStart { get => _fogStart; set => _fogStart = value; }
    
    public float FogEnd { get => _fogEnd; set => _fogEnd = value; }
}
