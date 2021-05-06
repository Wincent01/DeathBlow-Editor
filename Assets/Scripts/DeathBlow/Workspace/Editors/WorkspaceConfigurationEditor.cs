#if UNITY_EDITOR

using System.IO;
using DeathBlow.Components;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Workspace.Editors
{
    [CustomEditor(typeof(WorkspaceConfiguration))]
    public class WorkspaceConfigurationEditor : Editor
    {
        public const int SpaceSize = 10;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var readonlyObject = (WorkspaceConfiguration) serializedObject.targetObject;

            var workspaceNameProperty = serializedObject.FindProperty("_workspaceName");

            var title = new GUIStyle(EditorStyles.largeLabel);
            title.fontSize = 30;
            title.fontStyle = FontStyle.Bold;
            
            GUILayout.Label($"Workspace: {workspaceNameProperty.stringValue}", title);

            EditorGUILayout.PropertyField(workspaceNameProperty);

            GUILayout.Space(SpaceSize);
            GUILayout.Label("Resources (/res):", EditorStyles.boldLabel);

            var workingRootProperty = serializedObject.FindProperty("_workingRoot");
            var databaseProperty = serializedObject.FindProperty("_database");
            var workingRoot = workingRootProperty.stringValue;

            var newWorkingRoot = workingRoot;
            if (GUILayout.Button(workingRoot))
            {
                newWorkingRoot = EditorUtility.OpenFolderPanel(
                                "Select Resource root...",
                                !string.IsNullOrWhiteSpace(workingRoot) ? Path.GetDirectoryName(workingRoot) : "",
                                "res"
                );
            }
            
            var database = Path.Combine(newWorkingRoot, "cdclient.fdb");

            if (newWorkingRoot != workingRoot)
            {
                databaseProperty.stringValue = File.Exists(database) ? database : null;
                
                workingRootProperty.stringValue = newWorkingRoot;
            }

            var okStyle = new GUIStyle(EditorStyles.label) {normal = {textColor = Color.green}};
            var errorStyle = new GUIStyle(EditorStyles.label) {normal = {textColor = Color.red}};
            
            if (string.IsNullOrWhiteSpace(databaseProperty.stringValue))
            {
                GUILayout.Label("Warning: Failed to find database!", errorStyle);
            }
            else
            {
                var relative = Utilities.GetRelativePath(databaseProperty.stringValue, workingRootProperty.stringValue);
                GUILayout.Label($"Database: {relative}", okStyle);
            }
            
            GUILayout.Space(SpaceSize);
            GUILayout.Label("Directory setup:", EditorStyles.boldLabel);
            
            var assetPathProperty = serializedObject.FindProperty("_assetPath");
            var assetModelsPathProperty = serializedObject.FindProperty("_assetModelsPath");
            var assetObjectsPathProperty = serializedObject.FindProperty("_assetObjectsPath");
            var assetMaterialsPathProperty = serializedObject.FindProperty("_assetMaterialsPath");
            var assetTexturesPathProperty = serializedObject.FindProperty("_assetTexturesPath");

            EditorGUILayout.PropertyField(assetPathProperty);
            EditorGUILayout.PropertyField(assetModelsPathProperty);
            EditorGUILayout.PropertyField(assetObjectsPathProperty);
            EditorGUILayout.PropertyField(assetMaterialsPathProperty);
            EditorGUILayout.PropertyField(assetTexturesPathProperty);
            
            GUILayout.Space(SpaceSize);
            GUILayout.Label("Model configuration:", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_vertexMaterial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_normalMaterial"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_terrainMaterial"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif