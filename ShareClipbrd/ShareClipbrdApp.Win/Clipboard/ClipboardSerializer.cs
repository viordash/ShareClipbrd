using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using ShareClipbrd.Core.Clipboard;

namespace ShareClipbrdApp.Win.Clipboard {

    public class ClipboardSerializer : IClipboardSerializer {
        class ConverterFuncs {
            public Func<ClipboardData, object, bool> From { get; set; }
            public Func<Stream, object> To { get; set; }
            public ConverterFuncs(Func<ClipboardData, object, bool> from, Func<Stream, object> to) {
                From = from;
                To = to;
            }
        }

        static readonly Dictionary<string, ConverterFuncs> converters = new(){
            { DataFormats.Text, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.Text, new MemoryStream(System.Text.Encoding.ASCII.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.ASCII.GetString(((MemoryStream)stream).ToArray())
                )
                },
            { DataFormats.UnicodeText, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.UnicodeText, new MemoryStream(System.Text.Encoding.Unicode.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.Unicode.GetString(((MemoryStream)stream).ToArray())
                )
            },
            { DataFormats.StringFormat, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.StringFormat, new MemoryStream(System.Text.Encoding.ASCII.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.ASCII.GetString(((MemoryStream)stream).ToArray())
                )
            },
            { DataFormats.OemText, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.OemText, new MemoryStream(System.Text.Encoding.ASCII.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.ASCII.GetString(((MemoryStream)stream).ToArray())
                )
            },
            { DataFormats.Rtf, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.Rtf, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.ASCII.GetString(((MemoryStream) stream).ToArray())
                )
            },
            { DataFormats.Locale, new ConverterFuncs(
                (c,o) => {
                    if (o is MemoryStream castedValue) {c.Add(DataFormats.Locale, castedValue); return true; }
                    else {return false;}
                },
                (stream) => stream
                )
            },
            { DataFormats.Html, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.Html, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(castedValue))); return true; }
                    else {return false;}
                },
                (stream) => System.Text.Encoding.UTF8.GetString(((MemoryStream) stream).ToArray())
                )
            },
        };

        public bool FormatForFiles(string format) {
            return format == DataFormats.FileDrop;
        }

        public bool FormatForImage(string format) {
            return format == DataFormats.Bitmap;
        }

        public bool FormatForAudio(string format) {
            return format == DataFormats.WaveAudio;
        }

        public ClipboardData SerializeDataObjects(string[] formats, Func<string, object> getDataFunc) {
            var clipboardData = new ClipboardData();

            Debug.WriteLine(string.Join(", ", formats));

            foreach(var format in formats) {
                try {
                    var obj = getDataFunc(format);

                    if(!converters.TryGetValue(format, out ConverterFuncs? convertFunc)) {

                        if(obj is MemoryStream memoryStream) {
                            convertFunc = new ConverterFuncs(
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
                clipboardData.Add(DataFormats.FileDrop, new FileStream(file, FileMode.Open, FileAccess.Read));
            }

            return clipboardData;
        }


        public object DeserializeDataObject(string format, Stream dataStream) {
            if(!converters.TryGetValue(format, out ConverterFuncs? convertFunc)) {
                convertFunc = new ConverterFuncs((c, o) => false, (stream) => stream);
            }
            return convertFunc.To(dataStream);
        }
    }
}
