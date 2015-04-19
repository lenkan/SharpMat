using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpMat.Tests
{
    public static class MatStreamReaderFactory
    {
        public static MatStreamReader CreateWithData(params byte[] data)
        {
            return CreateWithDataAndEncoding(Encoding.Default, data);
        }

        public static MatStreamReader CreateWithDataAndEncoding(Encoding encoding, params byte[] data)
        {
            var stream = new MemoryStream(data);
            return new MatStreamReader(stream, encoding);
        }

        public static MatStreamReader CreateWithInt16(Endianness endianness, params short[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 2, values);
        }

        public static MatStreamReader CreateWithInt32(Endianness endianness, params int[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 4, values);
        }

        public static MatStreamReader CreateWithInt64(Endianness endianness, params long[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 8, values);
        }

        public static MatStreamReader CreateWithUint16(Endianness endianness, params ushort[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 2, values);
        }

        public static MatStreamReader CreateWithUint32(Endianness endianness, params uint[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 4, values);
        }

        public static MatStreamReader CreateWithUint64(Endianness endianness, params ulong[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 8, values);
        }

        public static MatStreamReader CreateWithSingle(Endianness endianness, params float[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 4, values);
        }

        public static MatStreamReader CreateWithDouble(Endianness endianness, params double[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 8, values);
        }

        public static MatStreamReader CreateWithValues<T>(Endianness endianness, Func<T, byte[]> converter, int valueSize, params T[] values)
        {
            byte[] data = new byte[values.Length * valueSize];
            for (int ii = 0; ii < values.Length; ++ii)
            {
                var bytes = converter(values[ii]);
                if (IsDifferentEndianness(endianness))
                {
                    bytes = bytes.Reverse().ToArray();
                }

                for (int jj = 0; jj < bytes.Length; ++jj)
                {
                    data[valueSize*ii + jj] = bytes[jj];
                }
            }
            return CreateWithData(data);    
        }

        public static bool IsDifferentEndianness(Endianness endianness)
        {
            return (endianness == Endianness.LittleEndian && !BitConverter.IsLittleEndian) ||
                   (endianness == Endianness.BigEndian && BitConverter.IsLittleEndian);
        }
    }
}