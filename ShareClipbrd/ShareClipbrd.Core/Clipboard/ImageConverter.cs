using System.Runtime.InteropServices;

namespace ShareClipbrd.Core.Clipboard {
    public class ImageConverter {

        enum BitmapCompressionMode : uint {
            BI_RGB = 0,
            BI_RLE8 = 1,
            BI_RLE4 = 2,
            BI_BITFIELDS = 3,
            BI_JPEG = 4,
            BI_PNG = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        struct BITMAPINFOHEADER {
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

            public void Init() {
                biSize = (uint)Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RGBQUAD {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        struct BITMAPINFO {
            public BITMAPINFOHEADER bmiHeader;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
            public RGBQUAD[] bmiColors;

            public static BITMAPINFO Deserialize(byte[] data) {
                if((data.Length - Marshal.SizeOf<BITMAPINFOHEADER>()) % Marshal.SizeOf<RGBQUAD>() != 0) {
                    throw new ArgumentException("Deserialize BITMAPINFO. data invalid");
                }
                var ptPoit = Marshal.AllocHGlobal(data.Length);
                if(ptPoit == 0) {
                    throw new InsufficientMemoryException();
                }
                Marshal.Copy(data, 0, ptPoit, data.Length);
                var obj = Marshal.PtrToStructure(ptPoit, typeof(BITMAPINFO));
                Marshal.FreeHGlobal(ptPoit);
                if(obj == null) {
                    throw new ArgumentException("Deserialize BITMAPINFO. cast invalid");
                }
                var bitmapinfo = (BITMAPINFO)obj;
                if(bitmapinfo.bmiHeader.biSize != Marshal.SizeOf<BITMAPINFOHEADER>()) {
                    throw new ArgumentException("Deserialize BITMAPINFO. header invalid");
                }
                return bitmapinfo;
            }
        }

        public static object FromDib(Stream stream) {
            if(OperatingSystem.IsWindows()) {
                return stream switch {
                    MemoryStream memoryStream => FromDibToBmpFileData(memoryStream),
                    _ => throw new ArgumentException(nameof(stream))
                };
            }

            throw new NotSupportedException($"OS: {Environment.OSVersion}");
        }

        public static byte[] FromDibToBmpFileData(MemoryStream memoryStream) {
            var bitmapinfo = BITMAPINFO.Deserialize(memoryStream.ToArray());

            return memoryStream.ToArray();
        }
    }
}
