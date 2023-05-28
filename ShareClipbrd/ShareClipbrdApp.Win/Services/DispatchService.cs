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
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                var dataObject = new DataObject();
                clipboardData.Deserialize((f, o) => dataObject.SetData(f, o));
                if(dataObject.GetFormats().Any()) {
                    Debug.WriteLine($"   *** formats: {string.Join(", ", dataObject.GetFormats())}");
                    System.Windows.Clipboard.Clear();
                    System.Windows.Clipboard.SetDataObject(dataObject);
                }
            }));
        }

        public void ReceiveFiles(StringCollection files) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                System.Windows.Clipboard.SetFileDropList(files);
            }));
        }
    }
}
