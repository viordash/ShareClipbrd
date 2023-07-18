using ShareClipbrd.Core.Clipboard;

namespace ShareClipbrd.Core.Tests.Clipboard {
    public class ImageConverterTests {

        [Test]
        public void FromDib_MemoryStream_Test() {
            var stream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 });

            Assert.That(ImageConverter.FromDib(stream), Is.EquivalentTo(new byte[] { 0x00, 0x01, 0x02, 0x03 }));
        }

        [Test]
        public void FromDib_Null_Throws_ArgumentException() {
            Assert.Throws<ArgumentException>(() => ImageConverter.FromDib(null));
        }

        [Test]
        public void FromDib_FileStream_Throws_ArgumentException() {
            using(var fs = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate))
                Assert.Throws<ArgumentException>(() => ImageConverter.FromDib(fs));
        }
    }
}
