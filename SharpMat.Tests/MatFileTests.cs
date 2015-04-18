using System.IO;
using NUnit.Framework;

namespace SharpMat.Tests
{
    [TestFixture]
    public class MatFileTests
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
    }
}
