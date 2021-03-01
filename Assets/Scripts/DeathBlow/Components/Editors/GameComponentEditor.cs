using System;
using System.IO;
using DeathBlow.Components.Game;
using UnityEditor;
using UnityEngine;

namespace DeathBlow.Components.Editors
{
    [CustomEditor(typeof(GameComponent))]
    public class GameComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Reload"))
            {
                var gameComponent = (GameComponent) serializedObject.targetObject;
                
                gameComponent.OnLoad();
            }
        }

        public void AssetProperty(string fieldName, string propertyName, string relativeTo = "")
        {
            GUILayout.Space(5);
            
            GUILayout.Label(fieldName);

            var property = serializedObject.FindProperty(propertyName);

            var value = property.stringValue;

            var assetName = value;

            var selected = !string.IsNullOrWhiteSpace(assetName);
            
            assetName = assetName.Replace('\\', '/');

            if (GUILayout.Button(assetName))
            {
                var source = Path.GetDirectoryName(Path.Combine(ResourceUtilities.SearchRoot, relativeTo, assetName));
                Debug.Log(source);
                assetName = EditorUtility.OpenFilePanelWithFilters(
                                "Select asset...",
                                selected ? source : ResourceUtilities.SearchRoot,
                                new string[0]
                );
                
                if (!string.IsNullOrWhiteSpace(assetName))
                {
                    assetName = Utilities.GetRelativeAssetPath(assetName, relativeTo);

                    assetName = Utilities.HostPath(assetName);

                    property.stringValue = assetName;

                    serializedObject.ApplyModifiedProperties();
                }
            }
            
            GUILayout.Space(5);
        }
    }
}