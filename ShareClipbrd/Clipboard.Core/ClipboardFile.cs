﻿using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Clipboard.Core {
    public class ClipboardFile {
        public class Convert {
            public Func<StringCollection, Func<string, Task<object?>>, Task<bool>> From { get; set; }
            public Convert(Func<StringCollection, Func<string, Task<object?>>, Task<bool>> from) {
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

        static string[] ParseUriLines(string text) {
            var lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var files = new List<string>();
            foreach(var item in lines) {
                if(!Uri.TryCreate(item, UriKind.Absolute, out Uri? uri)) {
                    continue;
                }
                files.Add(uri.LocalPath.Trim());
            }
            return files.ToArray();
        }

        static bool TryUriParse(object? data, [MaybeNullWhen(false)] out string[] files) {
            if(data is string lines) {
                files = ParseUriLines(lines);
                return true;
            }
            if(data is byte[] bytes) {
                files = ParseUriLines(System.Text.Encoding.UTF8.GetString(bytes));
                return true;
            }
            files = null;
            return false;
        }

        public static readonly Dictionary<string, Convert> Converters = new(){
            { Format.FileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.FileNames);
                    if (data is IList<string> list) {
                        var files = new List<string>();
                        foreach(var item in list) {
                            if (!Uri.TryCreate(item, UriKind.Absolute, out Uri? uri)) {
                                continue;
                            }
                            files.Add(uri.LocalPath.Trim());
                        }
                        c.AddRange(files.ToArray());
                        return true;
                    }
                    return false;
                })
            },

            { Format.FileDrop, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.FileDrop);
                    if (data is IList<string> list) {
                        c.AddRange(list.ToArray());
                        return true;
                    }
                    return false;
                })
            },

            { Format.XMateFileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.XMateFileNames);
                    if (TryUriParse(data, out string[]? files)) {
                        c.AddRange(files);
                        return true;
                    }
                    return false;
                })
                },

            { Format.XKdeFileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.XKdeFileNames);
                    if (TryUriParse(data, out string[]? files)) {
                        c.AddRange(files);
                        return true;
                    }
                    return false;
                })
                },

            { Format.XGnomeFileNames, new Convert(
                async (c, getDataFunc) => {
                    var data = await getDataFunc(Format.XGnomeFileNames);
                    if (TryUriParse(data, out string[]? files)) {
                        c.AddRange(files);
                        return true;
                    }
                    return false;
                })
                },
        };

        public static async Task<StringCollection> GetList(string[] formats, Func<string, Task<object?>> getDataFunc) {
            Debug.WriteLine(string.Join(", ", formats));

            var fileDropList = new StringCollection();
            foreach(var format in formats) {
                if(!Converters.TryGetValue(format, out Convert? convertFunc)) {
                    Debug.WriteLine($"not supported format: {format}");
                    continue;
                }

                if(!await convertFunc.From(fileDropList, getDataFunc)) {
                    throw new InvalidDataException(format);
                }
                break;
            }
            return fileDropList;
        }

        public static void SetFileDropList(Action<string, object> setDataFunc, IList<string> files) {

            if(OperatingSystem.IsWindows()) {
                setDataFunc(ClipboardFile.Format.FileNames, files);
                return;
            }

            if(OperatingSystem.IsLinux()) {
                var desktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
                var format = desktop?.ToLower() switch {
                    "kde" => ClipboardFile.Format.XKdeFileNames,
                    "mate" or "xfce" => ClipboardFile.Format.XMateFileNames,
                    "gnome" => ClipboardFile.Format.XGnomeFileNames,
                    _ => ClipboardFile.Format.XGnomeFileNames
                };

                var urls = new List<string>() { "copy" };
                foreach(var item in files) {
                    if(!Uri.TryCreate(item, UriKind.Absolute, out Uri? uri)) {
                        continue;
                    }
                    urls.Add(uri.AbsoluteUri);
                }

                var lines = string.Join("\n", urls);

                setDataFunc(format, lines);
                return;
            }

            throw new NotSupportedException($"OS: {Environment.OSVersion}");
        }

        public static bool ContainsFileDropList(string format) {
            return Converters.ContainsKey(format);
        }

    }
}
