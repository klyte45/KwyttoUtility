﻿using ICities;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System;
using System.Text;
using System.Xml.Serialization;

namespace Kwytto.Data
{

    [XmlRoot("DataExtension")]
    [Obsolete("Use binary serialization")]
    public abstract class DataExtensionBase<U> : IDataExtension where U : DataExtensionBase<U>, new()
    {
        public abstract string SaveId { get; }

        public static U Instance
        {
            get
            {
                if (!DataContainer.instance.Instances.TryGetValue(typeof(U), out IDataExtension result) || result == null)
                {
                    DataContainer.instance.Instances[typeof(U)] = new U();
                }
                return DataContainer.instance.Instances[typeof(U)] as U;
            }
            protected set => DataContainer.instance.Instances[typeof(U)] = XmlUtils.CloneViaXml(value);
        }

        public virtual bool IsLegacyCompatOnly { get; } = false;

        public IDataExtension Deserialize(byte[] data)
        {
            string content = data[0] == '<' ? Encoding.UTF8.GetString(data) : ZipUtils.Unzip(data);
            if (BasicIUserMod.DebugMode)
            {
               if (BasicIUserMod.DebugMode) LogUtils.DoLog($"Deserializing {typeof(U)}:\n{content}");
            }

            var result = XmlUtils.DefaultXmlDeserialize<U>(content);
            AfterDeserialize(result);
            return result;
        }

        public byte[] Serialize()
        {
            if (IsLegacyCompatOnly)
            {
                return null;
            }
            BeforeSerialize();
            var xml = XmlUtils.DefaultXmlSerialize((U)this, BasicIUserMod.DebugMode);
            if (BasicIUserMod.DebugMode)
            {
               if (BasicIUserMod.DebugMode) LogUtils.DoLog($"Serializing  {typeof(U)}:\n{xml}");
            }

            return ZipUtils.Zip(xml);
        }

        public virtual void OnReleased() { }

        public virtual U LoadDefaults(ISerializableData serializableData) { return null; }
        IDataExtension IDataExtension.LoadDefaults(ISerializableData serializableData) => LoadDefaults(serializableData);
        public virtual void BeforeSerialize() { }
        public virtual void AfterDeserialize(U loadedData) { }
    }
}
