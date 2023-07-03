using Avalonia;
using Avalonia.Controls;

namespace ShareClipbrdApp.Helpers {
    public class WindowsHelper {
        public static void LoadLocation(System.Drawing.Point point, Window window) {
            if(!point.IsEmpty
                && point.X + window.Width >= 0
                && point.Y + window.Height >= 0
                && window.Screens.Primary?.WorkingArea.Width > point.X
                && window.Screens.Primary?.WorkingArea.Height > point.Y) {
                window.Position = new PixelPoint(point.X, point.Y);
            }
        }

        public static void LoadSize(System.Drawing.Size size, Window window) {
            if(!size.IsEmpty
                && window.Screens.Primary?.Bounds.Width > size.Width
                && window.Screens.Primary?.Bounds.Height > size.Height) {
                window.Width = size.Width;
                window.Height = size.Height;
            }
        }
    }
}
