using System.Diagnostics;
using Avalonia;
using Avalonia.X11;
using Clipboard.Core;

namespace Clipboard.OS
{
    internal class Clipboard : IClipboard
    {

        public Clipboard(object? parent)
        {

        }

        public Task Clear()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ContainsFileDropList()
        {
            using var clipboard = new X11Clipboard();
            var formats = await clipboard.GetFormatsAsync();

            Debug.WriteLine($"----------- ContainsFileDropList 0 {formats?.Length}");
            return formats?.Any(x => ClipboardFile.ContainsFileDropList(x)) ?? false;
        }

        public Task<bool> ContainsImage()
        {
            return Task.FromResult(false);
        }

        public async Task<object?> GetData(string format)
        {
            using var clipboard = new X11Clipboard();
            return await clipboard.GetDataAsync(format);
        }

        public async Task<string[]> GetFormats()
        {
            using var clipboard = new X11Clipboard();
            return await clipboard.GetFormatsAsync();
        }

        public Task SetAudio(byte[] audioBytes)
        {
            throw new NotImplementedException();
        }

        public Task SetDataObject(ClipboardData data)
        {
            throw new NotImplementedException();
        }

        public Task SetFileDropList(IList<string> files)
        {
            throw new NotImplementedException();
        }
    }
}
