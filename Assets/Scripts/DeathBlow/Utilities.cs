using System;
using System.IO;
using DeathBlow.Components;
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

        public static string HostPath(string path) => path == null ? "" : path.Replace("\\\\", "\\").Replace('\\', '/');

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