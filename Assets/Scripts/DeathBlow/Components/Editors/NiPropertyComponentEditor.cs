#if UNITY_EDITOR

using System;
using System.Linq;
using System.Reflection;
using InfectedRose.Nif;
using UnityEditor;
using UnityEngine;
using Vector3 = InfectedRose.Nif.Vector3;
using Vector4 = UnityEngine.Vector4;

namespace DeathBlow.Components.Editors
{
    [CustomEditor(typeof(NiPropertyComponent))]
    public class NiPropertyComponentEditor : Editor
    {
        public static Type[] IntegerTypes { get; } =
        {
            typeof(uint),
            typeof(int),
            typeof(ushort),
            typeof(short),
            typeof(byte),
            typeof(sbyte)
        };
        
        public static Type[] LongTypes { get; } =
        {
            typeof(ulong),
            typeof(long)
        };
        
        public static Type[] FloatTypes { get; } =
        {
            typeof(float)
        };
        
        public static Type[] DoubleTypes { get; } =
        {
            typeof(double)
        };
        
        public override void OnInspectorGUI()
        {
            var component = (NiPropertyComponent) serializedObject.targetObject;

            EditorGUILayout.LabelField($"{component.type}");
            
            if (!string.IsNullOrWhiteSpace(component.Notice))
            {
                var style = new GUIStyle(EditorStyles.textField);
                style.normal.textColor = Color.green;

                if (GUILayout.Button($"{component.Notice}!", style))
                {
                    component.Notice = "";
                }
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Edit");

            var newEditing = EditorGUILayout.Toggle(component.Editing);
            
            EditorGUILayout.EndHorizontal();

            if (newEditing != component.Editing)
            {
                if (newEditing)
                {
                    component.Temporary = component.GetProperty();

                    component.Notice = "Loaded details";
                }
                else
                {
                    component.SetProperty(component.Temporary);
                    
                    component.Notice = "Saved details";
                }
            }

            component.Editing = newEditing;
            
            if (!component.Editing)
            {
                serializedObject.Update();
                
                return;
            }

            var property = component.Temporary;

            if (property == null)
            {
                return;
            }
            
            var type = property.GetType();
            
            var properties = type.GetProperties();

            foreach (var info in properties)
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField($"{info.Name} [{info.PropertyType.Name}]");

                InspectProperty(info, property);
                
                EditorGUILayout.EndHorizontal();
            }
            
            serializedObject.Update();
        }

        public static void InspectProperty(PropertyInfo info, object value)
        {
            var type = info.PropertyType;

            if (IntegerTypes.Contains(type))
            {
                var v = Convert.ToInt32(info.GetValue(value));
                
                var n = EditorGUILayout.IntField(v);

                if (type == typeof(uint))
                {
                    info.SetValue(value,  (uint) n);
                }
                
                if (type == typeof(ushort))
                {
                    info.SetValue(value,  (ushort) n);
                }
                
                if (type == typeof(short))
                {
                    info.SetValue(value,  (short) n);
                }
                
                if (type == typeof(byte))
                {
                    info.SetValue(value,  (byte) n);
                }
                
                if (type == typeof(int))
                {
                    info.SetValue(value,  n);
                }
                
                return;
            }

            if (LongTypes.Contains(type))
            {
                info.SetValue(value, EditorGUILayout.LongField((long) info.GetValue(value)));
                
                return;
            }

            if (FloatTypes.Contains(type))
            {
                info.SetValue(value, EditorGUILayout.FloatField((float) info.GetValue(value)));
                
                return;
            }

            if (DoubleTypes.Contains(type))
            {
                info.SetValue(value, EditorGUILayout.DoubleField((double) info.GetValue(value)));
                
                return;
            }

            if (type == typeof(bool))
            {
                info.SetValue(value, EditorGUILayout.Toggle((bool) info.GetValue(value)));
                
                return;
            }
            
            if (type == typeof(NiString))
            {
                var niString = (NiString) info.GetValue(value);

                var index = niString.Index;
                
                EditorGUILayout.LabelField($"Index: {(index == uint.MaxValue ? "Null" : index.ToString())}");
                
                return;
            }

            if (type.Name.StartsWith("Ptr"))
            {
                var ptr = info.GetValue(value);

                var indexInfo = ptr.GetType().GetField("Index");

                if (indexInfo == null)
                {
                    EditorGUILayout.LabelField($"Failed to load ptr");
                    
                    return;
                }

                var index = (int) indexInfo.GetValue(ptr);
                
                EditorGUILayout.LabelField($"Index: {(index == -1 ? "Null" : index.ToString())}");
                
                return;
            }
            
            if (type == typeof(Vector3))
            {
                var v = (Vector3) info.GetValue(value);
                v.x = EditorGUILayout.FloatField(v.x);
                v.y = EditorGUILayout.FloatField(v.y);
                v.z = EditorGUILayout.FloatField(v.z);
                info.SetValue(value, v);
                
                return;
            }
            
            if (type == typeof(Color3))
            {
                var v = (Color3) info.GetValue(value);
                v.r = EditorGUILayout.FloatField(v.r);
                v.g = EditorGUILayout.FloatField(v.g);
                v.b = EditorGUILayout.FloatField(v.b);
                info.SetValue(value, v);
                
                return;
            }
            
            if (type == typeof(Vector4))
            {
                var v = (Vector4) info.GetValue(value);
                v.x = EditorGUILayout.FloatField(v.x);
                v.y = EditorGUILayout.FloatField(v.y);
                v.z = EditorGUILayout.FloatField(v.z);
                v.w = EditorGUILayout.FloatField(v.w);
                info.SetValue(value, v);
                
                return;
            }
            
            EditorGUILayout.LabelField("Unsupported type!");
        }
    }
}
#endif