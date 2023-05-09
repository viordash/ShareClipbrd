using System;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Windows;
using ShareClipbrd.Core;

namespace UsAcRe.Core.Extensions {
    public static class ClipboardExtension {

        public static ClipboardData ToDto(this IDataObject dataObject) {
            var clipboardData = new ClipboardData();
            var formats = dataObject.GetFormats();
            foreach(var format in formats) {
                var obj = dataObject.GetData(format);

                if(format == DataFormats.Text && obj is string objText) {
                    clipboardData.Add(format, System.Text.Encoding.ASCII.GetBytes(objText));
                    continue;
                }
                if(format == DataFormats.UnicodeText && obj is string objUnicodeText) {
                    clipboardData.Add(format, System.Text.Encoding.Unicode.GetBytes(objUnicodeText));
                    continue;
                }
                if(format == DataFormats.StringFormat && obj is string objStringFormat) {
                    clipboardData.Add(format, System.Text.Encoding.ASCII.GetBytes(objStringFormat));
                    continue;
                }
                if(format == DataFormats.OemText && obj is string objOemText) {
                    clipboardData.Add(format, System.Text.Encoding.ASCII.GetBytes(objOemText));
                    continue;
                }
                if(format == DataFormats.Rtf && obj is string objRtf) {
                    clipboardData.Add(format, System.Text.Encoding.Unicode.GetBytes(objRtf));
                    continue;
                }
                if(format == DataFormats.Locale && obj is MemoryStream objLocale) {
                    clipboardData.Add(format, objLocale.ToArray());
                    continue;
                }
            }

            return clipboardData;
        }

    }
}
