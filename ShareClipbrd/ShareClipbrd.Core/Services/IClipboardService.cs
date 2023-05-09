using System.Collections.Specialized;

namespace ShareClipbrd.Core.Services {
    public interface IClipboardService {
        ClipboardData GetSerializedDataObjects(string[] formats, Func<string, object> getDataFunc);
        ClipboardData GetSerializedFiles(StringCollection files);
    }
}
