using System.Runtime.InteropServices;
using ShareClipbrd.Clipboard.Core.Helpers;

namespace ShareClipbrd.Clipboard.Core {
    public enum BitmapCompressionMode : uint {
        BI_RGB = 0,
        BI_RLE8 = 1,
        BI_RLE4 = 2,
        BI_BITFIELDS = 3,
        BI_JPEG = 4,
        BI_PNG = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CIEXYZ {
        public long ciexyzX;
        public long ciexyzY;
        public long ciexyzZ;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CIEXYZTRIPLE {
        public CIEXYZ ciexyzRed;
        public CIEXYZ ciexyzGreen;
        public CIEXYZ ciexyzBlue;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public BitmapCompressionMode biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;

        public BITMAPINFOHEADER() {
            biSize = StructHelper.Size(this);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPV5HEADER {
        public uint bV5Size;
        public int bV5Width;
        public int bV5Height;
        public ushort bV5Planes;
        public ushort bV5BitCount;
        public BitmapCompressionMode bV5Compression;
        public uint bV5SizeImage;
        public int bV5XPelsPerMeter;
        public int bV5YPelsPerMeter;
        public uint bV5ClrUsed;
        public uint bV5ClrImportant;

        public uint bV5RedMask;
        public uint bV5GreenMask;
        public uint bV5BlueMask;
        public uint bV5AlphaMask;
        public uint bV5CSType;
        public CIEXYZTRIPLE bV5Endpoints;
        public uint bV5GammaRed;
        public uint bV5GammaGreen;
        public uint bV5GammaBlue;
        public uint bV5Intent;
        public uint bV5ProfileData;
        public uint bV5ProfileSize;
        public uint bV5Reserved;

        public BITMAPV5HEADER() {
            bV5Size = StructHelper.Size(this);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RGBQUAD {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbReserved;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct BITMAPINFO {
        public BITMAPINFOHEADER bmiHeader;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
        public RGBQUAD[] bmiColors;

        public static bool TryParse(byte[] bytes, out BITMAPINFO bitmapinfo) {
            if(bytes.Length < StructHelper.Size<BITMAPINFOHEADER>()) {
                bitmapinfo = new BITMAPINFO();
                return false;
            }

            bitmapinfo = StructHelper.FromBytes<BITMAPINFO>(bytes);
            if(bitmapinfo.bmiHeader.biSize != StructHelper.Size<BITMAPINFOHEADER>()) {
                return false;
            }
            return true;
        }
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct BITMAPV5INFO {
        public BITMAPV5HEADER bmiHeader;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
        public RGBQUAD[] bmiColors;

        public static bool TryParse(byte[] bytes, out BITMAPV5INFO bitmapinfo) {
            if(bytes.Length < StructHelper.Size<BITMAPV5HEADER>()) {
                bitmapinfo = new BITMAPV5INFO();
                return false;
            }

            bitmapinfo = StructHelper.FromBytes<BITMAPV5INFO>(bytes);
            if(bitmapinfo.bmiHeader.bV5Size != StructHelper.Size<BITMAPV5HEADER>()) {
                return false;
            }
            return true;
        }
    }
}
