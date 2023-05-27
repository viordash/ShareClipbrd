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
        public class Convert {
            public Func<ClipboardData, Func<string, object>, bool> From { get; set; }
            public Func<Stream, object> To { get; set; }
            public Convert(Func<ClipboardData, Func<string, object>, bool> from, Func<Stream, object> to) {
                From = from;
                To = to;
            }
        }

        public static class Format {
            public const string Text = "Text";
            public const string UnicodeText = "UnicodeText";
            public const string StringFormat = "System.String";
            public const string OemText = "OEMText";
            public const string Rtf = "Rich Text Format";
            public const string Locale = "Locale";
            public const string Html = "HTML Format";
            public const string Bitmap = "Bitmap";
            public const string WaveAudio = "WaveAudio";

            public const string ZipArchive = "ZipArchive_60D54950";
        }

        public static readonly Dictionary<string, Convert> Converters = new(){
            { Format.Text, new Convert(
                (c,getDataFunc) => {
                    if (getDataFunc(Format.Text) is string castedValue) {c.Add(Format.Text, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray())
                )
                },
            { Format.UnicodeText, new Convert(
                (c,getDataFunc) => {
                    if (getDataFunc(Format.UnicodeText) is string castedValue) {c.Add(Format.UnicodeText, new MemoryStream(System.Text.Encoding.Unicode.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.Unicode.GetString(((MemoryStream)stream).ToArray())
                )
            },
            { Format.StringFormat, new Convert(
                (c,getDataFunc) => {
                    if (getDataFunc(Format.StringFormat) is string castedValue) {c.Add(Format.StringFormat, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream)stream).ToArray())
                )
            },
            { Format.OemText, new Convert(
                (c,getDataFunc) => {
                    if (getDataFunc(Format.OemText) is string castedValue) {c.Add(Format.OemText, new MemoryStream(System.Text.Encoding.ASCII.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.ASCII.GetString(((MemoryStream)stream).ToArray())
                )
            },
            { Format.Rtf, new Convert(
                (c,getDataFunc) => {
                    if (getDataFunc(Format.Rtf) is string castedValue) {c.Add(Format.Rtf, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream) stream).ToArray())
                )
            },
            { Format.Locale, new Convert(
                (c,getDataFunc) => {
                    if (getDataFunc(Format.Locale) is MemoryStream castedValue) {c.Add(Format.Locale, castedValue); return true; }
                    else {return false;}
                },
                (stream) => stream
                )
            },
            { Format.Html, new Convert(
                (c,getDataFunc) => {
                    if (getDataFunc(Format.Html) is string castedValue) {c.Add(Format.Html, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream) stream).ToArray())
                )
            },
        };

        public List<ClipboardItem> Formats { get; } = new();

        public void Add(string format, MemoryStream stream) {
            Formats.Add(new ClipboardItem(format, stream));
        }

        public void Serialize(string[] formats, Func<string, object> getDataFunc) {
            Debug.WriteLine(string.Join(", ", formats));

            foreach(var format in formats) {
                try {
                    if(!Converters.TryGetValue(format, out Convert? convertFunc)) {

                        var obj = getDataFunc(format);
                        if(obj is MemoryStream memoryStream) {
                            convertFunc = new Convert(
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

                    if(!convertFunc.From(this, getDataFunc)) {
                        throw new InvalidCastException(format);
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
                    convertFunc = new Convert((c, o) => false, (stream) => {
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
    }
}
