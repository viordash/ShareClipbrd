using System.Runtime.InteropServices;
using ShareClipbrd.Core.Helpers;

namespace ShareClipbrd.Core.Clipboard {
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
            var sizeDib = bitmapinfo.bmiHeader.biSize + bitmapinfo.bmiHeader.biClrUsed * StructHelper.Size<RGBQUAD>()
                + bitmapinfo.bmiHeader.biSizeImage;

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

        public static byte[] Create(byte[] bytes, BITMAPV5INFO bitmapinfo) {
            throw new NotImplementedException();
        }
    }
}
