using System.Text;
using Clipboard.Core.Helpers;

namespace Clipboard.Core.Tests.Helpers {
    public class WindowsHtmlFormatHelperTests {

        [Test]
        public void ExtractHtmlFragment_ValidFormat_ReturnsFragment() {
            var fullHtml = "Version:0.9\r\nStartHTML:0000000105\r\nEndHTML:0000000182\r\nStartFragment:0000000139\r\nEndFragment:0000000148\r\n<html><body>\r\n<!--StartFragment-->test text<!--EndFragment-->\r\n</body></html>";
            var bytes = Encoding.UTF8.GetBytes(fullHtml);

            var result = WindowsHtmlFormatHelper.ExtractHtmlFragment(bytes);

            var resultText = Encoding.UTF8.GetString(result);
            Assert.That(resultText, Is.EqualTo("test text"));
        }

        [Test]
        public void ExtractHtmlFragment_CyrillicContent_ReturnsFragment() {
            var fullHtml = "Version:0.9\r\nStartHTML:0000000136\r\nEndHTML:0000000266\r\nStartFragment:0000000170\r\nEndFragment:0000000232\r\nSourceURL:https://example.com\r\n<html><body>\r\n<!--StartFragment-->Процесс, исход которого полностью<!--EndFragment-->\r\n</body></html>";
            var bytes = Encoding.UTF8.GetBytes(fullHtml);

            var result = WindowsHtmlFormatHelper.ExtractHtmlFragment(bytes);

            var resultText = Encoding.UTF8.GetString(result);
            Assert.That(resultText, Is.EqualTo("Процесс, исход которого полностью"));
        }

        [Test]
        public void ExtractHtmlFragment_NullInput_ReturnsEmptyArray() {
            var result = WindowsHtmlFormatHelper.ExtractHtmlFragment(null!);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ExtractHtmlFragment_EmptyInput_ReturnsEmptyArray() {
            var result = WindowsHtmlFormatHelper.ExtractHtmlFragment([]);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ExtractHtmlFragment_NoFragmentMarkers_ReturnsOriginal() {
            var html = "<html><body>test</body></html>";
            var bytes = Encoding.UTF8.GetBytes(html);

            var result = WindowsHtmlFormatHelper.ExtractHtmlFragment(bytes);

            Assert.That(result, Is.EqualTo(bytes));
        }

        [Test]
        public void ExtractHtmlFragment_InvalidOffsets_ReturnsOriginal() {
            var windowsHtml = """
                Version:0.9
                StartFragment:9999999999
                EndFragment:0000000010
                <html><body>test</body></html>
                """;
            var bytes = Encoding.UTF8.GetBytes(windowsHtml);

            var result = WindowsHtmlFormatHelper.ExtractHtmlFragment(bytes);

            Assert.That(result, Is.EqualTo(bytes));
        }
    }
}
