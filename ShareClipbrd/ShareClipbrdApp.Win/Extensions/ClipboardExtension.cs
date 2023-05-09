using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using ShareClipbrd.Core;

namespace UsAcRe.Core.Extensions {
    public static class ClipboardExtension {
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

            { "Shell IDList Array", (c,o) => {
                if (o is MemoryStream castedValue) {c.Add("Shell IDList Array", castedValue.ToArray()); return true; }
                else {return false;}
            } },
            { "DataObjectAttributes", (c,o) => {
                if (o is MemoryStream castedValue) {c.Add("DataObjectAttributes", castedValue.ToArray()); return true; }
                else {return false;}
            } },
            { "DataObjectAttributesRequiringElevation", (c,o) => {
                if (o is MemoryStream castedValue) {c.Add("DataObjectAttributesRequiringElevation", castedValue.ToArray()); return true; }
                else {return false;}
            } },
            { "Shell Object Offsets", (c,o) => {
                if (o is MemoryStream castedValue) {c.Add("Shell Object Offsets", castedValue.ToArray()); return true; }
                else {return false;}
            } },
            { "Preferred DropEffect", (c,o) => {
                if (o is MemoryStream castedValue) {c.Add("Preferred DropEffect", castedValue.ToArray()); return true; }
                else {return false;}
            } },
            { "AsyncFlag", (c,o) => {
                if (o is MemoryStream castedValue) {c.Add("AsyncFlag", castedValue.ToArray()); return true; }
                else {return false;}
            } },
            { "FileDrop", (c,o) => {
                if (o is string castedValue) {c.Add("FileDrop", System.Text.Encoding.UTF8.GetBytes(castedValue)); return true; }
                else {return false;}
            } },

        };

        public static ClipboardData ToDto(this IDataObject dataObject) {
            var clipboardData = new ClipboardData();
            var formats = dataObject.GetFormats();
            foreach(var format in formats) {
                var obj = dataObject.GetData(format);

                if(!converters.TryGetValue(format, out Func<ClipboardData, object, bool>? convertFunc)) {
                    throw new NotSupportedException(format);
                }

                if(!convertFunc(clipboardData, obj)) {
                    throw new InvalidCastException(format);
                }
            }
            return clipboardData;
        }
    }
}
