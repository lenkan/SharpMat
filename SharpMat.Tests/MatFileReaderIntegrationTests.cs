using System.IO;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;

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

        [Test]
        public void ItShouldSupportReadingValuesFromMatrix()
        {
            using(var stream = new MemoryStream(Resources.OneDimensionalArray))
            using (var reader = new MatFileReader(stream, Encoding.Default))
            {
                var element = (MiMatrix) reader.ReadElement();
                Assert.That(element.GetValue(0), Is.EqualTo(1));
                Assert.That(element.GetValue(1), Is.EqualTo(2));
                Assert.That(element.GetValue(2), Is.EqualTo(3));
            }
        }

        [Test]
        public void ItShouldSupportAccessingTheValueFromMatrixWithCoordinates()
        {
            using (var stream = new MemoryStream(Resources.OneDimensionalArray))
            using (var reader = new MatFileReader(stream, Encoding.Default))
            {
                var element = (MiMatrix)reader.ReadElement();
                Assert.That(element.GetValue(0,0), Is.EqualTo(1));
                Assert.That(element.GetValue(0,1), Is.EqualTo(2));
                Assert.That(element.GetValue(0,2), Is.EqualTo(3));
            }
        }
    }
}
