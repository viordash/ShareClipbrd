using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

namespace ShareClipbrd.Core.Clipboard {
    public interface IClipboardSerializer {
        bool FormatForFiles(string format);
        bool FormatForImage(string format);
        bool FormatForAudio(string format);


        ClipboardData SerializeDataObjects(string[] formats, Func<string, object> getDataFunc);
        ClipboardData SerializeFiles(StringCollection files);

        object DeserializeDataObject(string format, Stream dataStream);
    }

    public class ClipboardSerializer : IClipboardSerializer {
        public bool FormatForFiles(string format) {
            return format == ClipboardData.Format.FileDrop;
        }

        public bool FormatForImage(string format) {
            return format == ClipboardData.Format.Bitmap;
        }

        public bool FormatForAudio(string format) {
            return format == ClipboardData.Format.WaveAudio;
        }

        public ClipboardData SerializeDataObjects(string[] formats, Func<string, object> getDataFunc) {
            var clipboardData = new ClipboardData();

            Debug.WriteLine(string.Join(", ", formats));

            foreach(var format in formats) {
                try {
                    var obj = getDataFunc(format);

                    if(!ClipboardData.Converters.TryGetValue(format, out ClipboardData.Convert? convertFunc)) {

                        if(obj is MemoryStream memoryStream) {
                            convertFunc = new ClipboardData.Convert(
                            (c, o) => {
                                if(o is MemoryStream castedValue) { c.Add(format, castedValue); return true; } else { return false; }
                            },
                            (stream) => stream
                            );

                        } else {
                            Debug.WriteLine($"not supported format: {format}");
                            continue;
                        }
                    }

                    if(!convertFunc.From(clipboardData, obj)) {
                        throw new InvalidCastException(format);
                    }
                } catch(System.Runtime.InteropServices.COMException e) {
                    Debug.WriteLine(e);
                }
            }
            return clipboardData;
        }

        public ClipboardData SerializeFiles(StringCollection files) {
            var clipboardData = new ClipboardData();

            foreach(var file in files.OfType<string>()) {
                if(File.GetAttributes(file).HasFlag(FileAttributes.Directory)) {
                    clipboardData.Add(ClipboardData.Format.DirectoryDrop, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(file)));
                } else {
                    clipboardData.Add(ClipboardData.Format.FileDrop, new FileStream(file, FileMode.Open, FileAccess.Read));
                }
            }

            return clipboardData;
        }


        public object DeserializeDataObject(string format, Stream dataStream) {
            if(!ClipboardData.Converters.TryGetValue(format, out ClipboardData.Convert? convertFunc)) {
                convertFunc = new ClipboardData.Convert((c, o) => false, (stream) => stream);
            }
            return convertFunc.To(dataStream);
        }
    }
}
