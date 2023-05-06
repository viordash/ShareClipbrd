using System.Windows;

namespace ShareClipbrdApp.Win.Helpers {
    public class WindowsHelper {
        public static void LoadLocation(System.Drawing.Point point, Window window) {
            if(!point.IsEmpty
                && point.X + window.Width >= 0
                && point.Y + window.Height >= 0
                && SystemParameters.FullPrimaryScreenWidth > point.X
                && SystemParameters.FullPrimaryScreenHeight > point.Y) {
                window.Left = point.X;
                window.Top = point.Y;
            }
        }

        public static void LoadSize(System.Drawing.Size size, Window window) {
            if(!size.IsEmpty
                && SystemParameters.FullPrimaryScreenWidth > size.Width
                && SystemParameters.FullPrimaryScreenHeight > size.Height) {
                window.Width = size.Width;
                window.Height = size.Height;
            }
        }
    }
}
