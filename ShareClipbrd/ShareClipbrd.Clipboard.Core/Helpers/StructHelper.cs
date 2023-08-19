using System.Runtime.InteropServices;

namespace ShareClipbrd.Clipboard.Core.Helpers {
    public class StructHelper {
        public static uint Size<T>() {
            return (uint)Marshal.SizeOf<T>();
        }

        public static uint Size<T>(T str) {
            return (uint)Marshal.SizeOf<T>();
        }

        public static T FromBytes<T>(byte[] data) {
            var size = Marshal.SizeOf<T>();
            var ptPoit = Marshal.AllocHGlobal(size);
            if(ptPoit == 0) {
                throw new InsufficientMemoryException();
            }
            try {
                Marshal.Copy(data, 0, ptPoit, size);
                var obj = Marshal.PtrToStructure(ptPoit, typeof(T));
                if(obj == null) {
                    throw new NullReferenceException();
                }
                return (T)obj;
            } finally {
                Marshal.FreeHGlobal(ptPoit);
            }
        }

        public static byte[] ToBytes<T>(T str) {
            if(str == null) {
                throw new NullReferenceException();
            }
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            var ptr = Marshal.AllocHGlobal(size);
            if(ptr == IntPtr.Zero) {
                throw new InsufficientMemoryException();
            }
            try {
                Marshal.StructureToPtr(str, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
            } finally {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }
    }
}
