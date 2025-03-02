using Avalonia;
using Avalonia.Controls;
using System.Linq;

namespace ShareClipbrdApp.Helpers {
    public class WindowsHelper {
        public static void LoadLocation(System.Drawing.Point point, Window window) {
            if(point.IsEmpty) {
                return;
            }
            if(point.X + window.Width < 0) {
                return;
            }
            if(point.Y + window.Height < 0) {
                return;
            }

            var screensMaxWidth = window.Screens.All.Select(x => x.Bounds.X + x.Bounds.Width).Max();
            if(point.X + window.Width / 2 > screensMaxWidth) {
                return;
            }

            var screensMaxHeight = window.Screens.All.Select(y => y.Bounds.Y + y.Bounds.Height).Max();
            if(point.Y + window.Height / 2 > screensMaxHeight) {
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
