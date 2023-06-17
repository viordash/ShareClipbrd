using System.Collections.Specialized;
using System.Diagnostics;

namespace ShareClipbrd.Core.Clipboard {
    public class ClipboardFile {
        public class Convert {
            public Func<StringCollection, Func<string, Task<object>>, Task<bool>> From { get; set; }
            public Convert(Func<StringCollection, Func<string, Task<object>>, Task<bool>> from) {
                From = from;
            }
        }

        public static class Format {
            public const string FileDrop = "FileDrop";
            public const string FileNames = "FileNames";
            public const string XMateFileNames = "x-special/mate-copied-files";
            public const string XKdeFileNames = "x-special/KDE-copied-files";
            public const string XGnomeFileNames = "x-special/gnome-copied-files";
        }

        public static readonly Dictionary<string, Convert> Converters = new(){
            { Format.FileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.FileNames);
                    if(data is IList<string> files) {c.AddRange(files.ToArray()); return true; }
                    if (data is string castedValue) {c.Add(castedValue); return true; }
                    if (data is byte[] bytes) {System.Text.Encoding.UTF8.GetString(bytes); return true; }
                    return false;
                })
                },

            { Format.XMateFileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.XMateFileNames);
                    if (data is string castedValue) {c.Add(castedValue); return true; }
                    if (data is byte[] bytes) {System.Text.Encoding.UTF8.GetString(bytes); return true; }
                    return false;
                })
                },

            { Format.XKdeFileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.XMateFileNames);
                    if (data is string castedValue) {c.Add(castedValue); return true; }
                    if (data is byte[] bytes) {System.Text.Encoding.UTF8.GetString(bytes); return true; }
                    return false;
                })
                },

            { Format.XGnomeFileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.XMateFileNames);
                    if (data is string castedValue) {c.Add(castedValue); return true; }
                    if (data is byte[] bytes) {System.Text.Encoding.UTF8.GetString(bytes); return true; }
                    return false;
                })
                },
        };

        public static async Task<StringCollection> GetFileDropList(string[] formats, Func<string, Task<object>> getDataFunc) {
            Debug.WriteLine(string.Join(", ", formats));

            var fileDropList = new StringCollection();
            foreach(var format in formats) {
                if(!Converters.TryGetValue(format, out Convert? convertFunc)) {
                    Debug.WriteLine($"not supported format: {format}");
                    // var data = await getDataFunc(format);
                    // if(data is string castedValue) {
                    //     Debug.WriteLine($"      val: {castedValue}");
                    // }
                    continue;
                }

                if(!await convertFunc.From(fileDropList, getDataFunc)) {
                    throw new InvalidDataException(format);
                }
                break;
            }
            return fileDropList;
        }
    }
}
