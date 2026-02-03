using Avalonia;
using Avalonia.Controls;
using System.Linq;

namespace ShareClipbrdApp.Helpers {
    public class WindowsHelper {
        public static void LoadLocation(System.Drawing.Point point, Window window) {
            if(point.IsEmpty) {
                return;
            }
            var pixelPoint = new PixelPoint(point.X, point.Y);

            var fitToAnyScreen = window.Screens.All.Any(x => x.Bounds.Contains(pixelPoint));
            if(!fitToAnyScreen) {
                return;
            }
            window.Position = new PixelPoint(point.X, point.Y);
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
