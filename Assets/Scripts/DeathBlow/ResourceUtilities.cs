using System;
using System.IO;
using DeathBlow.Components;
using UnityEngine;

namespace DeathBlow
{
    public static class ResourceUtilities
    {
        public static string SearchRoot => WorkspaceControl.CurrentWorkspace.WorkingRoot;

        public static byte[] ReadFrom(string source, string file)
        {
            file = file.ToLower();

            if (File.Exists(Path.Combine(source, file)))
            {
                return File.ReadAllBytes(Path.Combine(source, file));
            }
            
            foreach (var sample in Directory.GetFiles(SearchRoot, file, SearchOption.AllDirectories))
            {
                return File.ReadAllBytes(sample);
            }

            throw new FileNotFoundException($"Failed to search for file {file}");
        }
    
        public static Texture2D LoadTextureDxt(byte[] ddsBytes, TextureFormat textureFormat)
        {
            if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
                throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");

            var ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124)
                throw new Exception("Invalid DDS DXTn texture. Unable to read"); //this header byte should be 124 for DDS image files

            var height = ddsBytes[13] * 256 + ddsBytes[12];
            var width = ddsBytes[17] * 256 + ddsBytes[16];

            const int ddsHeaderSize = 128;
            var dxtBytes = new byte[ddsBytes.Length - ddsHeaderSize];
            Buffer.BlockCopy(ddsBytes, ddsHeaderSize, dxtBytes, 0, ddsBytes.Length - ddsHeaderSize);

            Texture2D texture;
        
            try
            {
                texture = new Texture2D(width, height, textureFormat, false);
                texture.LoadRawTextureData(dxtBytes);
                texture.Apply();
            }
            catch
            {
                texture = new Texture2D(width, height, textureFormat == TextureFormat.DXT1 ? TextureFormat.DXT5 : TextureFormat.DXT1, false);
                texture.LoadRawTextureData(dxtBytes);
                texture.Apply();
            }

            return texture;
        }
    }
}