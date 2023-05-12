using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using ShareClipbrd.Core;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Win.Services {

    public class ClipboardService : IClipboardService {
        static Dictionary<string, Func<ClipboardData, object, bool>> converters = new(){
            { DataFormats.Text, (c,o) => {
                if (o is string castedValue) {c.Add(DataFormats.Text, System.Text.Encoding.ASCII.GetBytes(castedValue)); return true; }
                else {return false;}
            } },
            { DataFormats.UnicodeText, (c,o) => {
                if (o is string castedValue) {c.Add(DataFormats.UnicodeText, System.Text.Encoding.Unicode.GetBytes(castedValue)); return true; }
                else {return false;}
            } },
            { DataFormats.StringFormat, (c,o) => {
                if (o is string castedValue) {c.Add(DataFormats.StringFormat, System.Text.Encoding.ASCII.GetBytes(castedValue)); return true; }
                else {return false;}
            } },
            { DataFormats.OemText, (c,o) => {
                if (o is string castedValue) {c.Add(DataFormats.OemText, System.Text.Encoding.ASCII.GetBytes(castedValue)); return true; }
                else {return false;}
            } },
            { DataFormats.Rtf, (c,o) => {
                if (o is string castedValue) {c.Add(DataFormats.Rtf, System.Text.Encoding.UTF8.GetBytes(castedValue)); return true; }
                else {return false;}
            } },
            { DataFormats.Locale, (c,o) => {
                if (o is MemoryStream castedValue) {c.Add(DataFormats.Locale, castedValue.ToArray()); return true; }
                else {return false;}
            } },
        };

        public bool SupportedFormat(string format) {
            return converters.Keys.Concat(new string[] { DataFormats.FileDrop, DataFormats.Bitmap, DataFormats.WaveAudio }).Contains(format);
        }

        public bool SupportedDataSize(Int32 size) {
            return size > 0 && size < 2_000_000_000;
        }

        public ClipboardData GetSerializedDataObjects(string[] formats, Func<string, object> getDataFunc) {
            var clipboardData = new ClipboardData();

            Debug.WriteLine(string.Join(", ", formats));

            foreach(var format in formats) {
                try {
                    var obj = getDataFunc(format);

                    if(!converters.TryGetValue(format, out Func<ClipboardData, object, bool>? convertFunc)) {
                        throw new NotSupportedException(format);
                    }

                    if(!convertFunc(clipboardData, obj)) {
                        throw new InvalidCastException(format);
                    }
                } catch(System.Runtime.InteropServices.COMException e) {
                    Debug.WriteLine(e);
                }
            }
            return clipboardData;
        }

        public ClipboardData GetSerializedFiles(StringCollection files) {
            var clipboardData = new ClipboardData();

            foreach(var file in files) {
                //clipboardData.Add(DataFormats.FileDrop, castedValue.ToArray());
            }

            return clipboardData;
        }

        public void SetClipboardData(ClipboardData data) {
            //throw new NotImplementedException();
        }
    }
}
