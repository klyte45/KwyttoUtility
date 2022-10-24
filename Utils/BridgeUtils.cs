using Kwytto.Interfaces;
using System;
using System.Linq;

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
    }
}
