using ColossalFramework.Plugins;
using HarmonyLib;
using Kwytto.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kwytto.Utils
{
    public static class BridgeUtils
    {
        public static T[] GetAllLoadableClassesInAppDomain<T>() where T : class
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.SelectMany(s =>
             {
                 try
                 {
                     return s.GetExportedTypes();
                 }
                 catch
                 {
                     return new Type[0];
                 }
             }).Where(p =>
             {
                 try
                 {
                     return typeof(T).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract;
                 }
                 catch { return false; }
             }).Select(x =>
             {
                 try
                 {
                     return x.GetConstructor(new Type[0]).Invoke(new object[0]) as T;
                 }
                 catch
                 {
                     return null;
                 }
             }).Where(x => x != null).ToArray();
        }

        public static T GetMostPrioritaryImplementation<T>() where T : class, IBridgePrioritizable
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var classes = assemblies.SelectMany(s =>
            {
                try
                {
                    return s.GetExportedTypes();
                }
                catch
                {
                    return new Type[0];
                }
            }).Where(p =>
            {
                try
                {
                    return typeof(T).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract;
                }
                catch
                {
                    return false;
                }
            }).ToArray();
            if (BasicIUserMod.DebugMode)
            {
                LogUtils.DoLog($"Found Classes of {typeof(T)} ({classes?.Count()}):\n\t{string.Join("\n\t", classes.Select(x => x.FullName).ToArray())}");
            }
            return classes.Select(x =>
            {
                try
                {
                    return x.GetConstructor(new Type[0]).Invoke(new object[0]) as T;
                }
                catch (Exception e)
                {
                    if (BasicIUserMod.DebugMode)
                        LogUtils.DoLog($"Class failed to be loaded as integration: {x}\n{e}");
                    return null;
                }
            }).Where(x => x?.IsBridgeEnabled ?? false).OrderBy(x => x.Priority).First();
        }

        public static void WarmupBridge(string targetModDll, string targetModName, List<Tuple<Type, string>> targetReversePatchTypes)
        {
            var targetAssembly = PluginManager.instance.GetPluginsInfo().FirstOrDefault(x => x.assemblyCount > 0 && x.isEnabled && x.GetAssemblies().Any(y => y.GetName().Name == targetModDll))
             ?? throw new Exception($"The {targetModName} bridge isn't available due to the mod not being active. Using fallback!");
            var vsAsset = targetAssembly.GetAssemblies().First(y => y.GetName().Name == targetModDll);

            var exportedTypes = vsAsset.GetExportedTypes();
            foreach (var tuple in targetReversePatchTypes)
            {
                var type = tuple.First;
                var sourceClassName = tuple.Second;
                var targetType = exportedTypes.First(x => x.Name == sourceClassName);
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    var srcMethod = targetType.GetMethod(method.Name, RedirectorUtils.allFlags, null, method.GetParameters().Select(x => x.ParameterType).ToArray(), null);
                    if (srcMethod != null) Harmony.ReversePatch(srcMethod, new HarmonyMethod(method));
                    else LogUtils.DoWarnLog($"Method not found while patching {targetModName}: {targetType.FullName} {srcMethod.Name}({string.Join(", ", method.GetParameters().Select(x => $"{x.ParameterType}").ToArray())})");
                }
            }
        }
    }
}
