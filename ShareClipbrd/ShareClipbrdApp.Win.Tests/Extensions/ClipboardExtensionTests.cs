using System.DirectoryServices.ActiveDirectory;
using System.Text;
using System.Windows;
using Microsoft.VisualBasic;
using Moq;
using ShareClipbrd.Core;
using UsAcRe.Core.Extensions;

namespace ShareClipbrdApp.Win.Tests.Extensions {
    public class Tests {

        Mock<IDataObject> dataObjectMock;

        [SetUp]
        public void Setup() {
            dataObjectMock = new();
        }

        [TearDown]
        public void Teardown() { }

        bool ToDto_DataFormats_Test(string dataFormat, object data, Encoding encoding) {
            dataObjectMock.Setup(x => x.GetFormats()).Returns(() => new[] { dataFormat });
            dataObjectMock.Setup(x => x.GetData(It.Is<string>(f => f == dataFormat))).Returns(() => data);

            var testable = dataObjectMock.Object.ToDto();
            Assert.That(testable.Formats.Keys, Is.EquivalentTo(new[] { dataFormat }));
            Assert.That(testable.Formats.Values, Is.EquivalentTo(new[] { encoding.GetBytes($"{dataFormat} Кирилица") }));
            return true;
        }


        bool ToDto_DataFormats_When_NoData_Test(string dataFormat) {
            dataObjectMock.Setup(x => x.GetFormats()).Returns(() => new[] { dataFormat });
            dataObjectMock.Setup(x => x.GetData(It.Is<string>(f => f == dataFormat))).Returns(() => new object());

            Assert.Throws<InvalidCastException>(() => dataObjectMock.Object.ToDto());
            return true;
        }

        [Test]
        public void ToDto_DataFormats_Text_Test() {
            Assert.That(ToDto_DataFormats_Test(DataFormats.Text, $"{DataFormats.Text} Кирилица", System.Text.Encoding.ASCII));
        }
        [Test]
        public void ToDto_DataFormats_Text_When_NoStringData_Test() {
            Assert.That(ToDto_DataFormats_When_NoData_Test(DataFormats.Text));
        }

        [Test]
        public void ToDto_DataFormats_UnicodeText_Test() {
            Assert.That(ToDto_DataFormats_Test(DataFormats.UnicodeText, $"{DataFormats.UnicodeText} Кирилица", System.Text.Encoding.Unicode));
        }
        [Test]
        public void ToDto_DataFormats_UnicodeText_When_NoStringData_Test() {
            Assert.That(ToDto_DataFormats_When_NoData_Test(DataFormats.UnicodeText));
        }

        [Test]
        public void ToDto_DataFormats_StringFormat_Test() {
            Assert.That(ToDto_DataFormats_Test(DataFormats.StringFormat, $"{DataFormats.StringFormat} Кирилица", System.Text.Encoding.ASCII));
        }
        [Test]
        public void ToDto_DataFormats_StringFormat_When_NoStringData_Test() {
            Assert.That(ToDto_DataFormats_When_NoData_Test(DataFormats.StringFormat));
        }

        [Test]
        public void ToDto_DataFormats_OemText_Test() {
            Assert.That(ToDto_DataFormats_Test(DataFormats.OemText, $"{DataFormats.OemText} Кирилица", System.Text.Encoding.ASCII));
        }
        [Test]
        public void ToDto_DataFormats_OemText_When_NoStringData_Test() {
            Assert.That(ToDto_DataFormats_When_NoData_Test(DataFormats.OemText));
        }

        [Test]
        public void ToDto_DataFormats_Rtf_Test() {
            Assert.That(ToDto_DataFormats_Test(DataFormats.Rtf, $"{DataFormats.Rtf} Кирилица", System.Text.Encoding.UTF8));
        }
        [Test]
        public void ToDto_DataFormats_Rtf_When_NoStringData_Test() {
            Assert.That(ToDto_DataFormats_When_NoData_Test(DataFormats.Rtf));
        }

        [Test]
        public void ToDto_DataFormats_Locale_Test() {
            dataObjectMock.Setup(x => x.GetFormats()).Returns(() => new[] { DataFormats.Locale });
            dataObjectMock.Setup(x => x.GetData(It.Is<string>(f => f == DataFormats.Locale))).Returns(() => new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 }));

            var testable = dataObjectMock.Object.ToDto();
            Assert.That(testable.Formats.Keys, Is.EquivalentTo(new[] { DataFormats.Locale }));
            Assert.That(testable.Formats.Values, Is.EquivalentTo(new[] { new byte[] { 0x00, 0x01, 0x02, 0x03 } }));
        }
        [Test]
        public void ToDto_DataFormats_Locale_When_NoStringData_Test() {
            Assert.That(ToDto_DataFormats_When_NoData_Test(DataFormats.Locale));
        }
    }
}