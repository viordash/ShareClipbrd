using System.Collections.Specialized;

namespace ShareClipbrd.Core.Clipboard {
    public interface IClipboardSerializer {
        ClipboardData SerializeDataObjects(string[] formats, Func<string, object> getDataFunc);
        ClipboardData SerializeFiles(StringCollection files);

        object DeserializeDataObject(string format, byte[] data);
    }
}
