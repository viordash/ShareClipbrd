using System.Runtime.InteropServices;

namespace ShareClipbrd.Core.Clipboard {
    public class ImageConverter {

        public static object FromDib(Stream stream) {
            if(OperatingSystem.IsWindows()) {
                return stream switch {
                    MemoryStream memoryStream => FromDibToBmpFileData(memoryStream),
                    _ => throw new ArgumentException(nameof(stream))
                };
            }

            throw new NotSupportedException($"OS: {Environment.OSVersion}");
        }

        public static byte[] FromDibToBmpFileData(MemoryStream memoryStream) {
            if(BITMAPINFO.TryParse(memoryStream.ToArray(), out BITMAPINFO bitmapInfo)) {
            }

            if(BITMAPV5INFO.TryParse(memoryStream.ToArray(), out BITMAPV5INFO bitmapV5Info)) {
            }

            return memoryStream.ToArray();
        }
    }
}
