﻿using ColossalFramework;
using ColossalFramework.Threading;
using ICities;
using Kwytto.Interfaces;
using Kwytto.LiteUI;
using Kwytto.Localization;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Kwytto.Data
{
    public sealed class DataContainer : SingletonLite<DataContainer>, ISerializableDataExtension
    {
        public static event Action OnDataLoaded;

        public Dictionary<Type, IDataExtension> Instances { get; private set; } = new Dictionary<Type, IDataExtension>();

        #region Serialization
        public IManagers Managers => SerializableDataManager?.managers;

        public ISerializableData SerializableDataManager { get; private set; }

        public void OnCreated(ISerializableData serializableData) => SerializableDataManager = serializableData;
        public void OnLoadData()
        {
            if (BasicIUserMod.DebugMode) LogUtils.DoLog($"LOADING DATA {GetType()}");
            instance.Instances = new Dictionary<Type, IDataExtension>();
            List<Type> instancesExt = ReflectionUtils.GetInterfaceImplementations(typeof(IDataExtension));
            if (BasicIUserMod.DebugMode) LogUtils.DoLog($"SUBTYPE COUNT: {instancesExt.Count};");
            foreach (Type type in instancesExt)
            {
                if (BasicIUserMod.DebugMode) LogUtils.DoLog($"LOADING DATA TYPE {type}");
                if (type.IsGenericType)
                {
                    try
                    {
                        IEnumerable<Type> allTypes;
                        try
                        {
                            allTypes = type.Assembly.GetTypes();
                        }
                        catch (ReflectionTypeLoadException r)
                        {
                            allTypes = r.Types.Where(k => !(k is null));
                        }
                        var targetParameters = allTypes.Where(x => !x.IsAbstract && !x.IsInterface && !x.IsGenericType && ReflectionUtils.CanMakeGenericTypeVia(type.GetGenericArguments()[0], x)).ToArray();
                        if (BasicIUserMod.DebugMode) LogUtils.DoLog($"PARAMETER PARAMS FOR {type.GetGenericArguments()[0]} FOUND: [{string.Join(",", targetParameters.Select(x => x.ToString()).ToArray())}]");
                        foreach (var param in targetParameters)
                        {
                            var targetType = type.MakeGenericType(param);
                            ProcessExtension(targetType);
                        }
                    }
                    catch (Exception e)
                    {
                        LogUtils.DoErrorLog($"FAILED CREATING GENERIC PARAM EXTENSOR: {e}");
                    }
                }
                else
                {
                    ProcessExtension(type);
                }
            }

            ThreadHelper.dispatcher.Dispatch(() =>
            {
                OnDataLoaded?.Invoke();
                OnDataLoaded = null;
            });
        }

        private void ProcessExtension(Type type)
        {
            var basicInstance = (IDataExtension)Activator.CreateInstance(type);
            if (!SerializableDataManager.EnumerateData().Contains(basicInstance.SaveId))
            {
                if (BasicIUserMod.DebugMode) LogUtils.DoLog($"NO DATA TYPE {type} - Instancing basic instance");
                instance.Instances[type] = basicInstance.LoadDefaults(SerializableDataManager) ?? basicInstance;
                return;
            }
            byte[] storage = SerializableDataManager.LoadData(basicInstance.SaveId);
            try
            {
                instance.Instances[type] = basicInstance.Deserialize(storage) ?? basicInstance;
                if (BasicIUserMod.DebugMode)
                {
                    string content = System.Text.Encoding.UTF8.GetString(storage);
                    if (BasicIUserMod.DebugMode) LogUtils.DoLog($"{type} DATA {storage.Length}b => {content}");
                }
            }
            catch (Exception e)
            {
                byte[] targetArr;
                bool zipped = false;
                try
                {
                    targetArr = ZipUtils.UnzipBytes(storage);
                    zipped = true;
                }
                catch
                {
                    targetArr = storage;
                }
                string content = System.Text.Encoding.UTF8.GetString(targetArr);
                LogUtils.DoErrorLog($"{type} CORRUPTED DATA! => \nException: {e.Message}\n{e.StackTrace}\nData {storage.Length} Z={zipped} b:\n{content}");
                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                {
                    title = string.Format(KStr.comm_errLoadingDataWindow_Title, type),
                    message = $"{string.Format(KStr.comm_errLoadingDataWindow_Header, BasicIUserMod.Instance.SimpleName)}\n{(BasicIUserMod.Instance.GitHubRepoPath.IsNullOrWhiteSpace() ? "" : "\n" + KStr.comm_errLoadingDataWindow_HeaderReportABug)}\n{KStr.comm_errLoadingDataWindow_HeaderRawData}:",
                    scrollText = content,
                    buttons = KwyttoDialog.basicOkButtonBar
                });
                instance.Instances[type] = basicInstance;
            }
        }

        // Token: 0x0600003B RID: 59 RVA: 0x00004020 File Offset: 0x00002220
        public void OnSaveData()
        {
            if (BasicIUserMod.DebugMode) LogUtils.DoLog($"SAVING DATA {GetType()}");
            if (instance?.Instances is null)
            {
                return;
            }

            foreach (Type type in instance.Instances.Keys)
            {
                if (instance.Instances[type]?.SaveId == null || Singleton<ToolManager>.instance.m_properties.m_mode != ItemClass.Availability.Game)
                {
                    continue;
                }


                byte[] data = instance.Instances[type]?.Serialize();
                if (BasicIUserMod.DebugMode)
                {
                    string content = System.Text.Encoding.UTF8.GetString(data);
                    if (BasicIUserMod.DebugMode) LogUtils.DoLog($"{type} DATA (L = {data?.Length}) =>  {content}");
                }
                if (data is null || data.Length == 0)
                {
                    SerializableDataManager.EraseData(instance.Instances[type].SaveId);
                    continue;
                }
                try
                {
                    SerializableDataManager.SaveData(instance.Instances[type].SaveId, data);
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog($"Exception trying to serialize {type}: {e} -  {e.Message}\n{e.StackTrace} ");
                }
            }
        }

        public void OnReleased()
        {
            if (!(instance?.Instances is null))
            {
                foreach (IDataExtension item in instance.Instances?.Values)
                {
                    item?.OnReleased();
                }
                instance.Instances = null;
            }
        }
        #endregion
    }
}
