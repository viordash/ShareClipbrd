using System;
using System.Collections.Generic;
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

        public async void ReceiveFiles(IList<string> files) {
            await Dispatcher.UIThread.InvokeAsync(new Action(() => {
                var dataObject = new DataObject();
                ClipboardFile.SetFileDropList((f, o) => dataObject.Set(f, o), files);
                //dataObject.Set(ClipboardFile.Format.FileNames, files);
                Application.Current?.Clipboard?.ClearAsync();
                Application.Current?.Clipboard?.SetDataObjectAsync(dataObject);
            }));
        }
    }
}
