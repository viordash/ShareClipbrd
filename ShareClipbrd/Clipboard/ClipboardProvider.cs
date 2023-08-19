using Clipboard.Core;

namespace Clipboard {
    internal class ClipboardProvider {
        IClipboard? current;

        public IClipboard Current {
            get {
                if(current != null) {
                    return current;
                }

                if(OperatingSystem.IsWindows()) {
                    current = new Win.Clipboard();
                    return current;
                }

                throw new NotSupportedException($"OS: {Environment.OSVersion}");
            }
        }
    }
}
