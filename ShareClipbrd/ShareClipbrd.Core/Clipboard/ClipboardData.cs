using System.Collections.Specialized;
using System.Diagnostics;

namespace ShareClipbrd.Core.Clipboard {
    public record ClipboardItem {
        public string Format { get; set; }
        public MemoryStream Stream { get; set; }
        public ClipboardItem(string format, MemoryStream stream) {
            Format = format;
            Stream = stream;
        }
    }
    public class ClipboardData {
        public class ConvertData {
            public Func<ClipboardData, Func<string, Task<object>>, Task<bool>> From { get; set; }
            public Func<Stream, object> To { get; set; }
            public ConvertData(Func<ClipboardData, Func<string, Task<object>>, Task<bool>> from, Func<Stream, object> to) {
                From = from;
                To = to;
            }
        }

        public class ConvertFilename {
            public Func<StringCollection, Func<string, Task<object>>, Task<bool>> From { get; set; }
            public ConvertFilename(Func<StringCollection, Func<string, Task<object>>, Task<bool>> from) {
                From = from;
            }
        }

        public static class DataFormat {
            public const string Text = "Text";
            public const string UnicodeText = "UnicodeText";
            public const string StringFormat = "System.String";
            public const string OemText = "OEMText";
            public const string Rtf = "Rich Text Format";
            public const string Locale = "Locale";
            public const string Html = "HTML Format";
            public const string Bitmap = "Bitmap";
            public const string WaveAudio = "WaveAudio";
        }

        public static class FilesFormat {
            public const string FileDrop = "FileDrop";
            public const string FileNames = "FileNames";
            public const string XMateFileNames = "x-special/mate-copied-files";
            public const string XKdeFileNames = "x-special/KDE-copied-files";
            public const string XGnomeFileNames = "x-special/gnome-copied-files";
        }

