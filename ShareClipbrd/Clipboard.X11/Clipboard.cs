using System.Diagnostics;
using Avalonia;
using Avalonia.X11;
using Avalonia.Input;
using Clipboard.Core;

namespace Clipboard.OS {
    internal class Clipboard : IClipboard {

        public Clipboard(object? parent) {

        }

        public async Task Clear() {
            using var clipboard = new X11Clipboard();
            await clipboard.ClearAsync();
        }

        public async Task<bool> ContainsFileDropList() {
            using var clipboard = new X11Clipboard();
            var formats = await clipboard.GetFormatsAsync();

            Debug.WriteLine($"----------- ContainsFileDropList 0 {formats?.Length}");
            return formats?.Any(x => ClipboardFile.ContainsFileDropList(x)) ?? false;
        }

        public Task<bool> ContainsImage() {
            return Task.FromResult(false);
        }

        public async Task<object?> GetData(string format) {
            using var clipboard = new X11Clipboard();
            return await clipboard.GetDataAsync(format);
        }

        public async Task<string[]> GetFormats() {
            using var clipboard = new X11Clipboard();
            return await clipboard.GetFormatsAsync();
        }

        public Task SetAudio(byte[] audioBytes) {
            throw new NotImplementedException();
        }

        public async Task SetDataObject(ClipboardData data) {
            using var clipboard = new X11Clipboard();
            var dataObject = new DataObject();
            data.Deserialize((f, o) => dataObject.Set(f, o));
            await clipboard.SetDataObjectAsync(dataObject);
        }

        public Task SetFileDropList(IList<string> files) {
            throw new NotImplementedException();
        }
    }
}
