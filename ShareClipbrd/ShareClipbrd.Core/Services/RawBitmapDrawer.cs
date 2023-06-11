namespace ShareClipbrd.Core.Services {
    public struct Color {
        public byte r, g, b, a;

        public static Color FromRGB(byte r, byte g, byte b) {
            return new Color { r = r, g = g, b = b, a = 255 };
        }
    }

    public interface IRawBitmapDrawer {
        void SetPixel(int x, int y, Color color);
        void Fill(Color color);
    }

    public class RawBitmapDrawer : IRawBitmapDrawer {
        private int width;
        private int height;
        private IntPtr firstPixelAddr;

        public int Width { get => width; }
        public int Height { get => height; }

        public RawBitmapDrawer(int width, int height, IntPtr firstPixelAddr) {
            this.width = width;
            this.height = height;
            this.firstPixelAddr = firstPixelAddr;
        }

        public void SetPixel(int x, int y, Color color) {
            if(x >= width || y >= height) {
                return;
            }
            unsafe {
                var ptr = (Color*)firstPixelAddr;
                *(ptr + x + y * width) = color;
            }
        }

        public void Fill(Color color) {
            var length = width * height;
            unsafe {
                var ptr = (Color*)firstPixelAddr;
                for(int i = 0; i < length; i++) {
                    *ptr = color;
                    ptr++;
                }
            }
        }
    }
}
