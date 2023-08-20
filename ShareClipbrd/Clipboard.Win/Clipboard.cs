using Clipboard.Core;

namespace Clipboard.OS {
    internal class Clipboard : IClipboard {
        public Task Clear() {
            System.Windows.Clipboard.Clear();
            return Task.CompletedTask;
        }

        public Task<bool> ContainsFileDropList() {
            return Task.FromResult(System.Windows.Clipboard.ContainsFileDropList());
        }

        public Task<bool> ContainsImage() {
            return Task.FromResult(System.Windows.Clipboard.ContainsImage());
        }

        public Task<object?> GetData(string format) {
            return Task.FromResult<object?>(System.Windows.Clipboard.GetData(format));
        }

        public Task<string[]> GetFormats() {
            var dataObject = System.Windows.Clipboard.GetDataObject();
            var formats = dataObject?.GetFormats() ?? Array.Empty<string>();
            return Task.FromResult(formats);
        }

        public Task SetAudio(byte[] audioBytes) {
            System.Windows.Clipboard.SetAudio(audioBytes);
            return Task.CompletedTask;
        }

        public Task SetDataObject(object data) {
            System.Windows.Clipboard.SetDataObject(data);
            return Task.CompletedTask;
        }

        public Task SetFileDropList(IList<string> files) {
            var fileDropList = new System.Collections.Specialized.StringCollection();
            fileDropList.AddRange(files.ToArray());
            System.Windows.Clipboard.SetFileDropList(fileDropList);
            return Task.CompletedTask;
        }
    }
}
