using System.Diagnostics;

namespace Clipboard.Core {
    public class ClipboardData {
        public record Item {
            public string Format { get; set; }
            public MemoryStream Stream { get; set; }
            public Item(string format, MemoryStream stream) {
                Format = format;
                Stream = stream;
            }
        }

        public class Convert {
            public Func<ClipboardData, Func<string, Task<object?>>, Task<bool>> From { get; set; }
            public Func<Stream, object> To { get; set; }
            public Func<string> GetFormat { get; set; }
            public Convert(Func<ClipboardData, Func<string, Task<object?>>, Task<bool>> from, Func<Stream, object> to, Func<string> getFormat) {
                From = from;
                To = to;
                GetFormat = getFormat;
            }
        }

        public static class Format {
            public const string Text = "TEXT";
            public const string UnicodeText = "UNICODETEXT";
            public const string Utf8String = "UTF8_STRING";
            public const string StringFormat = "System.String";
            public const string OemText = "OEMTEXT";
            public const string Rtf = "Rich Text Format";
            public const string Locale = "Locale";
            public const string Html = "HTML Format";
            public const string WaveAudio = "WaveAudio";

            public const string Bitmap = "Bitmap";
            public const string Dib = "Dib";

            public const string ImageBmp = "image/bmp";
        }

        public static readonly Dictionary<string, Convert> Converters = new(){
            { Format.Text, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.Text);
                    if (data is string castedValue) {c.Add(Format.Text, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.Text, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray()),
                () => Format.Text
            )},
            { Format.UnicodeText, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.UnicodeText);
                    if (data is string castedValue) {c.Add(Format.UnicodeText, new MemoryStream(System.Text.Encoding.Unicode.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.UnicodeText, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => System.Text.Encoding.Unicode.GetString(((MemoryStream)stream).ToArray()),
                () => Format.UnicodeText
            )},
            { Format.Utf8String, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.Utf8String);
                    if (data is string castedValue) {c.Add(Format.Utf8String, new MemoryStream(System.Text.Encoding.Unicode.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.Utf8String, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => System.Text.Encoding.Unicode.GetString(((MemoryStream)stream).ToArray()),
                () => Format.Utf8String
            )},
            { Format.StringFormat, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.StringFormat);
                    if (data is string castedValue) {c.Add(Format.StringFormat, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.StringFormat, new MemoryStream(bytes)); return true; }
                    if (data == null) { return true; }
                    return false;
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray()),
                () => Format.StringFormat
            )},
            { Format.OemText, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.OemText);
                    if (data is string castedValue) {c.Add(Format.OemText, new MemoryStream(System.Text.Encoding.ASCII.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.OemText, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => System.Text.Encoding.ASCII.GetString(((MemoryStream)stream).ToArray()),
                () => Format.OemText
            )},
            { Format.Rtf, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.Rtf);
                    if (data is string castedValue) {c.Add(Format.Rtf, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    else if (data is byte[] bytes) {c.Add(Format.Rtf, new MemoryStream(bytes)); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream) stream).ToArray()),
                () => Format.Rtf
            )},
            { Format.Locale, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.Locale);
                    if (data is MemoryStream castedValue) {c.Add(Format.Locale, castedValue); return true; }
                    if (data is byte[] bytes) {c.Add(Format.Locale, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => stream,
                () => Format.Locale
            )},
            { Format.Html, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.Html);
                    if (data is string castedValue) {c.Add(Format.Html, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.Html, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream) stream).ToArray()),
                () => Format.Html
            )},

            { Format.Dib, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.Dib);
                    if (data is MemoryStream castedValue) {c.Add(Format.Dib, castedValue); return true; }
                    if (data is byte[] bytes) {c.Add(Format.Dib, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => {
                    if(OperatingSystem.IsWindows()) {
                        return stream switch {
                            MemoryStream memoryStream => memoryStream.ToArray(),
                            _ => throw new ArgumentException(nameof(stream))
                        };
                    }

                    if(OperatingSystem.IsLinux()) {
                        return stream switch {
                            MemoryStream memoryStream => ImageConverter.FromDibToBmpFileData(memoryStream),
                            _ => throw new ArgumentException(nameof(stream))
                        };
                    }

                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                },
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return Format.Dib;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.ImageBmp;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},

            { Format.ImageBmp, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.ImageBmp);
                    if (data is MemoryStream castedValue) {c.Add(Format.ImageBmp, castedValue); return true; }
                    if (data is byte[] bytes) {c.Add(Format.ImageBmp, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => {
                    if(OperatingSystem.IsWindows()) {
                        return stream switch {
                            MemoryStream memoryStream => ImageConverter.FromBmpFileToDibData(memoryStream),
                            _ => throw new ArgumentException(nameof(stream))
                        };
                    }

                    if(OperatingSystem.IsLinux()) {
                        return stream switch {
                            MemoryStream memoryStream => memoryStream.ToArray(),
                            _ => throw new ArgumentException(nameof(stream))
                        };
                    }

                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                },
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return Format.Dib;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.ImageBmp;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},
        };

        public List<ClipboardData.Item> Formats { get; } = new();

        public void Add(string format, MemoryStream stream) {
            Formats.Add(new ClipboardData.Item(format, stream));
        }

        public async Task Serialize(string[] formats, Func<string, Task<object?>> getDataFunc) {
            Debug.WriteLine(string.Join(", ", formats));

            foreach(var format in formats) {
                try {
                    if(!Converters.TryGetValue(format.ToUpper(), out Convert? convertFunc)) {
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
                if(!Converters.TryGetValue(format.Format.ToUpper(), out Convert? convertFunc)) {
                    convertFunc = new Convert((c, o) => Task.FromResult(false), (stream) => {
                        if(stream is MemoryStream ms) {
                            var str = System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());
                            Debug.WriteLine($"--- {format} {str}");
                        }
                        return stream;
                    }, () => format.Format);
                }
                setDataFunc(convertFunc.GetFormat(), convertFunc.To(format.Stream));
            }
        }

        public Int64 GetTotalLenght() {
            return Formats.Sum(x => x.Stream.Length);
        }
    }
}
