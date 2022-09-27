using Kwytto.Interfaces;
using System;
using System.Linq;

namespace Kwytto.Utils
{
    public static class BridgeUtils
    {
        public static T GetMostPrioritaryImplementation<T>() where T : class, IBridgePrioritizable
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(T).IsAssignableFrom(p)).Select(x =>
            {
                try
                {
                    return x.GetConstructor(new Type[0]).Invoke(new object[0]) as T;
                }
                catch
                {
                    return null;
                }
            }).Where(x => x != null).OrderBy(x => x.Priority).FirstOrDefault();
        }
    }
}
