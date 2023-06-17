using System.Collections.Specialized;
using System.Diagnostics;
using ShareClipbrd.Core.Helpers;

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

        static string[] ParseLines(string text) {
            var lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var files = lines
                .Where(x => PathHelper.IsAbsolute(x))
                .ToArray();
            return files;
        }

        static bool TryParse(object data, out string[] files) {
            files = new string[] { };
            files = data switch {
                IList<string> list => list
                                        .Select(x => x.Trim())
                                        .Where(x => PathHelper.IsAbsolute(x))
                                        .ToArray(),
                string lines => ParseLines(lines),
                byte[] bytes => ParseLines(System.Text.Encoding.UTF8.GetString(bytes)),
                _ => new string[] { }
            };

            return files.Any();
        }

        public static readonly Dictionary<string, Convert> Converters = new(){
            { Format.FileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.FileNames);
                    if (TryParse(data, out string[] files)) {
                        c.AddRange(files); return true;
                    }
                    return false;
                })
                },

            { Format.XMateFileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.XMateFileNames);
                    if (TryParse(data, out string[] files)) {
                        c.AddRange(files); return true;
                    }
                    return false;
                })
                },

            { Format.XKdeFileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.XKdeFileNames);
                    if (TryParse(data, out string[] files)) {
                        c.AddRange(files); return true;
                    }
                    return false;
                })
                },

            { Format.XGnomeFileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.XGnomeFileNames);
                    if (TryParse(data, out string[] files)) {
                        c.AddRange(files); return true;
                    }
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
