using Clipboard.Core;

namespace Clipboard {
    internal class ClipboardProvider {
        static IClipboard? current;

        public static IClipboard Get(object? parent) {
            if(current != null) {
                return current;
            }

            current = new OS.Clipboard(parent);
            return current;
        }
    }
}