        public static readonly Dictionary<string, ConvertData> DataConverters = new(){
            { DataFormat.Text, new ConvertData(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(DataFormat.Text);
                    if (data is string castedValue) {c.Add(DataFormat.Text, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    else if (data is byte[] bytes) {c.Add(DataFormat.Text, new MemoryStream(bytes)); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray())
                )
                },
            { DataFormat.UnicodeText, new ConvertData(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(DataFormat.UnicodeText);
                    if (data is string castedValue) {c.Add(DataFormat.UnicodeText, new MemoryStream(System.Text.Encoding.Unicode.GetBytes(castedValue))); return true; }
                    else if (data is byte[] bytes) {c.Add(DataFormat.UnicodeText, new MemoryStream(bytes)); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.Unicode.GetString(((MemoryStream)stream).ToArray())
                )
            },
            { DataFormat.StringFormat, new ConvertData(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(DataFormat.StringFormat);
                    if (data is string castedValue) {c.Add(DataFormat.StringFormat, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    else if (data is byte[] bytes) {c.Add(DataFormat.StringFormat, new MemoryStream(bytes)); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray())
                )
            },
            { DataFormat.OemText, new ConvertData(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(DataFormat.OemText);
                    if (data is string castedValue) {c.Add(DataFormat.OemText, new MemoryStream(System.Text.Encoding.ASCII.GetBytes(castedValue))); return true; }
                    else if (data is byte[] bytes) {c.Add(DataFormat.OemText, new MemoryStream(bytes)); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.ASCII.GetString(((MemoryStream)stream).ToArray())
                )
            },
            { DataFormat.Rtf, new ConvertData(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(DataFormat.Rtf);
                    if (data is string castedValue) {c.Add(DataFormat.Rtf, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    else if (data is byte[] bytes) {c.Add(DataFormat.Rtf, new MemoryStream(bytes)); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream) stream).ToArray())
                )
            },
            { DataFormat.Locale, new ConvertData(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(DataFormat.Locale);
                    if (data is MemoryStream castedValue) {c.Add(DataFormat.Locale, castedValue); return true; }
                    else if (data is byte[] bytes) {c.Add(DataFormat.Locale, new MemoryStream(bytes)); return true; }
                    else {return false;}
                },
                (stream) => stream
                )
            },
            { DataFormat.Html, new ConvertData(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(DataFormat.Html);
                    if (data is string castedValue) {c.Add(DataFormat.Html, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    else if (data is byte[] bytes) {c.Add(DataFormat.Html, new MemoryStream(bytes)); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream) stream).ToArray())
                )
            },
        };

        public static readonly Dictionary<string, ConvertFilename> FilesConverters = new(){
            { FilesFormat.XMateFileNames, new ConvertFilename(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(FilesFormat.XMateFileNames);
                    if (data is string castedValue) {c.Add(castedValue); return true; }
                    else if (data is byte[] bytes) {System.Text.Encoding.UTF8.GetString(bytes); return true; }
                    else {return false;}
                })
                },

            { FilesFormat.XKdeFileNames, new ConvertFilename(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(FilesFormat.XMateFileNames);
                    if (data is string castedValue) {c.Add(castedValue); return true; }
                    else if (data is byte[] bytes) {System.Text.Encoding.UTF8.GetString(bytes); return true; }
                    else {return false;}
                })
                },

            { FilesFormat.XGnomeFileNames, new ConvertFilename(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(FilesFormat.XMateFileNames);
                    if (data is string castedValue) {c.Add(castedValue); return true; }
                    else if (data is byte[] bytes) {System.Text.Encoding.UTF8.GetString(bytes); return true; }
                    else {return false;}
                })
                },
        };

        public List<ClipboardItem> Formats { get; } = new();

        public void Add(string format, MemoryStream stream) {
            Formats.Add(new ClipboardItem(format, stream));
        }

        public async Task Serialize(string[] formats, Func<string, Task<object>> getDataFunc) {
            Debug.WriteLine(string.Join(", ", formats));

            foreach(var format in formats) {
                try {
                    if(!DataConverters.TryGetValue(format, out ConvertData? convertFunc)) {
                        Debug.WriteLine($"not supported format: {format}");
                        // var data = await getDataFunc(format);
                        // if(data is string castedValue) {
                        //     Debug.WriteLine($"      val: {castedValue}");
                        // }
                        continue;
                    }

                    if(!await convertFunc.From(this, getDataFunc)) {
                        throw new InvalidDataException(format);
                    }
                } catch(System.Runtime.InteropServices.COMException e) {
                    Debug.WriteLine(e);
                }
            }
        }

        public void Deserialize(Action<string, object> setDataFunc) {
            if(!Formats.Any()) {
                return;
            }

            foreach(var format in Formats) {
                if(!DataConverters.TryGetValue(format.Format, out ConvertData? convertFunc)) {
                    convertFunc = new ConvertData((c, o) => Task.FromResult(false), (stream) => {
                        if(stream is MemoryStream ms) {
                            var str = System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());
                            Debug.WriteLine($"--- {format} {str}");
                        }
                        return stream;
                    });
                }
                setDataFunc(format.Format, convertFunc.To(format.Stream));
            }
        }

        public Int64 GetTotalLenght() {
            return Formats.Sum(x => x.Stream.Length);
        }

        public static bool ContainsFileDropList(string[] formats) {
            return formats.Any(x => x == FilesFormat.FileNames);
        }

        public static async Task<StringCollection> GetFileDropList(Func<string, Task<object>> getDataFunc) {
            var obj = await getDataFunc(FilesFormat.FileNames);
            if(!(obj is IList<string> files)) {
                throw new InvalidDataException(FilesFormat.FileNames);
            }
            var fileDropList = new StringCollection();
            fileDropList.AddRange(files.ToArray());
            return fileDropList;
        }

        public async Task<StringCollection> GetFileDropList(string[] formats, Func<string, Task<object>> getDataFunc) {
            Debug.WriteLine(string.Join(", ", formats));

            var fileDropList = new StringCollection();
            foreach(var format in formats) {
                try {
                    if(!FilesConverters.TryGetValue(format, out ConvertFilename? convertFunc)) {
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
                } catch(System.Runtime.InteropServices.COMException e) {
                    Debug.WriteLine(e);
                }
            }
            return fileDropList;
        }
    }
}
