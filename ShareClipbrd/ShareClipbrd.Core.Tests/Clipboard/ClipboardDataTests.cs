using System.Text;
using ShareClipbrd.Core.Clipboard;

namespace ShareClipbrd.Core.Tests.Clipboard {
    public class ClipboardDataTests {

        ClipboardData testable;

        [SetUp]
        public void Setup() {
            testable = new();
        }

        [TearDown]
        public void Teardown() {
        }

        async Task<bool> DataFormats_Test(string dataFormat, object data, Encoding encoding) {
            await testable.Serialize(new[] { dataFormat }, (f) => { if(f == dataFormat) return Task.FromResult(data); else return Task.FromResult(new object()); });
            Assert.That(testable.Formats.Select(x => x.Format), Is.EquivalentTo(new[] { dataFormat }));
            Assert.That(testable.Formats.Select(x => x.Stream), Is.EquivalentTo(new[] { new MemoryStream(encoding.GetBytes($"{dataFormat} ��������")) }));
            return true;
        }

        [Test]
        public async Task DataFormats_Text_Test() {
            Assert.That(await DataFormats_Test(ClipboardData.Format.Text, $"{ClipboardData.Format.Text} ��������", System.Text.Encoding.UTF8));
        }
        [Test]
        public void DataFormats_Text_When_NoStringData_Test() {
            Assert.ThrowsAsync<InvalidDataException>(() => testable.Serialize(new[] { ClipboardData.Format.Text }, (f) => Task.FromResult(new object())));
        }

        [Test]
        public async Task DataFormats_UnicodeText_Test() {
            Assert.That(await DataFormats_Test(ClipboardData.Format.UnicodeText, $"{ClipboardData.Format.UnicodeText} ��������", System.Text.Encoding.Unicode));
        }
        [Test]
        public void DataFormats_UnicodeText_When_NoStringData_Test() {
            Assert.ThrowsAsync<InvalidDataException>(() => testable.Serialize(new[] { ClipboardData.Format.UnicodeText }, (f) => Task.FromResult(new object())));
        }

        [Test]
        public async Task DataFormats_StringFormat_Test() {
            Assert.That(await DataFormats_Test(ClipboardData.Format.StringFormat, $"{ClipboardData.Format.StringFormat} ��������", System.Text.Encoding.UTF8));
        }
        [Test]
        public void DataFormats_StringFormat_When_NoStringData_Test() {
            Assert.ThrowsAsync<InvalidDataException>(() => testable.Serialize(new[] { ClipboardData.Format.StringFormat }, (f) => Task.FromResult(new object())));
        }

        [Test]
        public async Task DataFormats_OemText_Test() {
            Assert.That(await DataFormats_Test(ClipboardData.Format.OemText, $"{ClipboardData.Format.OemText} ��������", System.Text.Encoding.ASCII));
        }
        [Test]
        public void DataFormats_OemText_When_NoStringData_Test() {
            Assert.ThrowsAsync<InvalidDataException>(() => testable.Serialize(new[] { ClipboardData.Format.OemText }, (f) => Task.FromResult(new object())));
        }

        [Test]
        public async Task DataFormats_Rtf_Test() {
            Assert.That(await DataFormats_Test(ClipboardData.Format.Rtf, $"{ClipboardData.Format.Rtf} ��������", System.Text.Encoding.UTF8));
        }
        [Test]
        public void DataFormats_Rtf_When_NoStringData_Test() {
            Assert.ThrowsAsync<InvalidDataException>(() => testable.Serialize(new[] { ClipboardData.Format.Rtf }, (f) => Task.FromResult(new object())));
        }

        [Test]
        public async Task DataFormats_Locale_Test() {
            await testable.Serialize(new[] { ClipboardData.Format.Locale }, (f) => {
                if(f == ClipboardData.Format.Locale) return Task.FromResult(new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 }) as object); else return Task.FromResult(new object());
            });

            Assert.That(testable.Formats.Select(x => x.Format), Is.EquivalentTo(new[] { ClipboardData.Format.Locale }));
            Assert.That(testable.Formats.Select(x => x.Stream), Is.EquivalentTo(new[] { new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 }) }));
        }
        [Test]
        public void DataFormats_Locale_When_NoStringData_Test() {
            Assert.ThrowsAsync<InvalidDataException>(() => testable.Serialize(new[] { ClipboardData.Format.Locale }, (f) => Task.FromResult(new object())));
        }
    }
}
