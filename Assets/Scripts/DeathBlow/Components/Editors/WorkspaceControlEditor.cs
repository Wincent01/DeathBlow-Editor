using DeathBlow.Workspace;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Editors
{
    [CustomEditor(typeof(WorkspaceControl))]
    public class WorkspaceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            GUILayout.Label("Deathblow Control Asset");
            
            GUILayout.Label($"Database loaded: {WorkspaceControl.Database != null}");

            var property = serializedObject.FindProperty("_currentWorkspace");

            var objectReference = property.objectReferenceValue;
            
            EditorGUILayout.PropertyField(property);

            serializedObject.ApplyModifiedProperties();

            if (objectReference != property.objectReferenceValue || GUILayout.Button("Reload workspace") || property.objectReferenceValue != null && WorkspaceControl.Database == null && !WorkspaceControl.LoadingDatabase)
            {
                WorkspaceControl.UpdateWorkspace((WorkspaceConfiguration) property.objectReferenceValue);
            }
            
            if (WorkspaceControl.Ok)
            {
                if (WorkspaceControl.SavingDatabase)
                {
                    GUILayout.Label("Saving database...");
                }
                else if (GUILayout.Button("Save workspace"))
                {
                    WorkspaceControl.SaveWorkspace(WorkspaceControl.CurrentWorkspace);
                }
            }
        }
    }
}