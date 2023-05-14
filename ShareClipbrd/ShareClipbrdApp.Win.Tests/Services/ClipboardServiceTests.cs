using System.Text;
using System.Windows;
using ShareClipbrdApp.Win.Clipboard;

namespace ShareClipbrdApp.Win.Tests.Services {
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
            var clipboardData = testable.SerializeDataObjects(new[] { dataFormat }, (f) => { if(f == dataFormat) return data; else return new object(); });
            Assert.That(clipboardData.Formats.Keys, Is.EquivalentTo(new[] { dataFormat }));
            Assert.That(clipboardData.Formats.Values, Is.EquivalentTo(new[] { encoding.GetBytes($"{dataFormat} Кирилица") }));
            return true;


        }

        [Test]
        public void DataFormats_Text_Test() {
            Assert.That(DataFormats_Test(DataFormats.Text, $"{DataFormats.Text} Кирилица", System.Text.Encoding.ASCII));
        }
        [Test]
        public void DataFormats_Text_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new[] { DataFormats.Text }, (f) => new object()));
        }

        [Test]
        public void DataFormats_UnicodeText_Test() {
            Assert.That(DataFormats_Test(DataFormats.UnicodeText, $"{DataFormats.UnicodeText} Кирилица", System.Text.Encoding.Unicode));
        }
        [Test]
        public void DataFormats_UnicodeText_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new[] { DataFormats.UnicodeText }, (f) => new object()));
        }

        [Test]
        public void DataFormats_StringFormat_Test() {
            Assert.That(DataFormats_Test(DataFormats.StringFormat, $"{DataFormats.StringFormat} Кирилица", System.Text.Encoding.ASCII));
        }
        [Test]
        public void DataFormats_StringFormat_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new[] { DataFormats.StringFormat }, (f) => new object()));
        }

        [Test]
        public void DataFormats_OemText_Test() {
            Assert.That(DataFormats_Test(DataFormats.OemText, $"{DataFormats.OemText} Кирилица", System.Text.Encoding.ASCII));
        }
        [Test]
        public void DataFormats_OemText_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new[] { DataFormats.OemText }, (f) => new object()));
        }

        [Test]
        public void DataFormats_Rtf_Test() {
            Assert.That(DataFormats_Test(DataFormats.Rtf, $"{DataFormats.Rtf} Кирилица", System.Text.Encoding.UTF8));
        }
        [Test]
        public void DataFormats_Rtf_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new[] { DataFormats.Rtf }, (f) => new object()));
        }

        [Test]
        public void DataFormats_Locale_Test() {
            var clipboardData = testable.SerializeDataObjects(new[] { DataFormats.Locale }, (f) => {
                if(f == DataFormats.Locale) return new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 }); else return new object();
            });

            Assert.That(clipboardData.Formats.Keys, Is.EquivalentTo(new[] { DataFormats.Locale }));
            Assert.That(clipboardData.Formats.Values, Is.EquivalentTo(new[] { new byte[] { 0x00, 0x01, 0x02, 0x03 } }));
        }
        [Test]
        public void DataFormats_Locale_When_NoStringData_Test() {
            Assert.Throws<InvalidCastException>(() => testable.SerializeDataObjects(new[] { DataFormats.Locale }, (f) => new object()));
        }
    }
}