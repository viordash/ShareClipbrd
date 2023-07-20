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
            var bytes = memoryStream.ToArray();
            if(BITMAPINFO.TryParse(bytes, out BITMAPINFO bitmapInfo)) {
                return BitmapFile.Create(bytes, bitmapInfo);
            }

            if(BITMAPV5INFO.TryParse(bytes, out BITMAPV5INFO bitmapV5Info)) {
                return BitmapFile.Create(bytes, bitmapV5Info);
            }

            throw new ArgumentException("Deserialize BITMAPINFO. data invalid");
        }
    }
}
