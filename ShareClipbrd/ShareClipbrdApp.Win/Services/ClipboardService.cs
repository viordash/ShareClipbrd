using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using ShareClipbrd.Core;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Win.Services {

    public class ClipboardService : IClipboardService {
        class ConverterFuncs {
            public Func<ClipboardData, object, bool> From { get; set; }
            public Func<byte[], object> To { get; set; }
            public ConverterFuncs(Func<ClipboardData, object, bool> from, Func<byte[], object> to) {
                From = from;
                To = to;
            }
        }

        static readonly Dictionary<string, ConverterFuncs> converters = new(){
            { DataFormats.Text, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.Text, System.Text.Encoding.ASCII.GetBytes(castedValue)); return true; }
                    else {return false;}
                },
                (b) => System.Text.Encoding.ASCII.GetString(b)
                )
                },
            { DataFormats.UnicodeText, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.UnicodeText, System.Text.Encoding.Unicode.GetBytes(castedValue)); return true; }
                    else {return false;}
                },
                (b) => System.Text.Encoding.Unicode.GetString(b)
                )
            },
            { DataFormats.StringFormat, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.StringFormat, System.Text.Encoding.ASCII.GetBytes(castedValue)); return true; }
                    else {return false;}
                },
                (b) => System.Text.Encoding.ASCII.GetString(b)
                )
            },
            { DataFormats.OemText, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.OemText, System.Text.Encoding.ASCII.GetBytes(castedValue)); return true; }
                    else {return false;}
                },
                (b) => System.Text.Encoding.ASCII.GetString(b)
                )
            },
            { DataFormats.Rtf, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.Rtf, System.Text.Encoding.UTF8.GetBytes(castedValue)); return true; }
                    else {return false;}
                },
                (b) => System.Text.Encoding.ASCII.GetString(b)
                )
            },
            { DataFormats.Locale, new ConverterFuncs(
                (c,o) => {
                    if (o is MemoryStream castedValue) {c.Add(DataFormats.Locale, castedValue.ToArray()); return true; }
                    else {return false;}
                },
                (b) => new MemoryStream(b)
                )
            },
            { DataFormats.Html, new ConverterFuncs(
                (c,o) => {
                    if (o is string castedValue) {c.Add(DataFormats.Html, System.Text.Encoding.UTF8.GetBytes(castedValue)); return true; }
                    else {return false;}
                },
                (b) => System.Text.Encoding.UTF8.GetString(b)
                )
            },
        };

        public bool SupportedFormat(string format) {
            return converters.Keys.Concat(new string[] { DataFormats.FileDrop, DataFormats.Bitmap, DataFormats.WaveAudio }).Contains(format);
        }

        public bool SupportedDataSize(Int32 size) {
            return size > 0 && size < 2_000_000_000;
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
                                if(o is MemoryStream castedValue) { c.Add(format, castedValue.ToArray()); return true; } else { return false; }
                            },
                            (b) => new MemoryStream(b)
                            );
                        } else {
                            throw new NotSupportedException(format);
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

            foreach(var file in files) {
                //clipboardData.Add(DataFormats.FileDrop, castedValue.ToArray());
            }

            return clipboardData;
        }

        public void SetClipboardData(ClipboardData clipboardData) {
            var dispatcher = Application.Current.Dispatcher;

            dispatcher?.BeginInvoke(new Action(() => {
                var dataObject = new DataObject();

                foreach(var format in clipboardData.Formats) {
                    if(!converters.TryGetValue(format.Key, out ConverterFuncs? convertFunc)) {
                        convertFunc = new ConverterFuncs((c, o) => false, (b) => new MemoryStream(b));
                    }
                    dataObject.SetData(format.Key, convertFunc.To(format.Value));
                }
                Debug.WriteLine($"   *** formats: {string.Join(", ", dataObject.GetFormats())}");
                Clipboard.Clear();
                Clipboard.SetDataObject(dataObject);
            }));
        }
    }
}
