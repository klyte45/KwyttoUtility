using Kwytto.Interfaces;
using Kwytto.Utils;
using System;

namespace Kwytto.Redirectors
{
    public class AssetEditorLoadSaveActionsRedirector : Redirector, IRedirectable
    {
        private static AssetEditorLoadSaveActionsRedirector instance;
        private event Action OnLoad;
        private event Action OnSave;
        public void Awake()
        {
            instance = this;
            System.Collections.Generic.List<Type> impls = ReflectionUtils.GetInterfaceImplementations(typeof(IAssetEditorActions));
            foreach (Type impl in impls)
            {
                var inst = impl.GetConstructor(new Type[0])?.Invoke(new object[0]) as IAssetEditorActions;
                OnLoad += inst.AfterLoad;
                OnSave += inst.AfterSave;
            }

            if (OnLoad != null)
            {
                AddRedirect(typeof(LoadAssetPanel).GetMethod("OnLoad", RedirectorUtils.allFlags), null, GetType().GetMethod("AfterLoadAsset", RedirectorUtils.allFlags));
            }
            if (OnSave != null)
            {
                AddRedirect(typeof(SaveAssetPanel).GetMethod("SaveRoutine", RedirectorUtils.allFlags), null, GetType().GetMethod("AfterSaveAsset", RedirectorUtils.allFlags));
            }
        }

        public static void AfterLoadAsset() => instance.OnLoad?.Invoke();
        public static void AfterSaveAsset() => instance.OnSave?.Invoke();
    }
}