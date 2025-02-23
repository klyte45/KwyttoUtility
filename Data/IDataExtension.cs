using ICities;
using System;

namespace Kwytto.Data
{
    public interface IDataExtension
    {
        string SaveId { get; }
        bool IsLegacyCompatOnly { get; }

        IDataExtension LoadDefaults(ISerializableData serializableData);
        IDataExtension Deserialize(byte[] data);
        byte[] Serialize();
        void OnReleased();
    }
}
