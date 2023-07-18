using System.Runtime.InteropServices;

namespace ShareClipbrd.Core.Helpers {
    public class StructHelper {
        public static T PtrToStructure<T>(byte[] data) {
            var size = Marshal.SizeOf<T>();
            var ptPoit = Marshal.AllocHGlobal(size);
            if(ptPoit == 0) {
                throw new InsufficientMemoryException();
            }
            Marshal.Copy(data, 0, ptPoit, size);
            var obj = Marshal.PtrToStructure(ptPoit, typeof(T));
            Marshal.FreeHGlobal(ptPoit);
            if(obj == null) {
                throw new NullReferenceException();
            }
            return (T)obj;
        }
    }
}
