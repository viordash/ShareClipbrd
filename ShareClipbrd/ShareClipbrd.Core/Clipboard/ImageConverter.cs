using ShareClipbrd.Core.Helpers;

namespace ShareClipbrd.Core.Clipboard {
    public class ImageConverter {

        public static byte[] FromDibToBmpFileData(MemoryStream memoryStream) {
            var bytes = memoryStream.ToArray();
            if(BITMAPINFO.TryParse(bytes, out BITMAPINFO bitmapInfo)) {
                var bytesBmp = BitmapFile.Create(bytes, bitmapInfo);
                // File.WriteAllBytes("/home/viordash/Downloads/test.bmp", bytesBmp);
                return bytesBmp;
            }

            if(BITMAPV5INFO.TryParse(bytes, out BITMAPV5INFO bitmapV5Info)) {
                return BitmapFile.Create(bytes, bitmapV5Info);
            }

            throw new ArgumentException("Deserialize BITMAPINFO. data invalid");
        }

        public static byte[] FromBmpFileToDibData(MemoryStream memoryStream) {
            var bytesBmp = memoryStream.ToArray();
            return BitmapFile.ExtractDib(bytesBmp);
        }

        public static byte[] FromDibToDib(MemoryStream memoryStream) {
            var bytes = memoryStream.ToArray();
            if(BITMAPINFO.TryParse(bytes, out BITMAPINFO bitmapInfo)) {
                var size = bitmapInfo.bmiHeader.biSize + bitmapInfo.bmiHeader.biClrUsed * StructHelper.Size<RGBQUAD>()
                        + bitmapInfo.bmiHeader.biSizeImage;
                return bytes.Take((int)size).ToArray();
            }

            if(BITMAPV5INFO.TryParse(bytes, out BITMAPV5INFO bitmapV5Info)) {
                var size = bitmapV5Info.bmiHeader.bV5Size + bitmapV5Info.bmiHeader.bV5ClrUsed * StructHelper.Size<RGBQUAD>()
                        + bitmapV5Info.bmiHeader.bV5SizeImage;
                return bytes.Take((int)size).ToArray();
            }

            throw new ArgumentException("Deserialize BITMAPINFO. data invalid");
        }
    }
}
