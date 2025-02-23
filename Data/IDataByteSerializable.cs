using System.IO;

namespace Kwytto.Data
{
    public interface IDataByteSerializable
    {
        void WriteData(BinaryWriter writer);
        void ReadData(BinaryReader reader);
    }
}
