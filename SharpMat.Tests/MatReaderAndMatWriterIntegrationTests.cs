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
    }
}
