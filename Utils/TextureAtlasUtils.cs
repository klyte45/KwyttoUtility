﻿using ColossalFramework.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static ColossalFramework.UI.UITextureAtlas;

namespace Kwytto.Utils
{

    public static class TextureAtlasUtils
    {
        public static string BORDER_FILENAME = "bordersDescriptor.txt";
        public static UITextureAtlas DefaultTextureAtlas => UIView.GetAView().defaultAtlas;

        public static void LoadPathTexturesIntoInGameTextureAtlas(string path, ref List<SpriteInfo> newFiles) => LoadPathTexturesIntoTextureAtlas(UIView.GetAView().defaultAtlas, path, ref newFiles);
        public static void LoadPathTexturesIntoTextureAtlas(UITextureAtlas textureAtlas, string path, ref List<SpriteInfo> newFiles)
        {
            if (textureAtlas == null)
            {
                return;
            }

            string[] files = Directory.GetFiles(path, "*.png");
            if (files.Length == 0)
            {
                return;
            }
            var borderDescriptors = new Dictionary<string, Tuple<RectOffset, bool>>();
            if (File.Exists($"{path}{Path.DirectorySeparatorChar}{BORDER_FILENAME}"))
            {
                ParseBorderDescriptors(File.ReadAllLines($"{path}{Path.DirectorySeparatorChar}{BORDER_FILENAME}"), out borderDescriptors);
            }
            foreach (string filename in files)
            {
                if (File.Exists(filename))
                {
                    byte[] fileData = File.ReadAllBytes(filename);
                    var tex = TextureUtils.New(2, 2);
                    if (tex.LoadImage(fileData))
                    {
                        newFiles.AddRange(CreateSpriteInfo(borderDescriptors, filename, tex));
                    }
                }
            }
        }
        public static Texture2D LoadTextureFromFile(string filename, TextureFormat format = TextureFormat.RGBA32, bool linear = true)
        {
            byte[] fileData = File.ReadAllBytes(filename);
            var tex = TextureUtils.New(2, 2, format, linear);
            return tex.LoadImage(fileData) ? tex : null;
        }
        public static List<SpriteInfo> CreateSpriteInfo(Dictionary<string, Tuple<RectOffset, bool>> borderDescriptors, string filename, Texture2D tex)
        {
            string textureName = Path.GetFileNameWithoutExtension(filename);
            string generatedSpriteName;
            if (textureName.StartsWith("%"))
            {
                generatedSpriteName = textureName.Substring(1);
            }
            else
            {
                generatedSpriteName = textureName;
            }
            borderDescriptors.TryGetValue(generatedSpriteName, out Tuple<RectOffset, bool> border);
            var res = new SpriteInfo
            {
                texture = tex,
                name = generatedSpriteName,
                border = border?.First ?? new RectOffset()
            };
            if (border?.Second ?? false)
            {
                return new List<SpriteInfo>() {
                    res,
                    new SpriteInfo
                        {
                            texture = tex,
                            name = generatedSpriteName +NoBorderSuffix,
                            border =new RectOffset()
                        }
                    };
            }
            else
            {
                return new List<SpriteInfo>() { res };
            }
        }

        public static readonly string NoBorderSuffix = "_NOBORDER";



        public static void ParseBorderDescriptors(IEnumerable<string> lines, out Dictionary<string, Tuple<RectOffset, bool>> borderDescriptors)
        {
            borderDescriptors = new Dictionary<string, Tuple<RectOffset, bool>>();
            foreach (string line in lines)
            {
                string[] lineSpilt = line.Split('=');
                if (lineSpilt.Length >= 2)
                {
                    string[] lineValues = lineSpilt[1].Split(',');
                    if (lineValues.Length == 4
                        && int.TryParse(lineValues[0], out int left)
                        && int.TryParse(lineValues[1], out int right)
                        && int.TryParse(lineValues[2], out int top)
                        && int.TryParse(lineValues[3], out int bottom)
                        )
                    {
                        borderDescriptors[lineSpilt[0]] = Tuple.New(new RectOffset(left, right, top, bottom), lineSpilt.Length >= 3 && bool.TryParse(lineSpilt[2], out bool noBorder) && noBorder);
                    }
                }
            }
        }

        public static void RegenerateDefaultTextureAtlas(List<SpriteInfo> newFiles) => RegenerateTextureAtlas(UIView.GetAView().defaultAtlas, newFiles);
        public static void RegenerateTextureAtlas(UITextureAtlas textureAtlas, List<SpriteInfo> newFiles)
        {
            IEnumerable<string> newSpritesNames = newFiles.Select(x => x.name);
            newFiles.AddRange(textureAtlas.sprites.Where(x => !newSpritesNames.Contains(x.name)));
            textureAtlas.sprites.Clear();
            textureAtlas.AddSprites(newFiles.ToArray());
            if (textureAtlas.texture == null)
            {
                textureAtlas.material.mainTexture = TextureUtils.New(1, 1);
                (textureAtlas.material.mainTexture as Texture2D).SetPixel(0, 0, default);
            }
            Rect[] array = textureAtlas.texture.PackTextures(textureAtlas.sprites.Select(x => x.texture).ToArray(), textureAtlas.padding, 4096 * 4);

            for (int i = 0; i < textureAtlas.count; i++)
            {
                textureAtlas.sprites[i].region = array[i];
            }
            textureAtlas.sprites.Sort();
            textureAtlas.RebuildIndexes();
            UIView.RefreshAll(false);
        }
    }
}
