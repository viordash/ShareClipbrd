namespace ShareClipbrd.Core.Clipboard {
    public class ImageConverter {

        public static object FromDib(Stream stream) {
            if(OperatingSystem.IsWindows()) {
                return stream switch {
                    MemoryStream memoryStream => memoryStream.ToArray(),
                    _ => throw new ArgumentException(nameof(stream))
                };
            }

            throw new NotSupportedException($"OS: {Environment.OSVersion}");
        }
    }
}
