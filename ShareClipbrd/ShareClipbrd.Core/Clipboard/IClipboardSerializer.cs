using System.Collections.Specialized;

namespace ShareClipbrd.Core.Clipboard {
    public interface IClipboardSerializer {
        bool FormatForFiles(string format);
        bool FormatForImage(string format);
        bool FormatForAudio(string format);


        ClipboardData SerializeDataObjects(string[] formats, Func<string, object> getDataFunc);
        ClipboardData SerializeFiles(StringCollection files);

        object DeserializeDataObject(string format, Stream dataStream);

    }
}
