using Kwytto.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace Kwytto.Utils
{
    public static class ByteableSerializableUtils
    {
        private static Dictionary<Type, Func<BinaryReader, object>> CommonReadConversion = new Dictionary<Type, Func<BinaryReader, object>>
        {
            [typeof(string)] = (reader) => reader.ReadString(),
            [typeof(int)] = (reader) => reader.ReadInt32(),
            [typeof(uint)] = (reader) => reader.ReadUInt32(),
            [typeof(short)] = (reader) => reader.ReadInt16(),
            [typeof(ushort)] = (reader) => reader.ReadUInt16(),
            [typeof(byte)] = (reader) => reader.ReadByte(),
            [typeof(sbyte)] = (reader) => reader.ReadSByte(),
            [typeof(long)] = (reader) => reader.ReadInt64(),
            [typeof(ulong)] = (reader) => reader.ReadUInt64(),
            [typeof(float)] = (reader) => reader.ReadSingle(),
            [typeof(decimal)] = (reader) => reader.ReadDecimal(),
            [typeof(double)] = (reader) => reader.ReadDouble(),
            [typeof(Vector2)] = (reader) => new Vector2(reader.ReadSingle(), reader.ReadSingle()),
            [typeof(Vector3)] = (reader) => new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
            [typeof(Vector4)] = (reader) => new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
            [typeof(byte[])] = (reader) => reader.ReadBytes(reader.ReadInt32()),

        };

        private static Dictionary<Type, Action<BinaryWriter, object>> CommonWriteConversion = new()
        {
            [typeof(string)] = (writer, x) => writer.Write((string)x),
            [typeof(int)] = (writer, x) => writer.Write((int)x),
            [typeof(uint)] = (writer, x) => writer.Write((uint)x),
            [typeof(short)] = (writer, x) => writer.Write((short)x),
            [typeof(ushort)] = (writer, x) => writer.Write((ushort)x),
            [typeof(byte)] = (writer, x) => writer.Write((byte)x),
            [typeof(sbyte)] = (writer, x) => writer.Write((sbyte)x),
            [typeof(long)] = (writer, x) => writer.Write((long)x),
            [typeof(ulong)] = (writer, x) => writer.Write((ulong)x),
            [typeof(float)] = (writer, x) => writer.Write((float)x),
            [typeof(decimal)] = (writer, x) => writer.Write((decimal)x),
            [typeof(double)] = (writer, x) => writer.Write((double)x),
            [typeof(Vector2)] = (writer, x) => { writer.Write(((Vector2)x).x); writer.Write(((Vector2)x).y); },
            [typeof(Vector3)] = (writer, x) => { writer.Write(((Vector3)x).x); writer.Write(((Vector3)x).y); writer.Write(((Vector3)x).z); },
            [typeof(Vector4)] = (writer, x) => { writer.Write(((Vector4)x).x); writer.Write(((Vector4)x).y); writer.Write(((Vector4)x).z); writer.Write(((Vector4)x).w); },
            [typeof(byte[])] = (writer, x) => { writer.Write(((byte[])x).Length); writer.Write((byte[])x); },
        };

        private static object ReadAs_switch(BinaryReader reader, Type type)
        {
            if (type.IsClass && !reader.ReadBoolean())
            {
                return null;
            }
            if (CommonReadConversion.TryGetValue(type, out var fn))
            {
                return fn(reader);
            }
            else if (type.IsAssignableFrom(typeof(IDataByteSerializable)))
            {
                return ReadAs_serializable(reader, type);
            }
            else if (type.IsArray || type.IsAssignableFrom(typeof(IList<>)))
            {
                return ReadAs_listOrArray(reader, type);
            }
            else
            {
                throw new NotImplementedException($"The type {type} can't be byte-serializable");
            }

        }
        private static T ReadAs_switch<T>(BinaryReader reader) => (T)ReadAs_switch(reader, typeof(T));
        private static object ReadAs_listOrArray(BinaryReader reader, Type type)
        {
            var count = reader.ReadInt32();
            Type listType;
            if (type.IsArray)
            {
                listType = type.GetElementType();
            }
            else
            {
                var listGenType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
                listType = listGenType.GetGenericParameterConstraints()[0];
            }
            var result = new object[count];
            for (int i = 0; i < count; i++)
            {
                result[0] = ReadAs_switch(reader, listType);
            }

            var constructor = type.GetConstructor(new Type[0]);

            var s = (IDataByteSerializable)constructor.Invoke(new object[0]);
            s.ReadData(reader);
            return s;
        }
        private static void ReadAs_array(BinaryReader reader, Array value) { }
        private static object ReadAs_serializable(BinaryReader reader, Type type)
        {
            var constructor = type.GetConstructor(new Type[0]);
            var s = (IDataByteSerializable)constructor.Invoke(new object[0]);
            s.ReadData(reader);
            return s;
        }


        private static void WriteAs_switch<T>(BinaryWriter writer, T value)
        {
            if (value is null)
            {
                writer.Write(false);
                return;
            }
            Type type = value.GetType();

            if (CommonWriteConversion.TryGetValue(type, out var fn))
            {
                fn(writer, value);
            }
            else if (type.IsAssignableFrom(typeof(IDataByteSerializable)))
            {
                if (type.IsClass) writer.Write(true);
                WriteAs_serializable(writer, (IDataByteSerializable)value);
            }
            else if (type.IsArray)
            {
                writer.Write(true);
                WriteAs_array(writer, value as Array);
            }
            else if (type.IsAssignableFrom(typeof(IList<>)))
            {
                writer.Write(true);
                WriteAs_list(writer, (IList)value);
            }
            else
            {
                throw new NotImplementedException($"The type {type} can't be byte-serializable");
            }
        }
        private static void WriteAs_list(BinaryWriter writer, IList value)
        {
            writer.WriteAs(value.Count);
            foreach (var item in value)
            {
                WriteAs_switch(writer, item);
            }
        }
        private static void WriteAs_array(BinaryWriter writer, Array value)
        {
            writer.WriteAs(value.Length);
            foreach (var item in value)
            {
                WriteAs_switch(writer, item);
            }
        }
        private static void WriteAs_serializable<T>(BinaryWriter writer, T value) where T : IDataByteSerializable
        {
            value.WriteData(writer);
        }

        public static void WriteAs(this BinaryWriter writer, string value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, int value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, uint value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, short value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, ushort value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, byte value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, sbyte value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, long value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, ulong value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, float value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, decimal value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, double value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, Vector2 value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, Vector3 value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, Vector4 value) => WriteAs_switch(writer, value);
        public static void WriteAs(this BinaryWriter writer, byte[] value) => WriteAs_switch(writer, value);
        public static void WriteAs<T>(this BinaryWriter writer, T[] value) => WriteAs_switch(writer, value);
        public static void WriteAs<T>(this BinaryWriter writer, List<T> value) => WriteAs_switch(writer, value);
        public static void WriteAs<T>(this BinaryWriter writer, T value) where T : IDataByteSerializable => WriteAs_switch(writer, value);




        public static void ReadAs(this BinaryReader reader, out string value) => value = ReadAs_switch<string>(reader);
        public static void ReadAs(this BinaryReader reader, out int value) => value = ReadAs_switch<int>(reader);
        public static void ReadAs(this BinaryReader reader, out uint value) => value = ReadAs_switch<uint>(reader);
        public static void ReadAs(this BinaryReader reader, out short value) => value = ReadAs_switch<short>(reader);
        public static void ReadAs(this BinaryReader reader, out ushort value) => value = ReadAs_switch<ushort>(reader);
        public static void ReadAs(this BinaryReader reader, out byte value) => value = ReadAs_switch<byte>(reader);
        public static void ReadAs(this BinaryReader reader, out sbyte value) => value = ReadAs_switch<sbyte>(reader);
        public static void ReadAs(this BinaryReader reader, out long value) => value = ReadAs_switch<long>(reader);
        public static void ReadAs(this BinaryReader reader, out ulong value) => value = ReadAs_switch<ulong>(reader);
        public static void ReadAs(this BinaryReader reader, out float value) => value = ReadAs_switch<float>(reader);
        public static void ReadAs(this BinaryReader reader, out decimal value) => value = ReadAs_switch<decimal>(reader);
        public static void ReadAs(this BinaryReader reader, out double value) => value = ReadAs_switch<double>(reader);
        public static void ReadAs(this BinaryReader reader, out Vector2 value) => value = ReadAs_switch<Vector2>(reader);
        public static void ReadAs(this BinaryReader reader, out Vector3 value) => value = ReadAs_switch<Vector3>(reader);
        public static void ReadAs(this BinaryReader reader, out Vector4 value) => value = ReadAs_switch<Vector4>(reader);
        public static void ReadAs(this BinaryReader reader, out byte[] value) => value = ReadAs_switch<byte[]>(reader);
        public static void ReadAs<X, T>(this BinaryReader reader, out T[] value) => value = ReadAs_switch<T[]>(reader);
        public static void ReadAs<L, T>(this BinaryReader reader, out L value) where L : IList<T>, new() => value = ReadAs_switch<L>(reader);
        public static void ReadAs<T>(this BinaryReader reader, out T value) where T : IDataByteSerializable, new() => value = ReadAs_switch<T>(reader);
    }
}
