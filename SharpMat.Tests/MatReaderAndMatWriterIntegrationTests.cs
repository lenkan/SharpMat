using System.IO;
using NUnit.Framework;

namespace SharpMat.Tests
{
    [TestFixture]
    public class MatReaderAndMatWriterIntegrationTests
    {
        [Test]
        public void TheyShouldReadAndWriteSameHeader()
        {
            var written = new MatHeader("This is the header text.");
            MatHeader read;

            byte[] data;
            using (var stream = new MemoryStream())
            {
                var writer = new MatWriter(stream);
                writer.WriteHeader(written);

                data = stream.ToArray();
            }

            using (var stream = new MemoryStream(data))
            {
                var reader = new MatReader(stream);
                read = reader.ReadHeader();
            }

            Assert.That(read.DescriptiveText, Is.EqualTo(written.DescriptiveText));
            Assert.That(read.Version, Is.EqualTo(written.Version));
            Assert.That(read.EndianIndicator, Is.EqualTo(written.EndianIndicator));
        }

        [Test]
        public void TheyShouldReadAndWriteSmallDataElements()
        {
            byte[] data;
            using (var stream = new MemoryStream())
            {
                var writer = new MatWriter(stream);
                writer.WriteHeader(new MatHeader("HEADER"));
                writer.WriteElementTag(new MatElementTag(MatDataType.MiUInt8, 4));
                writer.WriteByte(0x01);
                writer.WriteByte(0x02);
                writer.WriteByte(0x03);
                writer.WriteByte(0x04);
                data = stream.ToArray();
            }

            var reader = MatReaderFactory.CreateWithData(data);
            var tag = reader.ReadNextElementTag();
            Assert.That(tag.DataType, Is.EqualTo(MatDataType.MiUInt8));
            Assert.That(tag.DataSize, Is.EqualTo(4));
            Assert.That(reader.ReadByte(), Is.EqualTo(1));
            Assert.That(reader.ReadByte(), Is.EqualTo(2));
            Assert.That(reader.ReadByte(), Is.EqualTo(3));
            Assert.That(reader.ReadByte(), Is.EqualTo(4));
            Assert.That(() => reader.ReadByte(), Throws.InstanceOf<EndOfStreamException>());
        }
    }
}
