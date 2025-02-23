using ICities;
using Kwytto.Interfaces;
using Kwytto.Libraries;
using Kwytto.Utils;
using System;
using System.IO;
using System.Text;

namespace Kwytto.Data
{
    public abstract class DataExtensionByteLibBase<LIB, DESC> : BasicLib<LIB, DESC>, IDataExtension
        where LIB : DataExtensionByteLibBase<LIB, DESC>, IDataByteSerializable, new()
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
            if (BasicIUserMod.DebugMode) LogUtils.DoLog($"Deserializing {GetType()}");
            LIB lib;
            using (var stream = new MemoryStream(data))
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8);
                reader.ReadAs(out lib);
            }

            AfterDeserialize(lib);
            return lib;
        }

        public byte[] Serialize()
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.WriteAs(Instance);
            }
            return stream.ToArray();
        }



        public virtual void OnReleased() { }
        public virtual void AfterDeserialize(LIB instance) { }
        public virtual LIB LoadDefaults(ISerializableData serializableData) { return null; }
        IDataExtension IDataExtension.LoadDefaults(ISerializableData serializableData) => LoadDefaults(serializableData);

        public event Action EventDataChanged;

        protected override void Save() => EventDataChanged?.Invoke();
    }
}
