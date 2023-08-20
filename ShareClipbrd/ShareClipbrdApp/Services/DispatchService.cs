using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Clipboard;
using Clipboard.Core;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Services {
    public class DispatchService : IDispatchService {
        public async void ReceiveData(ClipboardData clipboardData) {
            await Dispatcher.UIThread.InvokeAsync(new Action(async () => {
                if(clipboardData.Formats.Any()) {
                    Debug.WriteLine($"   *** formats: {string.Join(", ", clipboardData.Formats)}");

                    if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                        var clipboard = ClipboardProvider.Get(desktop.MainWindow);
                        await clipboard.Clear();
                        await Task.Delay(100);
                        await clipboard.SetDataObject(clipboardData);
                    }

                }
            }));
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
