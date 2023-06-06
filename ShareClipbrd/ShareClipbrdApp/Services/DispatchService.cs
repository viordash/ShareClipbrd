using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Threading;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Services {
    public class DispatchService : IDispatchService {
        public async void ReceiveData(ClipboardData clipboardData) {
            await Dispatcher.UIThread.InvokeAsync(new Action(() => {
                var dataObject = new DataObject();
                clipboardData.Deserialize((f, o) => dataObject.Set(f, o));
                if(dataObject.GetDataFormats().Any()) {
                    Debug.WriteLine($"   *** formats: {string.Join(", ", dataObject.GetDataFormats())}");
                    Application.Current?.Clipboard?.ClearAsync();
                    Application.Current?.Clipboard?.SetDataObjectAsync(dataObject);
                }
            }));
        }

        public async void ReceiveFiles(StringCollection files) {
            await Dispatcher.UIThread.InvokeAsync(new Action(() => {
                //Application.Current.Clipboard.SetFileDropList(files);
            }));
        }
    }
}
