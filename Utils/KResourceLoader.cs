﻿using Kwytto.Interfaces;
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
        private static string NamespaceMod => $"{RefAssemblyMod.GetName().Name}.";
        public static Assembly RefAssemblyKwytto => typeof(KResourceLoader).Assembly;

        public static byte[] LoadResourceDataMod(string name) => LoadResourceData(NamespaceMod + name, RefAssemblyMod);
        public static byte[] LoadResourceDataKwytto(string name) => LoadResourceData("Kwytto." + name, RefAssemblyKwytto);
        private static byte[] LoadResourceData(string name, Assembly refAssembly)
        {
            var stream = (UnmanagedMemoryStream)refAssembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                if (BasicIUserMod.DebugMode) LogUtils.DoLog($"Could not find resource: {name} @ {refAssembly}");
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
                if (BasicIUserMod.DebugMode) LogUtils.DoLog($"Could not find resource: {name} @ {refAssembly}");
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
                    if (BasicIUserMod.DebugMode) LogUtils.DoLog($"Could not find resource: {name} @ {refAssembly}");
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
                var texture = TextureUtils.New(1, 1);
                texture.LoadImage(LoadResourceData(filename, refAssembly));
                return texture;
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }

        public static AssetBundle LoadBundle(string filename, Assembly refAssembly = null)
        {
            refAssembly = refAssembly ?? RefAssemblyMod;
            try
            {
                return AssetBundle.LoadFromMemory(LoadResourceData(refAssembly.GetName().Name + "." + filename, refAssembly));
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog("The file could not be read:" + e.Message);
            }

            return null;
        }

    }
}
