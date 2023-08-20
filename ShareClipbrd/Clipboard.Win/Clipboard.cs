using Clipboard.Core;

namespace Clipboard.OS {
    internal class Clipboard : IClipboard {
        public Task<bool> ContainsFileDropList() {
            return Task.FromResult(System.Windows.Clipboard.ContainsFileDropList());
        }

        public Task<bool> ContainsImage() {
            return Task.FromResult(System.Windows.Clipboard.ContainsImage());
        }

        public Task<object> GetData(string format) {
            return Task.FromResult(System.Windows.Clipboard.GetData(format));
        }

        public Task<string[]> GetFormatsAsync() {
            var dataObject = System.Windows.Clipboard.GetDataObject();
            var formats = dataObject?.GetFormats() ?? Array.Empty<string>();
            return Task.FromResult(formats);
        }
    }
}
