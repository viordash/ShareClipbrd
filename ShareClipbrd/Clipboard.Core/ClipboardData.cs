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
            public const string Text_win = "Text";
            public const string Text_x11 = "TEXT";
            public const string UnicodeText = "UnicodeText";
            public const string Utf8String = "UTF8_STRING";
            public const string OemText_win = "OEMText";
            public const string OemText_x11 = "OEMTEXT";
            public const string String_x11 = "STRING";
            public const string Rtf = "Rich Text Format";
            public const string Locale = "Locale";
            public const string Html_win = "HTML Format";
            public const string Html_x11 = "text/html";
            public const string GtkRichText = "application/x-gtk-text-buffer-rich-text";
            public const string GtkTextBufferContents = "GTK_TEXT_BUFFER_CONTENTS";
            public const string TexPlain = "text/plain";
            public const string TexPlainUtf8 = "text/plain;charset=utf-8";


            public const string WaveAudio = "WaveAudio";

            public const string Dib = "DeviceIndependentBitmap";

            public const string ImageBmp = "image/bmp";
        }

        public static readonly Dictionary<string, Convert> Converters = new(){
            { Format.Text_win, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.Text_win);
                    if (data is string castedValue) {c.Add(Format.Text_win, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.Text_win, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => {
                    if(OperatingSystem.IsWindows()) {
                        return stream switch {
                            MemoryStream memoryStream => System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray()),
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
                        return Format.Text_win;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.Text_x11;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},

            { Format.Text_x11, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.Text_x11);
                    if (data is string castedValue) {c.Add(Format.Text_x11, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.Text_x11, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => {
                    if(OperatingSystem.IsWindows()) {
                        return stream switch {
                            MemoryStream memoryStream => System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray()),
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
                        return Format.Text_win;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.Text_x11;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},

            { Format.UnicodeText, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.UnicodeText);
                    if (data is string castedValue) {c.Add(Format.UnicodeText, new MemoryStream(System.Text.Encoding.Unicode.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.UnicodeText, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => {
                    if(OperatingSystem.IsWindows()) {
                        return stream switch {
                            MemoryStream memoryStream => System.Text.Encoding.Unicode.GetString(memoryStream.ToArray()),
                            _ => throw new ArgumentException(nameof(stream))
                        };
                    }

                    if(OperatingSystem.IsLinux()) {
                        return stream switch {
                            MemoryStream memoryStream => System.Text.Encoding.Unicode.GetString(memoryStream.ToArray()),
                            _ => throw new ArgumentException(nameof(stream))
                        };
                    }

                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                },
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return Format.UnicodeText;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.Utf8String;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},


            { Format.Utf8String, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.Utf8String);
                    if (data is string castedValue) {c.Add(Format.Utf8String, new MemoryStream(System.Text.Encoding.Unicode.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.Utf8String, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => {
                    if(OperatingSystem.IsWindows()) {
                        return stream switch {
                            MemoryStream memoryStream => System.Text.Encoding.Unicode.GetString(memoryStream.ToArray()),
                            _ => throw new ArgumentException(nameof(stream))
                        };
                    }

                    if(OperatingSystem.IsLinux()) {
                        return stream switch {
                            MemoryStream memoryStream => System.Text.Encoding.Unicode.GetString(memoryStream.ToArray()),
                            _ => throw new ArgumentException(nameof(stream))
                        };
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                },
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return Format.UnicodeText;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.Utf8String;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},

            { Format.OemText_win, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.OemText_win);
                    if (data is string castedValue) {c.Add(Format.OemText_win, new MemoryStream(System.Text.Encoding.ASCII.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.OemText_win, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => System.Text.Encoding.ASCII.GetString(((MemoryStream)stream).ToArray()),
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return Format.OemText_win;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.OemText_x11;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},

            { Format.OemText_x11, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.OemText_x11);
                    if (data is string castedValue) {c.Add(Format.OemText_x11, new MemoryStream(System.Text.Encoding.ASCII.GetBytes(castedValue))); return true; }
                    if (data is byte[] bytes) {c.Add(Format.OemText_x11, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => System.Text.Encoding.ASCII.GetString(((MemoryStream)stream).ToArray()),
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return Format.OemText_win;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.OemText_x11;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},

            { Format.String_x11, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.String_x11);
                    if (data is string castedValue) {c.Add(Format.String_x11, new MemoryStream(System.Text.Encoding.ASCII.GetBytes(castedValue))); return true; }
                    return false;
                },
                (stream) => System.Text.Encoding.ASCII.GetString(((MemoryStream)stream).ToArray()),
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return string.Empty;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.String_x11;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
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
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return Format.Locale;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return string.Empty;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},
            { Format.Html_win, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.Html_win);
                    if (data is string castedValue) {c.Add(Format.Html_win, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    return false;
                },
                (stream) => {
                    if(OperatingSystem.IsWindows()) {
                        return stream switch {
                            MemoryStream memoryStream => System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()),
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
                        return Format.Html_win;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.Html_x11;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},
            { Format.Html_x11, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.Html_x11);
                    if (data is byte[] bytes) {c.Add(Format.Html_x11, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => {
                    if(OperatingSystem.IsWindows()) {
                        return stream switch {
                            MemoryStream memoryStream => System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()),
                            _ => throw new ArgumentException(nameof(stream))
                        };
                    }

                    if(OperatingSystem.IsLinux()) {
                        return stream switch {
                            MemoryStream memoryStream => memoryStream,
                            _ => throw new ArgumentException(nameof(stream))
                        };
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                },
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return Format.Html_win;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.Html_x11;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},

            { Format.GtkRichText, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.GtkRichText);
                    if (data is byte[] bytes) {c.Add(Format.GtkRichText, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => stream,
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return string.Empty;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.GtkRichText;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},

            { Format.GtkTextBufferContents, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.GtkTextBufferContents);
                    if (data is byte[] bytes) {c.Add(Format.GtkTextBufferContents, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => stream,
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return string.Empty;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.GtkTextBufferContents;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},

            { Format.TexPlain, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.TexPlain);
                    if (data is byte[] bytes) {c.Add(Format.TexPlain, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => stream,
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return string.Empty;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.TexPlain;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
            )},

            { Format.TexPlainUtf8, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.TexPlainUtf8);
                    if (data is byte[] bytes) {c.Add(Format.TexPlainUtf8, new MemoryStream(bytes)); return true; }
                    return false;
                },
                (stream) => stream,
                () => {
                    if(OperatingSystem.IsWindows()) {
                        return string.Empty;
                    }
                    if(OperatingSystem.IsLinux()) {
                        return Format.TexPlainUtf8;
                    }
                    throw new NotSupportedException($"OS: {Environment.OSVersion}");
                }
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
                            MemoryStream memoryStream => memoryStream,
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
                            MemoryStream memoryStream => new MemoryStream(ImageConverter.FromBmpFileToDibData(memoryStream)),
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
                    if(!Converters.TryGetValue(format, out Convert? convertFunc)) {
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
                if(!Converters.TryGetValue(format.Format, out Convert? convertFunc)) {
                    // convertFunc = new Convert((c, o) => Task.FromResult(false), (stream) => {
                    //     if(stream is MemoryStream ms) {
                    //         var str = System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());
                    //         Debug.WriteLine($"--- {format} {str}");
                    //     }
                    //     return stream;
                    // }, () => format.Format);
                    continue;
                }
                var formatName = convertFunc.GetFormat();
                if(string.IsNullOrEmpty(formatName)) {
                    continue;
                }
                setDataFunc(formatName, convertFunc.To(format.Stream));
            }
        }

        public Int64 GetTotalLenght() {
            return Formats.Sum(x => x.Stream.Length);
        }
    }
}
