using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Win.Services {
    public class DispatchService : IDispatchService {
        public void ReceiveData(ClipboardData clipboardData) {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow!.Dispatcher.BeginInvoke(new Action(async () => {
                await using(ProcessIndicator.Indicate(mainWindow, ProcessIndicator.Mode.Receive)) {
                    var dataObject = new DataObject();
                    clipboardData.Deserialize((f, o) => dataObject.SetData(f, o));
                    if(dataObject.GetFormats().Any()) {
                        Debug.WriteLine($"   *** formats: {string.Join(", ", dataObject.GetFormats())}");
                        System.Windows.Clipboard.Clear();
                        System.Windows.Clipboard.SetDataObject(dataObject);
                    }
                }
            }));
        }

        public void ReceiveFiles(StringCollection files) {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow!.Dispatcher.BeginInvoke(new Action(async () => {
                await using(ProcessIndicator.Indicate(mainWindow, ProcessIndicator.Mode.Receive)) {
                    System.Windows.Clipboard.SetFileDropList(files);
                }
            }));
        }
    }
}
