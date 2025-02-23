using ICities;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kwytto.Data
{
    public abstract class DataSerializableByteBase<U> : IDataByteSerializable where U : DataSerializableByteBase<U>, new()
    {
        protected abstract int CurrentVersion { get; }
        protected abstract void ApplyDefaults(int refVersion);
        protected virtual void AfterLoad(int refVersion) { }
        protected abstract Dictionary<int, Dictionary<string, FieldData>> fieldOrderByVersion { get; }
        protected void WriteData(BinaryWriter writer)
        {
            writer.Write(CurrentVersion);
            var fieldOrder = fieldOrderByVersion[CurrentVersion];
            foreach (var entry in fieldOrder)
            {
                entry.Value.Write(writer);
            }
        }
        protected void ReadData(BinaryReader reader)
        {
            var version = reader.ReadInt32();
            if (fieldOrderByVersion.TryGetValue(version, out var fieldOrder)) throw new Exception($"Version for {typeof(U)} is invalid! Valid: {string.Join(",", fieldOrderByVersion.Keys.Select(x => x.ToString()).ToArray())}");
            if (version != CurrentVersion) ApplyDefaults(version);
            foreach (var entry in fieldOrder)
            {
                var type = GetType().GetField(entry.Key);
                entry.Value.Read(reader);
            }
            AfterLoad(version);
        }
        void IDataByteSerializable.WriteData(BinaryWriter writer) => WriteData(writer);
        void IDataByteSerializable.ReadData(BinaryReader reader) => ReadData(reader);

        protected class FieldData
        {
            public FieldData(Action<BinaryWriter> write, Action<BinaryReader> read)
            {
                Write = write;
                Read = read;
            }
            public Action<BinaryWriter> Write { get; private set; }
            public Action<BinaryReader> Read { get; private set; }
        }
    }


    public abstract class DataExtensionByteBase<U> : DataSerializableByteBase<U>, IDataExtension where U : DataExtensionByteBase<U>, new()
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
            protected set
            {
                var newData = new U();
                newData.Deserialize(value.Serialize());
                DataContainer.instance.Instances[typeof(U)] = newData;
            }
        }

        public virtual bool IsLegacyCompatOnly { get; } = false;

        public IDataExtension Deserialize(byte[] data)
        {
            if (BasicIUserMod.DebugMode) LogUtils.DoLog($"Deserializing {typeof(U)}");

            using (var stream = new MemoryStream(data))
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8);
                ReadData(reader);
            }

            AfterDeserialize(this);
            return this;
        }

        public byte[] Serialize()
        {
            BeforeSerialize();
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                WriteData(writer);
            }

            return stream.ToArray();
        }


        public virtual void OnReleased() { }

        public virtual U LoadDefaults(ISerializableData serializableData, int version) { return null; }
        IDataExtension IDataExtension.LoadDefaults(ISerializableData serializableData) => LoadDefaults(serializableData, int.MinValue);
        protected override void ApplyDefaults(int refVersion) => LoadDefaults(null, refVersion);
        public virtual void BeforeSerialize() { }
        public virtual void AfterDeserialize(DataExtensionByteBase<U> loadedData) { }
    }
}
