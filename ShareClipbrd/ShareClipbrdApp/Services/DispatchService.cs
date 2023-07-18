using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Microsoft.CodeAnalysis;
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

        static async Task<IStorageItem?> TryCreateBclStorageItem(IStorageProvider storageProvider, Uri uri) {
            var directory = await storageProvider.TryGetFolderFromPathAsync(uri);
            if(directory != null) {
                return directory;
            }

            var file = await storageProvider.TryGetFileFromPathAsync(uri);
            if(file != null) {
                return file;
            }
            return null;
        }

        public async void ReceiveFiles(IList<string> files) {
            await Dispatcher.UIThread.InvokeAsync(new Action(async () => {
                if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                    var dataObject = new DataObject();
                    ClipboardFile.SetFileDropList(async (f, o) => {
                        if(f == ClipboardFile.Format.FileNames) {
                            var storageProvider = TopLevel.GetTopLevel(desktop.MainWindow)!.StorageProvider;

                            var storageItemsTasks = files
                                .Select(x => new Uri(x))
                                .Select(x => TryCreateBclStorageItem(storageProvider, x));

                            var storageItems = (await Task.WhenAll(storageItemsTasks))
                                    .Where(x => x is not null);

                            dataObject.Set(DataFormats.Files, storageItems);
                        } else {
                            dataObject.Set(f, o);
                        }
                    }, files);

                    var clipboard = TopLevel.GetTopLevel(desktop.MainWindow)!.Clipboard!;
                    await clipboard.ClearAsync();
                    await clipboard.SetDataObjectAsync(dataObject);
                }
            }));
        }

    }
}
