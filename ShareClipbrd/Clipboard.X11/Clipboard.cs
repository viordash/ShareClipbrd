using Avalonia;
using Avalonia.X11;
using Clipboard.Core;

namespace Clipboard.OS {
    internal class Clipboard : IClipboard {
        public Clipboard(object? parent) {
            var platform = parent as AvaloniaX11Platform;
            AvaloniaX11PlatformExtensions.InitializeX11Platform();
        }

        public Task Clear() {
            throw new NotImplementedException();
        }

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

        public Task SetAudio(byte[] audioBytes) {
            throw new NotImplementedException();
        }

        public Task SetDataObject(ClipboardData data) {
            throw new NotImplementedException();
        }

        public Task SetFileDropList(IList<string> files) {
            throw new NotImplementedException();
        }
    }
}
