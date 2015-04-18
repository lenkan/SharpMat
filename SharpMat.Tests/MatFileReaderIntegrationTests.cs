using System.IO;
using NUnit.Framework;

namespace SharpMat.Tests
{
    [TestFixture(Description = "Tests the intended uses of the API by reading from the file system.")]
    public class MatFileReaderIntegrationTests
    {
        [SetUp]
        public void SetUp()
        {
            File.WriteAllBytes("test.mat", Resources.SingleValue);
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete("test.mat");
        }

        [Test]
        public void ItShouldSupportReadingMatrix()
        {
            using (var reader = new MatFileReader("test.mat"))
            {
                var element = reader.ReadElement();
                Assert.That(element, Is.InstanceOf<MiMatrix>());
            }
        }

        [Test]
        public void ItShouldSupportReadingNameOfMatrix()
        {
            using (var reader = new MatFileReader("test.mat"))
            {
                var element = (MiMatrix)reader.ReadElement();
                Assert.That(element.Name, Is.EqualTo("x"));
            }
        }

        [Test]
        public void ItShouldSupportReadingValueFromMatrix()
        {
            using (var reader = new MatFileReader("test.mat"))
            {
                var element = (MiMatrix) reader.ReadElement();
                Assert.That(element.GetValue(0,0), Is.EqualTo(1));
            }
        }
    }
}
