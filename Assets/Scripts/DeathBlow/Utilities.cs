using System;
using System.Collections.Generic;
using System.IO;
using DeathBlow.Components;
using DeathBlow.Components.Editors;
using UnityEditor;
using UnityEngine;

namespace DeathBlow
{
    public static class Utilities
    {
        public static string GetRelativePath(string file, string folder)
        {
            var pathUri = new Uri(file);
            
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            
            var folderUri = new Uri(folder);
            
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
        
        public static string GetRelativeAssetPath(string file, string assetFolder)
        {
            var folder = Path.Combine(ResourceUtilities.SearchRoot, assetFolder);
            
            var pathUri = new Uri(file);
            
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            
            var folderUri = new Uri(folder);
            
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public static FileStream OpenAssetStreamRead(string file, string relativeTo = "")
        {
            file = file.Replace("\\", "/");
            
            var path = Path.Combine(ResourceUtilities.SearchRoot, relativeTo, file);
            
            Debug.Log(path);

            if (!File.Exists(path)) return null;

            return File.OpenRead(path);
        }
        
        public static FileStream OpenAssetStreamWrite(string file, string relativeTo = "")
        {
            file = file.Replace("\\", "/");
            
            var path = Path.Combine(ResourceUtilities.SearchRoot, relativeTo, file);

            if (!File.Exists(path)) return null;

            return File.OpenWrite(path);
        }

        public static void OpenWithDefaultProgram(string path)
        {
            path = path.Replace('/', Path.DirectorySeparatorChar);
            path = path.Replace('\\', Path.DirectorySeparatorChar);
            System.Diagnostics.Process fileopener = new System.Diagnostics.Process();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();

            Debug.Log(fileopener.StartInfo.FileName + " " + fileopener.StartInfo.Arguments);
        }

        public static string HostPath(string path) => path == null ? "" : path.Replace("\\\\", "\\").Replace('\\', '/');

        public static string ClientPath(string path) => path == null ? "" : path.Replace("/", "\\");

        public static string AssetProperty(string fieldName, string value, string relativeTo = "", AssetPropertySettings settings = AssetPropertySettings.Default)
        {
            GUILayout.Space(5);

            GUILayout.Label(fieldName);

            var assetName = value;

            var selected = !string.IsNullOrWhiteSpace(assetName);

            assetName = assetName.Replace('\\', '/');

            var normalizedFile = assetName.ToLower();

            var fullPath = Path.Combine(ResourceUtilities.SearchRoot, relativeTo, assetName);
            var normalizedFullPath = Path.Combine(ResourceUtilities.SearchRoot, relativeTo, normalizedFile);

            if (GUILayout.Button(assetName))
            {
                var source = Path.GetDirectoryName(fullPath);
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

                    value = assetName;
                }
            }

            if (File.Exists(normalizedFullPath) && (settings & AssetPropertySettings.Edit) == AssetPropertySettings.Edit)
            {
                GUILayout.Space(1);

                if (GUILayout.Button("Edit with default editor"))
                {
                    Utilities.OpenWithDefaultProgram(normalizedFullPath);
                }
            }

            GUILayout.Space(5);

            return value;
        }

        public static System.Numerics.Vector3 ToNative(Vector3 value) => new System.Numerics.Vector3(value.x, value.y, value.z);

        public static System.Numerics.Quaternion ToNative(Quaternion value) => new System.Numerics.Quaternion(value.x, value.y, value.z, value.w);

        public static Vector3 ToUnity(System.Numerics.Vector3 value) => new Vector3(value.X, value.Y, value.Z);

        public static Quaternion ToUnity(System.Numerics.Quaternion value) => new Quaternion(value.X, value.Y, value.Z, value.W);
        
        public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
        {
            Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

            var mid = Vector3.Lerp(start, end, t);

            return new Vector3(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
        }

        public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
        {
            Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

            var mid = Vector2.Lerp(start, end, t);

            return new Vector2(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t));
        }
        
        public static Vector3 ToGameSpace(Vector3 value)
        {
            var gameSpace = value;
            
            gameSpace.z *= -1;

            return gameSpace;
        }
        
        public static void GizmosDrawString(string text, Vector3 worldPos, Color? colour = null)
        {
            Handles.BeginGUI();

            var restoreColor = GUI.color;

            if (colour.HasValue) GUI.color = colour.Value;
            var view = SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

            if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
            {
                GUI.color = restoreColor;
                Handles.EndGUI();
                return;
            }

            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
            GUI.color = restoreColor;
            Handles.EndGUI();
        }

        public static Dictionary<TKey, TValue> CloneDictionary<TKey, TValue> (Dictionary<TKey, TValue> original)
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count,
                                                                    original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue)entry.Value);
            }

            return ret;
        }

        /*
        public static string GetWorkspaceAssetPath(string assetFolder, string relativeTo)
        {
            var source = Path.Combine(Application.dataPath, "../", ResourceUtilities.SearchRoot);

            var relative = Path.Combine(source, relativeTo);

            if (!Directory.Exists(relativeTo))
            {
                Directory.CreateDirectory(relativeTo);
            }

            var folder = Path.Combine(relative, assetFolder);
            
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            
            return GetRelativePath();
        }
        */
    }
}