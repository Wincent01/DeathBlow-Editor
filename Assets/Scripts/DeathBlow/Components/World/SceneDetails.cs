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

    [SerializeField] private string[] _skyBox =
    {
                    "",
                    "(invalid)",
                    "(invalid)",
                    "(invalid)",
                    "(invalid)",
                    "(invalid)"
    };

    [SerializeField] private Transform _light;

    [SerializeField] private float _luminosity;

    [SerializeField] private float _fogIntensity = 0;

    [SerializeField] public Vector3 _unknownVector0;

    [SerializeField] public Vector3 _unknownVector1;

    [SerializeField] public Transform _unknownRotation0;

    [SerializeField] public Transform _unknownRotation1;

    [Header("Fog 0")]
    [SerializeField] private float _fogStart = 1500;

    [SerializeField] private float _fogEnd = 1750;

    [SerializeField] public float _unknownFogSetting0;

    [SerializeField] public float _unknownFogSetting1;

    [SerializeField] public float _unknownFogSetting2;

    [SerializeField] public float _unknownFogSetting3;

    [Header("Fog 1")]
    [SerializeField] public float _fogStart1 = 1500;

    [SerializeField] public float _fogEnd1 = 1750;

    [SerializeField] public float _unknownFogSetting0_1;

    [SerializeField] public float _unknownFogSetting1_1;

    [SerializeField] public float _unknownFogSetting2_1;

    [SerializeField] public float _unknownFogSetting3_1;

    [Header("Debug")]
    [SerializeField] public string _sourceLevelFile;

    public string SceneName { get => _sceneName; set => _sceneName = value; }
    
    public uint SceneLayer { get => _sceneLayer; set => _sceneLayer = value; }

    public string[] SkyBox { get => _skyBox; set => _skyBox = value; }
    
    public Transform Light { get => _light; set => _light = value; }
    
    public float Luminosity { get => _luminosity; set => _luminosity = value; }
    
    public float FogIntensity { get => _fogIntensity; set => _fogIntensity = value; }
    
    public float FogStart { get => _fogStart; set => _fogStart = value; }
    
    public float FogEnd { get => _fogEnd; set => _fogEnd = value; }
}
