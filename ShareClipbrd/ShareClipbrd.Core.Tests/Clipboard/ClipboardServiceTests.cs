using System.Text;
using ShareClipbrd.Core.Clipboard;

namespace ShareClipbrd.Core.Tests.Clipboard {
    public class ClipboardServiceTests {

        ClipboardSerializer testable;

        [SetUp]
        public void Setup() {
            testable = new();
        }

        [TearDown]
        public void Teardown() {
        }

        bool DataFormats_Test(string dataFormat, object data, Encoding encoding) {
            var clipboardData = new ClipboardData();
            testable.SerializeDataObjects(clipboardData, new[] { dataFormat }, (f) => { if(f == dataFormat) return data; else return new object(); });
            Assert.That(clipboardData.Formats.Select(x => x.Format), Is.EquivalentTo(new[] { dataFormat }));
            Assert.That(clipboardData.Formats.Select(x => x.Data), Is.EquivalentTo(new[] { new MemoryStream(encoding.GetBytes($"{dataFormat} Кирилица")) }));
            return true;


        }

        [Test]
        public void DataFormats_Text_Test() {
            Assert.That(DataFormats_Test(ClipboardData.Format.Text, $"{ClipboardData.Format.Text} Кирилица", System.Text.Encoding.UTF8));
        }
        [Test]
        public void DataFormats_Text_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new ClipboardData(), new[] { ClipboardData.Format.Text }, (f) => new object()));
        }

        [Test]
        public void DataFormats_UnicodeText_Test() {
            Assert.That(DataFormats_Test(ClipboardData.Format.UnicodeText, $"{ClipboardData.Format.UnicodeText} Кирилица", System.Text.Encoding.Unicode));
        }
        [Test]
        public void DataFormats_UnicodeText_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new ClipboardData(), new[] { ClipboardData.Format.UnicodeText }, (f) => new object()));
        }

        [Test]
        public void DataFormats_StringFormat_Test() {
            Assert.That(DataFormats_Test(ClipboardData.Format.StringFormat, $"{ClipboardData.Format.StringFormat} Кирилица", System.Text.Encoding.UTF8));
        }
        [Test]
        public void DataFormats_StringFormat_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new ClipboardData(), new[] { ClipboardData.Format.StringFormat }, (f) => new object()));
        }

        [Test]
        public void DataFormats_OemText_Test() {
            Assert.That(DataFormats_Test(ClipboardData.Format.OemText, $"{ClipboardData.Format.OemText} Кирилица", System.Text.Encoding.ASCII));
        }
        [Test]
        public void DataFormats_OemText_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new ClipboardData(), new[] { ClipboardData.Format.OemText }, (f) => new object()));
        }

        [Test]
        public void DataFormats_Rtf_Test() {
            Assert.That(DataFormats_Test(ClipboardData.Format.Rtf, $"{ClipboardData.Format.Rtf} Кирилица", System.Text.Encoding.UTF8));
        }
        [Test]
        public void DataFormats_Rtf_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new ClipboardData(), new[] { ClipboardData.Format.Rtf }, (f) => new object()));
        }

        [Test]
        public void DataFormats_Locale_Test() {
            var clipboardData = new ClipboardData();
            testable.SerializeDataObjects(clipboardData, new[] { ClipboardData.Format.Locale }, (f) => {
                if(f == ClipboardData.Format.Locale) return new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 }); else return new object();
            });

            Assert.That(clipboardData.Formats.Select(x => x.Format), Is.EquivalentTo(new[] { ClipboardData.Format.Locale }));
            Assert.That(clipboardData.Formats.Select(x => x.Data), Is.EquivalentTo(new[] { new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 }) }));
        }
        [Test]
        public void DataFormats_Locale_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new ClipboardData(), new[] { ClipboardData.Format.Locale }, (f) => new object()));
        }
    }
}