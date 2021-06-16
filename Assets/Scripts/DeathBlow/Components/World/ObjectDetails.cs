using UnityEngine;
using System.Collections;
using InfectedRose.Lvl;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEditor;
using DeathBlow.Components.Game;
using System.Linq;
using DeathBlow;
using DeathBlow.Components.Editors;
using InfectedRose.Core;
using Object = UnityEngine.Object;

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

        var scriptComponent = template.GetComponent<ScriptComponent>();

        if (scriptComponent == null)
        {
            GUILayout.Space(3);
            
            var entry = objectDetails.GetEntry("custom_script_server");

            var value = entry == null ? "" : entry.Value;

            value = Utilities.AssetProperty("Custom Script Server", value, settings: AssetPropertySettings.Edit);

            value = Utilities.ClientPath(value);

            if (entry == null && !string.IsNullOrWhiteSpace(value))
            {
                objectDetails.SetEntry("custom_script_server", ObjectDataType.UTF16, value, true);
            }
            else if (entry != null)
            {
                entry.Value = value;
            }
        }

        GUILayout.Space(5);

        GUILayout.Label("Data entries:");

        if (GUILayout.Button("Log data"))
        {
            Debug.Log(objectDetails.GetDataDictionary().ToString());
        }

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

[Serializable]
public class UnityObjectReferenceEntry
{
    public string Key;

    public Object Reference;
}

public class ObjectDetails : MonoBehaviour
{
    [SerializeField] private List<ObjectDataEntry> _data = new List<ObjectDataEntry>();

    [HideInInspector] [SerializeField] private int _lot;

    [HideInInspector] [SerializeField] private GameObject _spawnerTemplate;

    [HideInInspector] [SerializeField] private bool _isSpawned;

    [HideInInspector] [SerializeField] private List<UnityObjectReferenceEntry> _unityObjectReferences;

    public List<ObjectDataEntry> Data { get => _data; set => _data = value; }

    public int Lot { get => _lot; set => _lot = value; }

    public GameObject SpawnerTemplate { get => _spawnerTemplate; set => _spawnerTemplate = value; }

    public bool IsSpawned { get => _isSpawned; set => _isSpawned = value; }

    public List<UnityObjectReferenceEntry> UnityObjectReferences
    {
        get => _unityObjectReferences;
        set => _unityObjectReferences = value;
    }
    
    public Object GetReference(string key) => _unityObjectReferences.FirstOrDefault(e => e.Key == key)?.Reference;

    public void SetReference(string key, Object reference)
    {
        var entry = _unityObjectReferences.FirstOrDefault(e => e.Key == key);

        if (entry != null)
        {
            entry.Reference = reference;
            
            return;
        }

        _unityObjectReferences.Add(new UnityObjectReferenceEntry {Key = key, Reference = reference});
    }

    public void OnDrawGizmos()
    {
        foreach (var gameComponent in GetComponentsInChildren<GameComponent>())
        {
            gameComponent.OnDetailGizmos(this);
        }
    }

    public void DeleteReference(string key) => _unityObjectReferences.Remove(
                    _unityObjectReferences.FirstOrDefault(e => e.Key == key)
    );

    public ObjectDataEntry GetEntry(string key) => _data.FirstOrDefault(e => e.Key == key);

    public ObjectDataEntry SetEntry(string key, ObjectDataType type, string value, bool custom = false)
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

    public void SimpleDataSelector(ObjectDataType type, string key, string title = null, bool custom = false)
    {
        var entry = GetEntry(key);
        
        var value = entry == null ? "" : entry.Value;

        switch (type)
        {
            case ObjectDataType.UTF8:
            case ObjectDataType.UTF16:
                value = EditorGUILayout.TextField(title ?? key, value);
                break;
            case ObjectDataType.UInt32:
            case ObjectDataType.Int32:
                value = EditorGUILayout.IntField(title ?? key, int.TryParse(value, out var i) ? i : 0).ToString();
                break;
            case ObjectDataType.Float32:
                value = EditorGUILayout.FloatField(title ?? key, float.TryParse(value, out var f) ? f : 0).ToString(CultureInfo.InvariantCulture);
                break;
            case ObjectDataType.Float64:
                value = EditorGUILayout.DoubleField(title ?? key, double.Parse(value)).ToString(CultureInfo.InvariantCulture);
                break;
            case ObjectDataType.Boolean:
                value = EditorGUILayout.Toggle(title ?? key, value == "1") ? "1" : "0";
                break;
            case ObjectDataType.UInt64:
                value = EditorGUILayout.LongField(title ?? key, long.Parse(value)).ToString();
                break;
            case ObjectDataType.ObjectID:
                value = EditorGUILayout.LongField(title ?? key, long.Parse(value)).ToString();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        
        if (entry == null && !string.IsNullOrWhiteSpace(value))
        {
            SetEntry(key, type, value, custom);
        }
        else if (entry != null)
        {
            entry.Value = value;
        }
    }

    public T ReferenceSelector<T>(string key, string title = null) where T : Object
    {
        var reference = GetReference(key);

        if (!(reference is T))
        {
            reference = null;
        }

        var newReference = (T) EditorGUILayout.ObjectField(title ?? key, reference, typeof(T), true);
        
        if (reference != newReference) SetReference(key, newReference);

        return newReference;
    }
    
    public LegoDataDictionary GetDataDictionary()
    {
        var dictionary = new LegoDataDictionary();

        var entries = _data.ToList();

        if (_spawnerTemplate != null)
        {
            var details = _spawnerTemplate.GetComponent<ObjectDetails>();
            
            if (details != null)
            {
                foreach (var entry in details._data)
                {
                    entries.Add(entry);
                }
            }
        }

        foreach (var entry in entries)
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
                    dataValue = value;
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
