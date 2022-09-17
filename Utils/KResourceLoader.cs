using Kwytto.Interfaces;
using Kwytto.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Kwytto.Utils
{
    public static class KResourceLoader
    {
        public static string Prefix { get; } = "Klyte";

        public static Assembly RefAssembly => BasicIUserMod.Instance.GetType().Assembly;

        public static byte[] LoadResourceData(string name)
        {
            name = $"{Prefix}.{name}";

            var stream = (UnmanagedMemoryStream)RefAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                LogUtils.DoLog("Could not find resource: " + name);
                return null;
            }

            var read = new BinaryReader(stream);
            return read.ReadBytes((int)stream.Length);
        }

        public static string LoadResourceString(string name)
        {
            name = $"{Prefix}.{name}";

            var stream = (UnmanagedMemoryStream)RefAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                LogUtils.DoLog("Could not find resource: " + name);
                return null;
            }

            var read = new StreamReader(stream);
            return read.ReadToEnd();
        }
        public static IEnumerable<string> LoadResourceStringLines(string name)
        {
            name = $"{Prefix}.{name}";

            using (var stream = (UnmanagedMemoryStream)RefAssembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                {
                    LogUtils.DoLog("Could not find resource: " + name);
                    yield break;
                }

                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        yield return line;
                    }
                }
            }
        }

        public static Texture2D LoadCommonsTexture(CommonsSpriteNames sprite)
        {
            return LoadTexture(GetCommonsTexturePath(sprite));
        }

        public static string GetCommonsTexturePath(CommonsSpriteNames sprite)
        {
            return $"_commons.UI.Images.{sprite}.png";
        }

        public static Texture2D LoadModTexture(string filename, string folder = "Images")
        {
            return LoadTexture($"UI.{folder}.{filename}.png");
        }
        public static Texture2D LoadTexture(string filename)
        {
            try
            {
                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                texture.LoadImage(LoadResourceData(filename));
                return texture;
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }

        public static AssetBundle LoadBundle(string filename)
        {
            try
            {
                return AssetBundle.LoadFromMemory(LoadResourceData(filename));
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }

    }
}
