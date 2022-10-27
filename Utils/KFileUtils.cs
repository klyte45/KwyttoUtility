using ColossalFramework.IO;
using ColossalFramework.Packaging;
using ColossalFramework.UI;
using Kwytto.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ColossalFramework.Packaging.Package;

namespace Kwytto.Utils
{
    public class KFileUtils
    {
        #region File & Prefab Utils
        public static readonly string BASE_FOLDER_PATH = DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Klyte45Mods" + Path.DirectorySeparatorChar;

        public static FileInfo EnsureFolderCreation(string folderName)
        {
            if (File.Exists(folderName) && (File.GetAttributes(folderName) & FileAttributes.Directory) != FileAttributes.Directory)
            {
                File.Delete(folderName);
            }
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            return new FileInfo(folderName);
        }
        public static bool IsFileCreated(string fileName) => File.Exists(fileName);
        public static bool TryGetWorkshopId(PrefabInfo info, out long workshopId)
        {
            workshopId = -1L;
            return info != null && info.name != null && info.name.Contains(".") && long.TryParse(info.name.Split(new char[]
            {
                '.'
            })[0], out workshopId);
        }

        public static void ScanPrefabsFolders<T, I>(PrefabIndexesAbstract<T, I> index, string filenameToSearch, Action<FileStream, T> action) where T : PrefabInfo where I : PrefabIndexesAbstract<T, I>
            => ScanPrefabsFolders(index, new Dictionary<string, Action<FileStream, T>>
            {
                [filenameToSearch] = action
            });

        public static void ScanPrefabsFolders<T, I>(PrefabIndexesAbstract<T, I> index, Dictionary<string, Action<FileStream, T>> actions) where T : PrefabInfo where I : PrefabIndexesAbstract<T, I>
        {
            var list = new List<string>();
            index.PrefabsData.ForEach((loaded) =>
            {
                Package.Asset asset = PackageManager.FindAssetByName(loaded.Key);
                if (!(asset == null) && !(asset.package == null))
                {
                    string packagePath = asset.package.packagePath;
                    if (packagePath != null)
                    {
                        foreach (string filenameToSearch in actions.Keys)
                        {
                            string filePath = Path.Combine(Path.GetDirectoryName(packagePath), filenameToSearch);
                            if (!list.Contains(filePath))
                            {
                                list.Add(filePath);
                                if (File.Exists(filePath))
                                {
                                    using (FileStream stream = File.OpenRead(filePath))
                                    {
                                        actions[filenameToSearch](stream, loaded.Value.Info as T);
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        public static readonly string BASE_STAGING_FOLDER_PATH = Path.Combine(DataLocation.assetsPath, "K45_Staging");
        public static string GetModStagingBasePath() => Path.Combine(BASE_STAGING_FOLDER_PATH, BasicIUserMod.Instance.SafeName);
        public static string GetModStagingBasePathForAsset(Asset asset) => Path.Combine(GetModStagingBasePath(), asset.package.packageName);
        public static string GetRootPackageFolderForK45(Asset asset) => asset?.package is null ? null : asset.isWorkshopAsset ? Path.GetDirectoryName(asset.package.packagePath) : GetModStagingBasePathForAsset(asset);
        public static string GetRootFolderForK45(Asset asset) => asset is null ? null : asset.isWorkshopAsset ? Path.GetDirectoryName(asset.package.packagePath) : GetModStagingBasePathForAsset(asset);
        public static string GetRootFolderForK45<T>(T info) where T : PrefabInfo => GetRootFolderForK45(PrefabUtils.GetAssetFromPrefab(info));

        public static void DoInPrefabFolder(PrefabInfo targetPrefab, Action<string> actionToPerform) => DoInPrefabFolder(targetPrefab.name, actionToPerform);
        public static void DoInPrefabFolder(string prefabName, Action<string> actionToPerform)
        {
            Package.Asset asset = PackageManager.FindAssetByName(prefabName);
            if (!(asset == null) && !(asset.package == null))
            {
                string packagePath = asset.package.packagePath;
                if (packagePath != null)
                {
                    actionToPerform(Path.GetDirectoryName(packagePath));
                }
            }
        }
        public static void ScanPrefabsFoldersDirectory<T, I>(PrefabIndexesAbstract<T, I> index, string directoryToFind, Action<ulong, string, T> action) where T : PrefabInfo where I : PrefabIndexesAbstract<T, I>
        {
            var list = new List<string>();
            index.PrefabsData.ForEach((loaded) =>
            {
                Package.Asset asset = PackageManager.FindAssetByName(loaded.Key);
                if (!(asset == null) && !(asset.package == null))
                {
                    string packagePath = asset.package.packagePath;
                    if (packagePath != null)
                    {
                        string filePath = Path.Combine(Path.GetDirectoryName(packagePath), directoryToFind);
                        if (!list.Contains(filePath))
                        {
                            list.Add(filePath);
                            LogUtils.DoLog("DIRECTORY TO FIND: " + filePath);
                            if (Directory.Exists(filePath))
                            {
                                action(asset.package.GetPublishedFileID().AsUInt64, filePath, loaded.Value.Info as T);
                            }
                        }
                    }
                }
            });
        }

        public static void ScanPrefabsFoldersDirectoryNoLoad(string directoryToFind, Action<string, Package, Asset> action)
        {
            var list = new List<string>();
            ForEachNonLoadedPrefab((package, asset) =>
            {
                string packagePath = asset.package.packagePath;
                if (packagePath != null)
                {
                    string filePath = Path.Combine(Path.GetDirectoryName(packagePath), directoryToFind);
                    if (!list.Contains(filePath))
                    {
                        list.Add(filePath);
                        if (Directory.Exists(filePath))
                        {
                            action(filePath, package, asset);
                        }
                    }
                }
            });
        }
        public static void ScanPrefabsFoldersFileNoLoad(string file, Action<FileStream, Package, Asset> action)
        {
            var list = new List<string>();
            ForEachNonLoadedPrefab((package, asset) =>
            {
                string packagePath = asset.package.packagePath;
                if (packagePath != null)
                {
                    string filePath = Path.Combine(Path.GetDirectoryName(packagePath), file);
                    if (!list.Contains(filePath))
                    {
                        list.Add(filePath);
                        if (File.Exists(filePath))
                        {
                            using (FileStream stream = File.OpenRead(filePath))
                            {
                                action(stream, package, asset);
                            }
                        }
                    }
                }
            });
        }
        public static void ForEachNonLoadedPrefab(Action<Package, Asset> action)
        {
            foreach (Package pack in PackageManager.allPackages)
            {
                IEnumerable<Asset> assets = pack.FilterAssets((AssetType)103);
                if (assets.Count() == 0)
                {
                    continue;
                }

                action(pack, assets.First());
            }
        }
        public static string[] GetAllFilesEmbeddedAtFolder(string packageDirectory, string extension)
        {

            var executingAssembly = KResourceLoader.RefAssemblyMod;
            string folderName = $"Klyte.{packageDirectory}";
            return executingAssembly
                .GetManifestResourceNames()
                .Where(r => r.StartsWith(folderName) && r.EndsWith(extension))
                .Select(r => r.Substring(folderName.Length + 1))
                .ToArray();
        }
        #endregion
    }
}
