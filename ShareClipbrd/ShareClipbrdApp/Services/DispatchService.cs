using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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

                    if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                        var clipboard = TopLevel.GetTopLevel(desktop.MainWindow)!.Clipboard!;
                        clipboard.ClearAsync();
                        clipboard.SetDataObjectAsync(dataObject);
                    }

                }
            }));
        }

        public async void ReceiveFiles(IList<string> files) {
            await Dispatcher.UIThread.InvokeAsync(new Action(async () => {
                var dataObject = new DataObject();
                ClipboardFile.SetFileDropList((f, o) => {
                    if(f == ClipboardFile.Format.FileNames) {
                        dataObject.Set(DataFormats.Files, o);
                    } else {
                        dataObject.Set(f, o);
                    }
                }, files);

                if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                    var clipboard = TopLevel.GetTopLevel(desktop.MainWindow)!.Clipboard!;
                    await clipboard.ClearAsync();
                    await clipboard.SetDataObjectAsync(dataObject);
                }
            }));
        }
    }
}
