using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace Avalonia.X11 {
    internal unsafe static class XLib {
        private const string libX11 = "libX11.so.6";
        private const string libX11Randr = "libXrandr.so.2";
        private const string libX11Ext = "libXext.so.6";
        private const string libXInput = "libXi.so.6";
        private const string libXCursor = "libXcursor.so.1";

        [DllImport(libX11)]
        public static extern IntPtr XOpenDisplay(IntPtr display);

        [DllImport(libX11)]
        public static extern int XCloseDisplay(IntPtr display);

        [DllImport(libX11)]
        public static extern IntPtr XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, int width,
            int height, int border_width, IntPtr border, IntPtr background);

        [DllImport(libX11)]
        public static extern IntPtr XRootWindow(IntPtr display, int screen_number);
        [DllImport(libX11)]
        public static extern IntPtr XDefaultRootWindow(IntPtr display);

        [DllImport(libX11)]
        public static extern IntPtr XNextEvent(IntPtr display, out XEvent xevent);

        [DllImport(libX11)]
        public static extern IntPtr XNextEvent(IntPtr display, XEvent* xevent);

        [DllImport(libX11)]
        public static extern int XConnectionNumber(IntPtr diplay);

        [DllImport(libX11)]
        public static extern int XPending(IntPtr diplay);

        [DllImport(libX11)]
        public static extern IntPtr XSelectInput(IntPtr display, IntPtr window, IntPtr mask);

        [DllImport(libX11)]
        public static extern int XDestroyWindow(IntPtr display, IntPtr window);

        [DllImport(libX11)]
        public static extern int XFlush(IntPtr display);

        [DllImport(libX11)]
        public static extern int XSendEvent(IntPtr display, IntPtr window, bool propagate, IntPtr event_mask,
            ref XEvent send_event);

        [DllImport(libX11)]
        public static extern int XFree(IntPtr data);

        [DllImport(libX11)]
        public static extern int XFree(void* data);


        [DllImport(libX11)]
        public static extern IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

        [DllImport(libX11)]
        public static extern int XInternAtoms(IntPtr display, string[] atom_names, int atom_count, bool only_if_exists,
            IntPtr[] atoms);

        [DllImport(libX11)]
        public static extern IntPtr XGetAtomName(IntPtr display, IntPtr atom);

        public static string? GetAtomName(IntPtr display, IntPtr atom) {
            var ptr = XGetAtomName(display, atom);
            if(ptr == IntPtr.Zero)
                return null;
            var s = Marshal.PtrToStringAnsi(ptr);
            XFree(ptr);
            return s;
        }

        [DllImport(libX11)]
        public static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type,
            int format, PropertyMode mode, ref uint value, int nelements);

        [DllImport(libX11)]
        public static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type,
            int format, PropertyMode mode, ref IntPtr value, int nelements);

        [DllImport(libX11)]
        public static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type,
            int format, PropertyMode mode, byte[] data, int nelements);

        [DllImport(libX11)]
        public static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type,
            int format, PropertyMode mode, uint[] data, int nelements);

        [DllImport(libX11)]
        public static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type,
            int format, PropertyMode mode, int[] data, int nelements);

        [DllImport(libX11)]
        public static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type,
            int format, PropertyMode mode, IntPtr[] data, int nelements);
        [DllImport(libX11)]
        public static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type,
            int format, PropertyMode mode, void* data, int nelements);

        [DllImport(libX11)]
        public static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type,
            int format, PropertyMode mode, IntPtr atoms, int nelements);

        [DllImport(libX11, CharSet = CharSet.Ansi)]
        public static extern int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type,
            int format, PropertyMode mode, string text, int text_length);

        [DllImport(libX11)]
        public static extern int XGetWindowProperty(IntPtr display, IntPtr window, IntPtr atom, IntPtr long_offset,
            IntPtr long_length, bool delete, IntPtr req_type, out IntPtr actual_type, out int actual_format,
            out IntPtr nitems, out IntPtr bytes_after, out IntPtr prop);

        [DllImport(libX11)]
        public static extern IntPtr XSetErrorHandler(XErrorHandler error_handler);

        [DllImport(libX11)]
        public static extern IntPtr XGetErrorText(IntPtr display, byte code, StringBuilder buffer, int length);

        [DllImport(libX11)]
        public static extern int XInitThreads();

        [DllImport(libX11)]
        public static extern int XConvertSelection(IntPtr display, IntPtr selection, IntPtr target, IntPtr property,
            IntPtr requestor, IntPtr time);

        [DllImport(libX11)]
        public static extern IntPtr XGetSelectionOwner(IntPtr display, IntPtr selection);

        [DllImport(libX11)]
        public static extern int XSetSelectionOwner(IntPtr display, IntPtr selection, IntPtr owner, IntPtr time);

        [DllImport(libX11)]
        public static extern bool XFilterEvent(ref XEvent xevent, IntPtr window);

        [DllImport(libX11)]
        public static extern bool XFilterEvent(XEvent* xevent, IntPtr window);

        [StructLayout(LayoutKind.Explicit)]
        public struct epoll_data {
            [FieldOffset(0)]
            public IntPtr ptr;
            [FieldOffset(0)]
            public int fd;
            [FieldOffset(0)]
            public uint u32;
            [FieldOffset(0)]
            public ulong u64;
        }

        public const int EPOLLIN = 1;
        public const int EPOLL_CTL_ADD = 1;
        public const int O_NONBLOCK = 2048;

        [StructLayout(LayoutKind.Sequential)]
        public struct epoll_event {
            public uint events;
            public epoll_data data;
        }

        [DllImport("libc")]
        public extern static int epoll_create1(int size);

        [DllImport("libc")]
        public extern static int epoll_ctl(int epfd, int op, int fd, ref epoll_event __event);

        [DllImport("libc")]
        public extern static int epoll_wait(int epfd, epoll_event* events, int maxevents, int timeout);

        [DllImport("libc")]
        public extern static int pipe2(int* fds, int flags);
        [DllImport("libc")]
        public extern static IntPtr write(int fd, void* buf, IntPtr count);

        [DllImport("libc")]
        public extern static IntPtr read(int fd, void* buf, IntPtr count);

        public enum EventCodes {
            X11 = 1,
            Signal = 2
        }
    }
}
