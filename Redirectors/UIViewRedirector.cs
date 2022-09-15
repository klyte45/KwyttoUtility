using ColossalFramework.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System;

namespace Kwytto.Redirectors
{

    public class UIViewRedirector : Redirector, IRedirectable
    {
        public Redirector RedirectorInstance => this;
        public void Awake() => AddRedirect(typeof(UIView).GetMethod("Start", RedirectorUtils.allFlags), null, GetType().GetMethod("AfterStart", RedirectorUtils.allFlags));

        public static void AfterStart()
        {
            System.Collections.Generic.List<Type> impls = ReflectionUtils.GetInterfaceImplementations(typeof(IViewStartActions), typeof(UIViewRedirector));
            foreach (Type impl in impls)
            {
                var inst = impl.GetConstructor(new Type[0])?.Invoke(new object[0]) as IViewStartActions;
                inst?.OnViewStart();
            }
        }
    }
}