using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Helpers;

namespace ShareClipbrd.Core.Tests.Clipboard {
    public class ImageConverterTests {

        [Test]
        public void FromDib_MemoryStream_Test() {
            if(OperatingSystem.IsWindows()) {
                var stream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 });
                var obj = ImageConverter.FromDib(stream);
                Assert.That(obj, Is.InstanceOf<MemoryStream>());
                var memoryStream = (MemoryStream)obj;
                Assert.That(memoryStream.ToArray(), Is.EquivalentTo(new byte[] { 0x00, 0x01, 0x02, 0x03 }));
            }

            if(OperatingSystem.IsLinux()) {
                var streamDib = new MemoryStream(TestData.Dib_32x32);
                var obj = ImageConverter.FromDib(streamDib);
                Assert.That(obj, Is.InstanceOf<byte[]>());
                var data = (byte[])obj;
                Assert.That(data, Has.Length.GreaterThan(StructHelper.Size<BitmapFile.BITMAPFILEHEADER>()));
                var bmpHeader = StructHelper.FromBytes<BitmapFile.BITMAPFILEHEADER>(data);
                Assert.That(bmpHeader.bfType, Is.EqualTo(0x4D42));
                Assert.That(bmpHeader.bfSize, Is.LessThan(data.Length).And.GreaterThan(StructHelper.Size<BitmapFile.BITMAPFILEHEADER>()));
            }
        }

        [Test]
        public void FromDib_Null_Throws_ArgumentException() {
            Assert.Throws<ArgumentException>(() => ImageConverter.FromDib(null));
        }

        [Test]
        public void FromDib_FileStream_Throws_ArgumentException() {
            using(var fs = new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate)) {
                Assert.Throws<ArgumentException>(() => ImageConverter.FromDib(fs));
            }
        }

        [Test]
        public void FromDibToBmpFileData_Test() {
            var streamDib = new MemoryStream(TestData.Dib_32x32);
            var data = ImageConverter.FromDibToBmpFileData(streamDib);
            Assert.That(data, Has.Length.GreaterThan(StructHelper.Size<BitmapFile.BITMAPFILEHEADER>()));
            var bmpHeader = StructHelper.FromBytes<BitmapFile.BITMAPFILEHEADER>(data);
            Assert.That(bmpHeader.bfType, Is.EqualTo(0x4D42));
            Assert.That(bmpHeader.bfSize, Is.LessThan(data.Length).And.GreaterThan(StructHelper.Size<BitmapFile.BITMAPFILEHEADER>()));
        }

        [Test]
        public void FromDibToBmpFileData_For_Incorrect_Data_Length_Throws_ArgumentException() {
            var streamDib = new MemoryStream(TestData.Dib_32x32.Skip(1).ToArray());
            var ex = Assert.Throws<ArgumentException>(() => ImageConverter.FromDibToBmpFileData(streamDib));
            Assert.That(ex.Message, Is.EqualTo("Deserialize BITMAPINFO. data invalid"));
        }

        [Test]
        public void FromDibToBmpFileData_For_Incorrect_Header_Throws_ArgumentException() {
            var data = TestData.Dib_32x32.ToArray();
            data[0]--;
            var streamDib = new MemoryStream(data);
            var ex = Assert.Throws<ArgumentException>(() => ImageConverter.FromDibToBmpFileData(streamDib));
            Assert.That(ex.Message, Is.EqualTo("Deserialize BITMAPINFO. data invalid"));
        }
    }
}
