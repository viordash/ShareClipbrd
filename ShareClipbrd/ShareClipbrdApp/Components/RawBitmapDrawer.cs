using System;

namespace ShareClipbrdApp.Components {
    public struct Color {
        public byte r, g, b, a;

        public static Color FromRGB(byte a, byte r, byte g, byte b) {
            return new Color { r = r, g = g, b = b, a = a };
        }
    }

    public class RawBitmapDrawer {
        readonly int width;
        readonly int height;
        readonly IntPtr firstPixelAddr;

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
