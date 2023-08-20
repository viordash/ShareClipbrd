using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;
using Clipboard;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Services {
    public class DispatchService : IDispatchService {
        public async void ReceiveData(ClipboardData clipboardData) {
            // await Dispatcher.UIThread.InvokeAsync(new Action(async () => {
            //     var dataObject = new DataObject();
            //     clipboardData.Deserialize((f, o) => dataObject.Set(f, o));
            //     if(dataObject.GetDataFormats().Any()) {
            //         Debug.WriteLine($"   *** formats: {string.Join(", ", dataObject.GetDataFormats())}");

            //         if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            //             var clipboard = ClipboardProvider.Get(desktop.MainWindow);
            //             await clipboard.Clear();
            //             await Task.Delay(100);
            //             await clipboard.SetDataObject(dataObject);
            //         }

            //     }
            // }));
        }

        public async void ReceiveFiles(IList<string> files) {
            await Dispatcher.UIThread.InvokeAsync(new Action(async () => {
                if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                    var clipboard = ClipboardProvider.Get(desktop.MainWindow);
                    await clipboard.Clear();
                    await Task.Delay(100);
                    await clipboard.SetFileDropList(files);
                }
            }));
        }

    }
}
