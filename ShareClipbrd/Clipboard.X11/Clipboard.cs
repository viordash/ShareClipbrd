using Clipboard.Core;

namespace Clipboard.OS {
    internal class Clipboard : IClipboard {
        public Task<bool> ContainsFileDropList() {
            return Task.FromResult(false);
        }

        public Task<bool> ContainsImage() {
            return Task.FromResult(false);
        }

        public Task<object?> GetData(string format) {
            return Task.FromResult<object?>(new object());
        }

        public Task<string[]> GetFormats() {
            return Task.FromResult(Array.Empty<string>());
        }
    }
}
