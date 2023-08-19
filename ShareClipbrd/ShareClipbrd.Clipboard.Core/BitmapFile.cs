using System.Runtime.InteropServices;
using ShareClipbrd.Clipboard.Core.Helpers;

namespace ShareClipbrd.Clipboard.Core {
    public class BitmapFile {
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct BITMAPFILEHEADER {
            public ushort bfType;
            public uint bfSize;
            public ushort bfReserved1;
            public ushort bfReserved2;
            public uint bfOffBits;

            public BITMAPFILEHEADER() {
                bfSize = StructHelper.Size(this);
            }
        }

        public static byte[] Create(byte[] bytes, BITMAPINFO bitmapinfo) {
            var bitmapFileHeader = new BITMAPFILEHEADER();

            using var memorystream = new MemoryStream();
            var sizeImage = bitmapinfo.bmiHeader.biSizeImage > 0
                ? bitmapinfo.bmiHeader.biSizeImage
                : (uint)(bitmapinfo.bmiHeader.biBitCount / 8) * (uint)bitmapinfo.bmiHeader.biWidth * (uint)bitmapinfo.bmiHeader.biHeight;
            var sizeDib = bitmapinfo.bmiHeader.biSize + bitmapinfo.bmiHeader.biClrUsed * StructHelper.Size<RGBQUAD>()
                + sizeImage;

            bitmapFileHeader.bfType = 0x4D42;
            bitmapFileHeader.bfSize = StructHelper.Size<BITMAPFILEHEADER>() + sizeDib;
            bitmapFileHeader.bfReserved1 = 0;
            bitmapFileHeader.bfReserved2 = 0;
            bitmapFileHeader.bfOffBits = StructHelper.Size<BITMAPFILEHEADER>() + bitmapinfo.bmiHeader.biSize
                + bitmapinfo.bmiHeader.biClrUsed * StructHelper.Size<RGBQUAD>();

            memorystream.Write(StructHelper.ToBytes(bitmapFileHeader));

            memorystream.Write(bytes, 0, (int)sizeDib);
            return memorystream.ToArray();
        }

        public static byte[] ExtractDib(byte[] bytesBmp) {
            return bytesBmp.Skip((int)StructHelper.Size<BITMAPFILEHEADER>()).ToArray();
        }

        public static byte[] Create(byte[] bytes, BITMAPV5INFO bitmapinfo) {
            throw new NotImplementedException();
        }
    }
}
