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

            var testable = dataObjectMock.Object.ToDto();
            Assert.That(testable.Formats, Is.Empty);
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
    }
}