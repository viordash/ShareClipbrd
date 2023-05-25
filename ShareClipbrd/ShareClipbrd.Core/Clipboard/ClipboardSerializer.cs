using System.Collections.Specialized;
using System.Diagnostics;

namespace ShareClipbrd.Core.Clipboard {
    public interface IClipboardSerializer {

        void SerializeDataObjects(ClipboardData clipboardData, string[] formats, Func<string, object> getDataFunc);
        void SerializeFiles(ClipboardData clipboardData, StringCollection files);

        object DeserializeDataObject(string format, Stream dataStream);
    }

    public class ClipboardSerializer : IClipboardSerializer {

        public void SerializeDataObjects(ClipboardData clipboardData, string[] formats, Func<string, object> getDataFunc) {
            Debug.WriteLine(string.Join(", ", formats));

            foreach(var format in formats) {
                try {
                    if(!ClipboardData.Converters.TryGetValue(format, out ClipboardData.Convert? convertFunc)) {

                        var obj = getDataFunc(format);
                        if(obj is MemoryStream memoryStream) {
                            convertFunc = new ClipboardData.Convert(
                            (c, f) => {
                                c.Add(format, memoryStream); return true;
                            },
                            (stream) => stream
                            );

                        } else {
                            Debug.WriteLine($"not supported format: {format}");
                            continue;
                        }
                    }

                    if(!convertFunc.From(clipboardData, getDataFunc)) {
                        throw new InvalidCastException(format);
                    }
                } catch(System.Runtime.InteropServices.COMException e) {
                    Debug.WriteLine(e);
                }
            }
        }

        public void SerializeFiles(ClipboardData clipboardData, StringCollection files) {
            clipboardData.Add(ClipboardData.Format.ZipArchive, files);
        }


        public object DeserializeDataObject(string format, Stream dataStream) {
            if(!ClipboardData.Converters.TryGetValue(format, out ClipboardData.Convert? convertFunc)) {
                convertFunc = new ClipboardData.Convert((c, o) => false, (stream) => {
                    Debug.WriteLine($"--- >>>>");
                    if(stream is MemoryStream ms) {
                        var str = System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());
                        Debug.WriteLine($"--- {format} {str}");
                    }
                    Debug.WriteLine($"--- <<<<");

                    return stream;
                });
            }
            return convertFunc.To(dataStream);
        }
    }
}
