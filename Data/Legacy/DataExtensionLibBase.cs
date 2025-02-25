﻿using ICities;
using Kwytto.Interfaces;
using Kwytto.Libraries;
using Kwytto.Utils;
using System;
using System.Text;

namespace Kwytto.Data
{
    [Obsolete("Use binary serialization")]
    public abstract class DataExtensionLibBase<LIB, DESC> : BasicLib<LIB, DESC>, IDataExtension
        where LIB : DataExtensionLibBase<LIB, DESC>, new()
        where DESC : class, ILibable
    {
        public abstract string SaveId { get; }

        public bool IsLegacyCompatOnly { get; } = false;
        public static LIB Instance
        {
            get
            {
                if (!DataContainer.instance.Instances.TryGetValue(typeof(LIB), out IDataExtension result) || result is null)
                {
                    var newItem = new LIB();
                    newItem.AfterDeserialize(newItem);
                    DataContainer.instance.Instances[typeof(LIB)] = newItem;
                }
                return DataContainer.instance.Instances[typeof(LIB)] as LIB;
            }
        }


        public IDataExtension Deserialize(byte[] data)
        {
            string content = data[0] == '<' ? Encoding.UTF8.GetString(data) : ZipUtils.Unzip(data);
            var result = XmlUtils.DefaultXmlDeserialize<LIB>(content);
            AfterDeserialize(result);
            return result;
        }

        public byte[] Serialize() => !IsLegacyCompatOnly ? ZipUtils.Zip(XmlUtils.DefaultXmlSerialize((LIB)this, false)) : null;

        public virtual void OnReleased() { }
        public virtual void AfterDeserialize(LIB instance) { }
        public virtual LIB LoadDefaults(ISerializableData serializableData) { return null; }
        IDataExtension IDataExtension.LoadDefaults(ISerializableData serializableData) => LoadDefaults(serializableData);

        public event Action EventDataChanged;

        protected override void Save() => EventDataChanged?.Invoke();
    }
}
