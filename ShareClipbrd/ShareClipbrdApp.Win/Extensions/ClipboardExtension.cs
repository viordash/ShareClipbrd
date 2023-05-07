using System.Windows;
using ShareClipbrd.Core;

namespace UsAcRe.Core.Extensions {
    public static class ClipboardExtension {
        public static ClipboardData ToDto(this IDataObject dataObject) {
            return new ClipboardData();
        }

    }
}
