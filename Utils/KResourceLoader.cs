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
        public static Assembly RefAssemblyMod => BasicIUserMod.Instance.GetType().Assembly;
        private static string NamespaceMod => $"{BasicIUserMod.Instance.SafeName}.";
        public static Assembly RefAssemblyKwytto => typeof(KResourceLoader).Assembly;

        public static byte[] LoadResourceDataMod(string name) => LoadResourceData(NamespaceMod + name, RefAssemblyMod);
        public static byte[] LoadResourceDataKwytto(string name) => LoadResourceData("Kwytto." + name, RefAssemblyKwytto);
        private static byte[] LoadResourceData(string name, Assembly refAssembly)
        {
            var stream = (UnmanagedMemoryStream)refAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                LogUtils.DoLog("Could not find resource: " + name);
                return null;
            }

            var read = new BinaryReader(stream);
            return read.ReadBytes((int)stream.Length);
        }

        public static string LoadResourceStringMod(string name) => LoadResourceString(NamespaceMod + name, RefAssemblyMod);
        public static string LoadResourceStringKwytto(string name) => LoadResourceString("Kwytto." + name, RefAssemblyKwytto);
        private static string LoadResourceString(string name, Assembly refAssembly)
        {
            var stream = (UnmanagedMemoryStream)refAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                LogUtils.DoLog("Could not find resource: " + name);
                return null;
            }

            var read = new StreamReader(stream);
            return read.ReadToEnd();
        }
        public static IEnumerable<string> LoadResourceStringLinesMod(string name) => LoadResourceStringLines(NamespaceMod + name, RefAssemblyMod);
        public static IEnumerable<string> LoadResourceStringLinesKwytto(string name) => LoadResourceStringLines("Kwytto." + name, RefAssemblyKwytto);
        private static IEnumerable<string> LoadResourceStringLines(string name, Assembly refAssembly)
        {
            using (var stream = (UnmanagedMemoryStream)refAssembly.GetManifestResourceStream(name))
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

        public static Texture2D LoadTextureKwytto(CommonsSpriteNames sprite)
        {
            return LoadTexture(GetCommonsTexturePath(sprite), RefAssemblyKwytto);
        }

        public static string GetCommonsTexturePath(CommonsSpriteNames sprite)
        {
            return $"Kwytto.UI.Images.{sprite}.png";
        }

        public static Texture2D LoadTextureMod(string filename, string folder = "Images")
        {
            return LoadTexture(NamespaceMod + $"UI.{folder}.{filename}.png", RefAssemblyMod);
        }
        private static Texture2D LoadTexture(string filename, Assembly refAssembly)
        {
            try
            {
                var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false, true)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Point
                };
                texture.LoadImage(LoadResourceData(filename, refAssembly));
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
                return AssetBundle.LoadFromMemory(LoadResourceData(NamespaceMod + filename, RefAssemblyMod));
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }

    }
}
