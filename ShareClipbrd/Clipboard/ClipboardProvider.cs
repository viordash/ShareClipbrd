using Clipboard.Core;

namespace Clipboard {
    internal class ClipboardProvider {
        IClipboard? current;

        public IClipboard Get(object? parent) {
            if(current != null) {
                return current;
            }

            current = new OS.Clipboard();
            return current;
        }
    }
}
