using System.Collections.Specialized;

namespace ShareClipbrd.Core.Services {
    public interface IClipboardService {
        ClipboardData SerializeDataObjects(string[] formats, Func<string, object> getDataFunc);
        ClipboardData SerializeFiles(StringCollection files);

        bool SupportedDataSize(Int32 size);

        void SetClipboardData(ClipboardData clipboardData);
    }
}
