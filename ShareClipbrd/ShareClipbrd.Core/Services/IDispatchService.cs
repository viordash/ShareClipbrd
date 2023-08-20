using Clipboard;

namespace ShareClipbrd.Core.Services {
    public interface IDispatchService {
        void ReceiveData(ClipboardData clipboardData);
        void ReceiveFiles(IList<string> files);
    }
}
