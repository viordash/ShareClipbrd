using System.Diagnostics;
using Avalonia;
using Avalonia.X11;
using Clipboard.Core;

namespace Clipboard.OS {
    internal class Clipboard : IClipboard {
        readonly X11Clipboard clipboard;

        public Clipboard(object? parent) {
            // var platform = new AvaloniaX11Platform(new X11PlatformOptions());
            clipboard = new X11Clipboard();
            // var cts = new CancellationTokenSource();
            // var platformThreading = new X11PlatformThreading(platform);
            // Task.Run(() => platformThreading.RunLoop(cts.Token));
        }

        public Task Clear() {
            throw new NotImplementedException();
        }

        public async Task<bool> ContainsFileDropList() {
            var formats = await clipboard.GetFormatsAsync();

            Debug.WriteLine($"----------- ContainsFileDropList 0 {formats?.Length}");
            return formats?.Any(x => ClipboardFile.ContainsFileDropList(x)) ?? false;
        }

        public Task<bool> ContainsImage() {
            return Task.FromResult(false);
        }

        public Task<object?> GetData(string format) {
            return Task.FromResult<object?>(clipboard.GetDataAsync(format));
        }

        public Task<string[]> GetFormats() {
            // var formats = await clipboard.GetFormatsAsync();
            return clipboard.GetFormatsAsync();
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
