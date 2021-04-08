using UnityEngine;
using System.Collections;
using InfectedRose.Lvl;
using System.Collections.Generic;
using System;
using UnityEditor;
using DeathBlow.Components.Game;
using System.Linq;

public enum ObjectDataType
{
    UTF16 = 0,
    Int32 = 1,
    Float32 = 3,
    Float64 = 4,
    UInt32 = 5,
    Boolean = 7,
    UInt64 = 8,
    ObjectID = 9,
    UTF8 = 13
}


[CustomEditor(typeof(ObjectDetails))]
public class ObjectDetailsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var lotProperty = serializedObject.FindProperty("_lot");

        var objectDetails = (ObjectDetails)serializedObject.targetObject;

        var template = objectDetails.GetComponentInChildren<GameTemplate>();

        if (template == null)
        {
            GUILayout.Label("No template child object found.");
        }

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Template", template, template.GetType());
        EditorGUI.EndDisabledGroup();

        if (objectDetails.IsSpawned)
        {
            GUILayout.Space(3);
            GUILayout.Label("This object is a spawn template!");
            GUILayout.Space(3);
        }

        GUILayout.Label("Components");

        foreach (var gameComponent in template.GetComponentsInChildren<GameComponent>())
        {
            GUILayout.Space(3);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(gameComponent, gameComponent.GetType());
            EditorGUI.EndDisabledGroup();

            gameComponent.OnDetailGUI(objectDetails);

            if (gameComponent.GetType() == typeof(SpawnerComponent))
            {
                var spawnedObjectProperty = serializedObject.FindProperty("_spawnerTemplate");

                EditorGUILayout.PropertyField(spawnedObjectProperty);

                if (objectDetails.SpawnerTemplate != null)
                {
                    if (objectDetails.SpawnerTemplate.transform.parent != objectDetails.transform)
                    {
                        objectDetails.transform.position = objectDetails.SpawnerTemplate.transform.position;
                        objectDetails.SpawnerTemplate.transform.parent = objectDetails.transform;
                    }

                    objectDetails.SpawnerTemplate.GetComponent<ObjectDetails>().IsSpawned = true;
                }
            }
        }

        GUILayout.Space(5);

        GUILayout.Label("Data entries:");

        var dataProperty = serializedObject.FindProperty("_data");

        EditorGUILayout.PropertyField(dataProperty);

        serializedObject.ApplyModifiedProperties();

        //base.OnInspectorGUI();
    }
}

[Serializable]
public class ObjectDataEntry
{
    public string Key;

    public ObjectDataType Type;

    public string Value;

    public bool Custom;
}

public class ObjectDetails : MonoBehaviour
{
    [SerializeField] private List<ObjectDataEntry> _data = new List<ObjectDataEntry>();

    [HideInInspector] [SerializeField] private int _lot;

    [HideInInspector] [SerializeField] private GameObject _spawnerTemplate;

    [HideInInspector] [SerializeField] private bool _isSpawned;

    public List<ObjectDataEntry> Data { get => _data; set => _data = value; }

    public int Lot { get => _lot; set => _lot = value; }

    public GameObject SpawnerTemplate { get => _spawnerTemplate; set => _spawnerTemplate = value; }

    public bool IsSpawned { get => _isSpawned; set => _isSpawned = value; }

    public ObjectDataEntry GetEntry(string key) => _data.FirstOrDefault(e => e.Key == key);

    public ObjectDataEntry CreateEntry(string key, ObjectDataType type, string value, bool custom = false)
    {
        var entry = GetEntry(key);

        if (entry != null)
        {
            entry.Type = type;
            entry.Value = value;
            entry.Custom = custom;

            return entry;
        }

        entry = new ObjectDataEntry();

        entry.Key = key;
        entry.Type = type;
        entry.Value = value;
        entry.Custom = custom;

        _data.Add(entry);

        return entry;
    }

    public void DeleteEntry(string key) => _data.Remove(GetEntry(key));

    public LegoDataDictionary GetDataDictionary()
    {
        var dictionary = new LegoDataDictionary();

        foreach (var entry in _data)
        {
            var value = entry.Value;
            object dataValue;

            switch (entry.Type)
            {
                case ObjectDataType.UTF16:
                    dataValue = value;
                    break;
                case ObjectDataType.Int32:
                    dataValue = int.Parse(value);
                    break;
                case ObjectDataType.Float32:
                    dataValue = float.Parse(value);
                    break;
                case ObjectDataType.Float64:
                    dataValue = double.Parse(value);
                    break;
                case ObjectDataType.UInt32:
                    dataValue = uint.Parse(value);
                    break;
                case ObjectDataType.Boolean:
                    dataValue = bool.Parse(value);
                    break;
                case ObjectDataType.UInt64:
                    dataValue = ulong.Parse(value);
                    break;
                case ObjectDataType.ObjectID:
                    dataValue = long.Parse(value);
                    break;
                case ObjectDataType.UTF8:
                    dataValue = value.Select(c => (byte)c).ToArray();
                    break;
                default:
                    dataValue = value;
                    break;
            }

            dictionary.Add(entry.Key, dataValue, (byte) entry.Type);
        }

        return dictionary;
    }
}
