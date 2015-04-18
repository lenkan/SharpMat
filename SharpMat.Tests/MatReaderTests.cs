using System;
using System.Collections.Generic;
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
        #region Reading element and tags tests

        [Test]
        public void ItShouldSupportReadingNormalElementTag()
        {
            //Data type = 1 => MiInt8, size = 255
            var data = new byte[] {0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF };
            var reader = MatReaderFactory.CreateWithData(data);
            reader.Endianness = Endianness.BigEndian;

            var tag = reader.ReadElementTag();
            Assert.That(tag.DataType, Is.EqualTo(MatDataType.MiInt8));
            Assert.That(tag.DataSize, Is.EqualTo(255));
        }

        [Test]
        public void ItShouldSupportReadingSmallFormatElementTag()
        {
            var data = new byte[] {0x00, 0x02, 0x00, 0x01};
            var reader = MatReaderFactory.CreateWithData(data);
            reader.Endianness = Endianness.BigEndian;

            var tag = reader.ReadElementTag();
            Assert.That(tag.DataType, Is.EqualTo(MatDataType.MiInt8));
            Assert.That(tag.DataSize, Is.EqualTo(2));
        }

        [TestCase(MatArrayType.MxCell)]
        [TestCase(MatArrayType.MxChar)]
        [TestCase(MatArrayType.MxDouble)]
        [TestCase(MatArrayType.MxInt16)]
        [TestCase(MatArrayType.MxInt32)]
        [TestCase(MatArrayType.MxInt64)]
        [TestCase(MatArrayType.MxInt8)]
        [TestCase(MatArrayType.MxObject)]
        [TestCase(MatArrayType.MxSingle)]
        [TestCase(MatArrayType.MxSparse)]
        [TestCase(MatArrayType.MxStruct)]
        [TestCase(MatArrayType.MxUInt16)]
        [TestCase(MatArrayType.MxUInt32)]
        [TestCase(MatArrayType.MxUInt64)]
        [TestCase(MatArrayType.MxUInt8)]
        public void ItShouldSupportReadingArrayFlags(MatArrayType arrayType)
        {
            const byte complex = 1 << 3;
            const byte global = 1 << 2;
            const byte logical = 1 << 1;

            byte[] data = new byte[8*7];
            data[2] = complex;
            data[3] = (byte)arrayType;
            data[8 + 2] = global;
            data[8 + 3] = (byte) arrayType;
            data[16 + 2] = logical;
            data[16 + 3] = (byte)arrayType;
            data[24 + 2] = complex | global;
            data[24 + 3] = (byte)arrayType;
            data[32 + 2] = complex | logical;
            data[32 + 3] = (byte)arrayType;
            data[40 + 2] = complex | logical | global;
            data[40 + 3] = (byte)arrayType;
            data[48 + 2] = logical | global;
            data[48 + 3] = (byte)arrayType;


            var reader = MatReaderFactory.CreateWithData(data);
            reader.Endianness = Endianness.BigEndian;
            MatArrayFlags[] flagses = 
            {
                reader.ReadArrayFlags(),
                reader.ReadArrayFlags(),
                reader.ReadArrayFlags(),
                reader.ReadArrayFlags(),
                reader.ReadArrayFlags(),
                reader.ReadArrayFlags(),
                reader.ReadArrayFlags()
            };

            Assert.That(flagses.Select(x => x.ArrayType), Is.All.EqualTo(arrayType));
            Assert.That(flagses.Select(x => new[] {x.Complex, x.Global, x.Logical}), Is.EquivalentTo(new List<bool[]>
            {
                new []{true, false, false},
                new []{false, true, false},
                new []{false, false, true},
                new []{true, true, false},
                new []{true, false, true},
                new []{true, true, true},
                new []{false, true, true},
            }));
        }

        #endregion

        #region Reading value tests

        [Test]
        public void ItShouldSupportReadingBytesFromStream()
        {
            var data = new byte[] {0x01, 0x02};
            var reader = MatReaderFactory.CreateWithData(data);
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
            var reader = MatReaderFactory.CreateWithInt16(endianness, 37);
            reader.Endianness = endianness;
            Assert.That(reader.ReadInt16(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingInt32(Endianness endianness)
        {
            var reader = MatReaderFactory.CreateWithInt32(endianness, 37);
            reader.Endianness = endianness;
            Assert.That(reader.ReadInt32(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingInt64(Endianness endianness)
        {
            var reader = MatReaderFactory.CreateWithInt64(endianness, 37);
            reader.Endianness = endianness;
            Assert.That(reader.ReadInt64(), Is.EqualTo(37));
        }


        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingUint16(Endianness endianness)
        {
            var reader = MatReaderFactory.CreateWithUint16(endianness, 37);
            reader.Endianness = endianness;
            Assert.That(reader.ReadUInt16(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingUint32(Endianness endianness)
        {
            var reader = MatReaderFactory.CreateWithUint32(endianness, 37);
            reader.Endianness = endianness;
            Assert.That(reader.ReadUInt32(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingUint64(Endianness endianness)
        {
            var reader = MatReaderFactory.CreateWithUint64(endianness, 37);
            reader.Endianness = endianness;
            Assert.That(reader.ReadUInt64(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingSingle(Endianness endianness)
        {
            var reader = MatReaderFactory.CreateWithSingle(endianness, 37);
            reader.Endianness = endianness;
            Assert.That(reader.ReadSingle(), Is.EqualTo(37));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingDouble(Endianness endianness)
        {
            var reader = MatReaderFactory.CreateWithDouble(endianness, 789.231d);
            reader.Endianness = endianness;
            Assert.That(reader.ReadDouble(), Is.EqualTo(789.231d));
        }

        [TestCase(Endianness.BigEndian)]
        [TestCase(Endianness.LittleEndian)]
        [TestCase(Endianness.Default)]
        public void ItShouldSupportReadingDoubleArray(Endianness endianness)
        {
            var reader = MatReaderFactory.CreateWithDouble(endianness, 1d, 2, 3);
            reader.Endianness = endianness;
            Assert.That(reader.ReadDoubles(3), Is.EquivalentTo(new []{1d, 2, 3}));
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
            var reader = MatReaderFactory.CreateWithDataAndEncoding(encoding, data);
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
            var reader = MatReaderFactory.CreateWithDataAndEncoding(encoding, data);
            Assert.That(reader.ReadChars(5), Is.EquivalentTo(new []{'c', 'h', 'a', 'r', 's'}));
        }

        [TestCase("UTF-8")]
        [TestCase("UTF-32")]
        [TestCase("ASCII")]
        public void ItShouldSupportReadingStrings(string encodingName)
        {
            var encoding = Encoding.GetEncoding(encodingName);
            byte[] data = encoding.GetBytes("string");
            var reader = MatReaderFactory.CreateWithDataAndEncoding(encoding, data);
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
            var reader = MatReaderFactory.CreateWithData(0x00, 0x01);
            reader.Dispose();

            Assert.That(() => reader.ReadByte(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadChar(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadChars(1), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadDouble(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadElementTag(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadInt16(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadInt32(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadInt32Array(1), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => reader.ReadInt64(), Throws.InstanceOf<ObjectDisposedException>());
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

            var reader = MatReaderFactory.CreateWithData(data);
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
    }
}
