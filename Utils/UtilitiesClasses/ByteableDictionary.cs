using Kwytto.Data;
using System.Collections.Generic;
using System.IO;


namespace Kwytto.Utils
{
    public abstract class ByteableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDataByteSerializable
    {

        public void ReadData(BinaryReader reader)
        {
            Clear();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                this[ReadKey(reader)] = ReadValue(reader);
            }
        }

        public void WriteData(BinaryWriter writer)
        {
            writer.Write(Count);
            foreach (var entry in this)
            {
                SerializeKey(writer, entry.Key);
                SerializeValue(writer, entry.Value);
            }
        }
        protected abstract TKey ReadKey(BinaryReader reader);
        protected abstract TValue ReadValue(BinaryReader reader);
        protected abstract void SerializeKey(BinaryWriter writer, TKey k);
        protected abstract void SerializeValue(BinaryWriter writer, TValue v);
    }

    public static class ByteableDictionary
    {
        public abstract class KeyString<TVal> : ByteableDictionary<string, TVal>
        {
            protected override string ReadKey(BinaryReader reader)
            {
                reader.ReadAs(out string value);
                return value;
            }
            protected override void SerializeKey(BinaryWriter writer, string k)
            {
                writer.WriteAs(k);
            }
        }
        public abstract class KeyInt<TVal> : ByteableDictionary<int, TVal>
        {
            protected override int ReadKey(BinaryReader reader)
            {
                reader.ReadAs(out int value);
                return value;
            }
            protected override void SerializeKey(BinaryWriter writer, int k)
            {
                writer.WriteAs(k);
            }
        }
        public abstract class KeyLong<TVal> : ByteableDictionary<long, TVal>
        {
            protected override long ReadKey(BinaryReader reader)
            {
                reader.ReadAs(out long value);
                return value;
            }
            protected override void SerializeKey(BinaryWriter writer, long k)
            {
                writer.WriteAs(k);
            }
        }
        public class KeyStringValueClass<TVal> : KeyString<TVal> where TVal : IDataByteSerializable, new()
        {
            protected override TVal ReadValue(BinaryReader reader)
            {
                reader.ReadAs(out TVal value);
                return value;
            }
            protected override void SerializeValue(BinaryWriter writer, TVal v)
            {
                writer.WriteAs(v);
            }
        }
        public class KeyStringValueString : KeyString<string>
        {
            protected override string ReadValue(BinaryReader reader)
            {
                reader.ReadAs(out string value);
                return value;
            }

            protected override void SerializeValue(BinaryWriter writer, string v)
            {
                writer.WriteAs(v);
            }
        }
    }
}
