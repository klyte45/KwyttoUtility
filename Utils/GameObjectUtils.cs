using System;
using UnityEngine;

namespace Kwytto.Utils
{
    public static class GameObjectUtils
    {

        public static T CreateElement<T>(Transform parent, string name = null) where T : MonoBehaviour
        {
            CreateElement<T>(out T uiItem, parent, name);
            return uiItem;
        }
        public static void CreateElement<T>(out T uiItem, Transform parent, string name = null) where T : MonoBehaviour
        {
            var container = new GameObject();
            container.transform.parent = parent;
            uiItem = (T)container.AddComponent(typeof(T));
            if (name != null)
            {
                container.name = name;
            }
        }
        public static GameObject CreateElement(Type type, Transform parent)
        {
            var container = new GameObject();
            container.transform.parent = parent;
            container.AddComponent(type);
            return container;
        }
    }
}
