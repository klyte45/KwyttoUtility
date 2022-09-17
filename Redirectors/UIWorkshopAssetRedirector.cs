using ColossalFramework.Packaging;
using ColossalFramework.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.IO;
using System.Linq;

namespace Kwytto.Redirectors
{
    public class UIWorkshopAssetRedirector : Redirector, IRedirectable
    {
        public Redirector RedirectorInstance => this;
        public void Awake()
        {
            if (BasicIUserMod.Instance.AssetExtraFileNames?.Length > 0 || BasicIUserMod.Instance.AssetExtraDirectoryNames?.Length > 0)
            {
                AddRedirect(typeof(WorkshopAssetUploadPanel).GetMethod("PrepareStagingArea", RedirectorUtils.allFlags), null, GetType().GetMethod("AfterPrepareStagingArea", RedirectorUtils.allFlags));
            }
        }

        public static void AfterPrepareStagingArea(WorkshopAssetUploadPanel __instance)
        {
            var m_ContentPath = __instance.GetType().GetField("m_ContentPath", RedirectorUtils.allFlags).GetValue(__instance) as string;
            var m_TargetAsset = __instance.GetType().GetField("m_TargetAsset", RedirectorUtils.allFlags).GetValue(__instance) as Package.Asset;
            var rootAssetFolder = Path.GetDirectoryName(m_TargetAsset.package.packagePath);
            LogUtils.DoErrorLog($"rootAssetFolder2: {rootAssetFolder}; ");
            bool bundledAnyFile = false;
            if (!(BasicIUserMod.Instance.AssetExtraFileNames is null))
            {
                foreach (string filename in BasicIUserMod.Instance.AssetExtraFileNames)
                {
                    var targetFilename = Path.Combine(rootAssetFolder, filename);
                    if (File.Exists(targetFilename))
                    {
                        File.Copy(targetFilename, Path.Combine(m_ContentPath, filename));
                        bundledAnyFile = true;
                    }
                }
            }
            if (!(BasicIUserMod.Instance.AssetExtraDirectoryNames is null))
            {
                foreach (string directory in BasicIUserMod.Instance.AssetExtraDirectoryNames)
                {
                    var targetFolder = Path.Combine(rootAssetFolder, directory);
                    if (Directory.Exists(targetFolder))
                    {
                        WorkshopHelper.DirectoryCopy(targetFolder, Path.Combine(m_ContentPath, directory), true);
                        bundledAnyFile = true;
                    }
                }
            }

            if (bundledAnyFile)
            {
                var tagsField = (__instance.GetType().GetField("m_Tags", RedirectorUtils.allFlags));
                tagsField.SetValue(__instance, (tagsField.GetValue(__instance) as string[]).Concat(new string[] { BasicIUserMod.Instance.SimpleName, $"K45 {BasicIUserMod.Instance.Acronym}" }).Distinct().ToArray());
            }

        }
    }
}