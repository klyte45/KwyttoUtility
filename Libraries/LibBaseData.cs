﻿using ICities;
using Kwytto.Data;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System;
using System.Text;

namespace Kwytto.Libraries
{
    public abstract class LibBaseData<LIB, DESC> : BasicLib<LIB, DESC>, IDataExtension
        where LIB : LibBaseData<LIB, DESC>, new()
        where DESC : class, ILibable
    {
        public abstract string SaveId { get; }
        public static LIB Instance
        {
            get
            {
                if (!DataContainer.instance.Instances.TryGetValue(typeof(LIB), out IDataExtension result) || result == null)
                {
                    DataContainer.instance.Instances[typeof(LIB)] = new LIB();
                }
                return DataContainer.instance.Instances[typeof(LIB)] as LIB;
            }
        }

        public bool IsLegacyCompatOnly { get; } = false;

        public IDataExtension Deserialize(byte[] data)
        {
            string content;
            if (data[0] == '<')
            {
                content = Encoding.UTF8.GetString(data);
            }
            else
            {
                content = ZipUtils.Unzip(data);
            }

            return XmlUtils.DefaultXmlDeserialize<LIB>(content);
        }

        public byte[] Serialize() => ZipUtils.Zip(XmlUtils.DefaultXmlSerialize((LIB)this, false));
        public virtual void OnReleased() { }
        public virtual LIB LoadDefaults(ISerializableData serializableData) { return null; }
        IDataExtension IDataExtension.LoadDefaults(ISerializableData serializableData) => LoadDefaults(serializableData);

        protected override void Save() { }
    }
}
