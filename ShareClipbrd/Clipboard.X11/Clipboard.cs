using System.Diagnostics;
using Avalonia.X11;
using Avalonia.Input;
using Clipboard.Core;

namespace Clipboard.OS {
    internal class Clipboard : IClipboard {
        readonly X11Clipboard clipboard;

        public Clipboard(object? parent) {
            clipboard = new X11Clipboard();
        }

        public async Task Clear() {
            await clipboard.ClearAsync();
        }

        public async Task<bool> ContainsFileDropList() {
            var formats = await clipboard.GetFormatsAsync();

            Debug.WriteLine($"----------- ContainsFileDropList 0 {formats?.Length}");
            return formats?.Any(x => ClipboardFile.ContainsFileDropList(x)) ?? false;
        }

        public Task<bool> ContainsImage() {
            return Task.FromResult(false);
        }

        public async Task<object?> GetData(string format) {
            return await clipboard.GetDataAsync(format);
        }

        public async Task<string[]> GetFormats() {
            return await clipboard.GetFormatsAsync();
        }

        public Task SetAudio(byte[] audioBytes) {
            throw new NotImplementedException();
        }

        public async Task SetDataObject(ClipboardData data) {
            var dataObject = new DataObject();
            data.Deserialize((f, o) => dataObject.Set(f, o));
            await clipboard.SetDataObjectAsync(dataObject);
        }

        public Task SetFileDropList(IList<string> files) {
            ClipboardFile.SetFileDropList(async (f, o) => {
                var dataObject = new DataObject();
                dataObject.Set(f, o);
                await clipboard.SetDataObjectAsync(dataObject);
            }, files);

            return Task.CompletedTask;
        }
    }
}
