using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SharpMat.Tests
{
    [TestFixture]
    public class MatReaderTests
    {
        #region Test helper methods
        
        public  enum Endianness
        {
            Default,
            LittleEndian,
            BigEndian
        }
        
        private static MatReader CreateWithData(params byte[] data)
        {
            return CreateWithDataAndEncoding(Encoding.Default, data);
        }

        private static MatReader CreateWithDataAndEncoding(Encoding encoding, params byte[] data)
        {
            var stream = new MemoryStream(data);
            return new MatReader(stream, encoding);
        }

        private static MatReader CreateWithInt16(Endianness endianness, params short[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 2, values);
        }

        private static MatReader CreateWithInt32(Endianness endianness, params int[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 4, values);
        }

        private static MatReader CreateWithInt64(Endianness endianness, params long[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 8, values);
        }

        private static MatReader CreateWithUint16(Endianness endianness, params ushort[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 2, values);
        }

        private static MatReader CreateWithUint32(Endianness endianness, params uint[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 4, values);
        }

        private static MatReader CreateWithUint64(Endianness endianness, params ulong[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 8, values);
        }

        private static MatReader CreateWithSingle(Endianness endianness, params float[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 4, values);
        }

        private static MatReader CreateWithDouble(Endianness endianness, params double[] values)
        {
            return CreateWithValues(endianness, BitConverter.GetBytes, 8, values);
        }

        private static MatReader CreateWithValues<T>(Endianness endianness, Func<T, byte[]> converter, int valueSize, params T[] values)
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

        private static bool IsDifferentEndianness(Endianness endianness)
        {
            return (endianness == Endianness.LittleEndian && !BitConverter.IsLittleEndian) ||
                       (endianness == Endianness.BigEndian && BitConverter.IsLittleEndian);
        }

        #endregion

        #region Reading value tests

        [Test]
        public void ItShouldSupportReadingBytesFromStream()
        {
            var data = new byte[] {0x01, 0x02};
            var reader = CreateWithData(data);
            var byte1 = reader.ReadByte();
            var byte2 = reader.ReadByte();

            Assert.That(byte1, Is.EqualTo(data[0]));
            Assert.That(byte2, Is.EqualTo(data[1]));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingInt16(Endianness endianness)
        {
            var reader = CreateWithInt16(endianness, 37);
            reader.RequiresByteSwapping = IsDifferentEndianness(endianness);
            Assert.That(reader.ReadInt16(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingInt32(Endianness endianness)
        {
            var reader = CreateWithInt32(endianness, 37);
            reader.RequiresByteSwapping = IsDifferentEndianness(endianness);
            Assert.That(reader.ReadInt32(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingInt64(Endianness endianness)
        {
            var reader = CreateWithInt64(endianness, 37);
            reader.RequiresByteSwapping = IsDifferentEndianness(endianness);
            Assert.That(reader.ReadInt64(), Is.EqualTo(37));
        }


        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingUint16(Endianness endianness)
        {
            var reader = CreateWithUint16(endianness, 37);
            reader.RequiresByteSwapping = IsDifferentEndianness(endianness);
            Assert.That(reader.ReadUInt16(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingUint32(Endianness endianness)
        {
            var reader = CreateWithUint32(endianness, 37);
            reader.RequiresByteSwapping = IsDifferentEndianness(endianness);
            Assert.That(reader.ReadUInt32(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingUint64(Endianness endianness)
        {
            var reader = CreateWithUint64(endianness, 37);
            reader.RequiresByteSwapping = IsDifferentEndianness(endianness);
            Assert.That(reader.ReadUInt64(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingSingle(Endianness endianness)
        {
            var reader = CreateWithSingle(endianness, 37);
            reader.RequiresByteSwapping = IsDifferentEndianness(endianness);
            Assert.That(reader.ReadSingle(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingDouble(Endianness endianness)
        {
            var reader = CreateWithDouble(endianness, 789.231d);
            reader.RequiresByteSwapping = IsDifferentEndianness(endianness);
            Assert.That(reader.ReadDouble(), Is.EqualTo(789.231d));
        }

        #endregion

        #region Reading characters and text tests

        [TestCase("UTF-8")]
        [TestCase("UTF-32")]
        [TestCase("ASCII")]
        public void ItShouldSupportReadingCharactersOneByOne(string encodingName)
        {
            var encoding = Encoding.GetEncoding(encodingName);
            byte[] data = encoding.GetBytes("chars");
            var reader = CreateWithDataAndEncoding(encoding, data);
            Assert.That(reader.ReadChar(), Is.EqualTo('c'));
            Assert.That(reader.ReadChar(), Is.EqualTo('h'));
            Assert.That(reader.ReadChar(), Is.EqualTo('a'));
            Assert.That(reader.ReadChar(), Is.EqualTo('r'));
            Assert.That(reader.ReadChar(), Is.EqualTo('s'));
        }

        [TestCase("UTF-8")]
        [TestCase("UTF-32")]
        [TestCase("ASCII")]
        public void ItShouldSupportReadingCharacters(string encodingName)
        {
            var encoding = Encoding.GetEncoding(encodingName);
            byte[] data = encoding.GetBytes("chars");
            var reader = CreateWithDataAndEncoding(encoding, data);
            Assert.That(reader.ReadChars(5), Is.EquivalentTo(new []{'c', 'h', 'a', 'r', 's'}));
        }

        [TestCase("UTF-8")]
        [TestCase("UTF-32")]
        [TestCase("ASCII")]
        public void ItShouldSupportReadingStrings(string encodingName)
        {
            var encoding = Encoding.GetEncoding(encodingName);
            byte[] data = encoding.GetBytes("string");
            var reader = CreateWithDataAndEncoding(encoding, data);
            Assert.That(reader.ReadString(6), Is.EqualTo("string"), "Failed with encoding: " + encodingName);
        }

        #endregion

        #region Error handling tests

        [Test]
        public void ItShouldThrowEndOfStreamExceptionWhenReadingByteFromEmptyStream()
        {
            using (var stream = new MemoryStream())
            {
                var reader = new MatReader(stream);
                Assert.That(() => reader.ReadByte(), Throws.InstanceOf<EndOfStreamException>());
            }
        }

        [Test]
        public void ItShouldThrowObjectDisposedExceptionIfUsedAfterDisposabl()
        {
            var reader = CreateWithData(0x00, 0x01);
            reader.Dispose();

            Assert.That(() => reader.ReadByte(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadChar(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadChars(1), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadDouble(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadElementTag(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadHeader(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadInt16(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadInt32(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadInt32Array(1), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadInt64(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadMatrixHeader(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadNextElementTag(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadSingle(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadString(1), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadUInt16(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadUInt32(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadUInt64(), Throws.InstanceOf<ObjectDisposedException>());
        }

        #endregion
        
        #region Decompressiontests

        [Test]
        public void ItShouldSupportDecompressingPartOfTheData()
        {
            byte[] data;
            int compressedSize;

            //Create a data array with part of the data compressed
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream, Encoding.Default, true))
                {
                    binaryWriter.Write((uint) 1);
                }

                long sizeBefore = memoryStream.Length;
                using (var zipStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                using (var binaryWriter = new BinaryWriter(zipStream, Encoding.Default, true))
                {
                    binaryWriter.Write((long)2);
                }
                compressedSize = (int)memoryStream.Length - (int)sizeBefore;

                using (var binaryWriter = new BinaryWriter(memoryStream, Encoding.Default, true))
                {
                    binaryWriter.Write((uint)1);
                }

                data = memoryStream.ToArray();
            }

            var reader = CreateWithData(data);
            var value1 = reader.ReadUInt32();
            reader.BeginDecompress(compressedSize); //Decompress for given number of bytes
            var value2 = reader.ReadInt64();
            reader.EndDecompress();
            var value3 = reader.ReadUInt32();
            
            Assert.That(value1, Is.EqualTo(1));
            Assert.That(value2, Is.EqualTo(2));
            Assert.That(value3, Is.EqualTo(1));
        }

        #endregion

        #region Resources dependent tests

        [Test]
        public void ItShouldBePosssibleToReadNextElementTagAndMatrixWithoutReadingHeaderFirst()
        {
            //This resource has a single element, 'x' = 1
            var reader = CreateWithData(Resources.SingleValue);

            var element = reader.ReadNextElementTag();
            Assert.That(element.DataType, Is.EqualTo(MatDataType.MiMatrix));

            var matrix = reader.ReadMatrixHeader();
            Assert.That(matrix.ArrayType, Is.EqualTo(MatArrayType.MxDouble));
            Assert.That(matrix.Name, Is.EqualTo("x"));
            Assert.That(matrix.Dimensions, Is.EquivalentTo(new []{1, 1}));
        }

        [Test]
        public void ItShouldReturnNullWhenNoMoreElementsToRead()
        {
            //This resource has for matrices, i.e. four elements.
            var reader = CreateWithData(Resources.Testing);

            var elements = new MatElementTag[5];
            elements[0] = reader.ReadNextElementTag();
            elements[1] = reader.ReadNextElementTag();
            elements[2] = reader.ReadNextElementTag();
            elements[3] = reader.ReadNextElementTag();
            elements[4] = reader.ReadNextElementTag();

            Assert.That(elements[0], Is.Not.Null);
            Assert.That(elements[1], Is.Not.Null);
            Assert.That(elements[2], Is.Not.Null);
            Assert.That(elements[3], Is.Not.Null);
            Assert.That(elements[4], Is.Null);
        }

        #endregion
    }
}
