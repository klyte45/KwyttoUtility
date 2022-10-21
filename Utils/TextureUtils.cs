﻿using System;
using System.IO;
using UnityEngine;

namespace Kwytto.Utils
{
    public static class TextureUtils
    {
        public static Texture2D New(int width, int height, TextureFormat format = TextureFormat.RGBA32, bool linear = true) => new Texture2D(width, height, format, false, linear);

        public static Texture2D DeCompress(this Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Default);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height, source.format, false, true)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point
            };

            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public static void DumpTo(this Texture2D texture, string filename)
        {
            byte[] bytes;
            try
            {
                bytes = GetBytes(texture);
                bytes.GetLength(0);
            }
            catch (Exception ex)
            {
                try
                {
                    var img = texture.MakeReadable();
                    bytes = GetBytes(img);
                }
                catch (Exception ex2)
                {
                    LogUtils.DoErrorLog("There was an error while dumping the texture - " + ex.Message + " => " + ex2.Message);
                    return;
                }
            }

            File.WriteAllBytes(filename, bytes);
            LogUtils.DoWarnLog($"Texture dumped to \"{filename}\"");
        }

        private static byte[] GetBytes(Texture2D img)
        {
            byte[] bytes = img.EncodeToPNG();
            if (bytes is null)
            {
                bytes = img.DeCompress().EncodeToPNG();
            }

            return bytes;
        }
        public static string ToBase64(this Texture2D src)
        {
            byte[] imageData = src.EncodeToPNG();
            return Convert.ToBase64String(imageData);
        }

        public static Texture2D Base64ToTexture2D(string encodedData, bool linear = true)
        {
            byte[] imageData = Convert.FromBase64String(encodedData);

            GetImageSize(imageData, out int width, out int height);

            Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false, linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point
            };
            texture.LoadImage(imageData);
            return texture;
        }
        private static void GetImageSize(byte[] imageData, out int width, out int height)
        {
            width = ReadInt(imageData, 3 + 15);
            height = ReadInt(imageData, 3 + 15 + 2 + 2);
        }
        private static int ReadInt(byte[] imageData, int offset)
        {
            return (imageData[offset] << 8) | imageData[offset + 1];
        }
    }
}
