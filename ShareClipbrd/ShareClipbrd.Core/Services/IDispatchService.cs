using System.Collections.Specialized;
using ShareClipbrd.Core.Clipboard;

namespace ShareClipbrd.Core.Services {
    public interface IDispatchService {
        void Progress(int percent);
        void ReceiveData(ClipboardData clipboardData);
        void ReceiveFiles(StringCollection files);
    }
}
