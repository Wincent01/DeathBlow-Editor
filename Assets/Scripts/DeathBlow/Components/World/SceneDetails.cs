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

    public string SceneName { get => _sceneName; set => _sceneName = value; }
    
    public uint SceneLayer { get => _sceneLayer; set => _sceneLayer = value; }

    public string SkyBox { get => _skyBox; set => _skyBox = value; }
}
