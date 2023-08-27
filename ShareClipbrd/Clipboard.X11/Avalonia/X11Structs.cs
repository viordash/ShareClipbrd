// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software",, to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// NOT COMPLETE

using System;
using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable 649

namespace Avalonia.X11
{
	//
	// In the structures below, fields of type long are mapped to IntPtr.
	// This will work on all platforms where sizeof(long)==sizeof(void*), which
	// is almost all platforms except WIN64.
	//

	[StructLayout(LayoutKind.Sequential)]
	internal struct XAnyEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal int send_event;
		internal IntPtr display;
		internal IntPtr window;
	}


	[StructLayout(LayoutKind.Sequential)]
	internal struct XPropertyEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal int send_event;
		internal IntPtr display;
		internal IntPtr window;
		internal IntPtr atom;
		internal IntPtr time;
		internal int state;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XSelectionClearEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal int send_event;
		internal IntPtr display;
		internal IntPtr window;
		internal IntPtr selection;
		internal IntPtr time;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XSelectionRequestEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal int send_event;
		internal IntPtr display;
		internal IntPtr owner;
		internal IntPtr requestor;
		internal IntPtr selection;
		internal IntPtr target;
		internal IntPtr property;
		internal IntPtr time;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XSelectionEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal int send_event;
		internal IntPtr display;
		internal IntPtr requestor;
		internal IntPtr selection;
		internal IntPtr target;
		internal IntPtr property;
		internal IntPtr time;
	}


	[StructLayout(LayoutKind.Sequential)]
	internal struct XErrorEvent
	{
		internal XEventName type;
		internal IntPtr display;
		internal IntPtr resourceid;
		internal IntPtr serial;
		internal byte error_code;
		internal XRequest request_code;
		internal byte minor_code;
	}


	[StructLayout(LayoutKind.Explicit)]
	internal struct XEvent
	{
		[FieldOffset(0)] internal XEventName type;
		[FieldOffset(0)] internal XAnyEvent AnyEvent;
		[FieldOffset(0)] internal XPropertyEvent PropertyEvent;

		[FieldOffset(0)] internal XSelectionClearEvent SelectionClearEvent;
		[FieldOffset(0)] internal XSelectionRequestEvent SelectionRequestEvent;
		[FieldOffset(0)] internal XSelectionEvent SelectionEvent;

		[FieldOffset(0)] internal XErrorEvent ErrorEvent;

		public override string ToString()
		{
			switch (type)
			{
				case XEventName.SelectionClear:
					return ToString(SelectionClearEvent);
				case XEventName.SelectionNotify:
					return ToString(SelectionEvent);
				case XEventName.SelectionRequest:
					return ToString(SelectionRequestEvent);
				default:
					return type.ToString();
			}
		}

		[UnconditionalSuppressMessage("Trimming", "IL2075", Justification = TrimmingMessages.IgnoreNativeAotSupressWarningMessage)]
		public static string ToString(object ev)
		{
			string result = string.Empty;
			Type type = ev.GetType();
			FieldInfo[] fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance);
			for (int i = 0; i < fields.Length; i++)
			{
				if (!string.IsNullOrEmpty(result))
				{
					result += ", ";
				}
				object? value = fields[i].GetValue(ev);
				result += fields[i].Name + "=" + (value == null ? "<null>" : value.ToString());
			}
			return type.Name + " (" + result + ")";
		}
	}


	internal enum XEventName
	{
		KeyPress = 2,
		KeyRelease = 3,
		ButtonPress = 4,
		ButtonRelease = 5,
		MotionNotify = 6,
		EnterNotify = 7,
		LeaveNotify = 8,
		FocusIn = 9,
		FocusOut = 10,
		KeymapNotify = 11,
		Expose = 12,
		GraphicsExpose = 13,
		NoExpose = 14,
		VisibilityNotify = 15,
		CreateNotify = 16,
		DestroyNotify = 17,
		UnmapNotify = 18,
		MapNotify = 19,
		MapRequest = 20,
		ReparentNotify = 21,
		ConfigureNotify = 22,
		ConfigureRequest = 23,
		GravityNotify = 24,
		ResizeRequest = 25,
		CirculateNotify = 26,
		CirculateRequest = 27,
		PropertyNotify = 28,
		SelectionClear = 29,
		SelectionRequest = 30,
		SelectionNotify = 31,
		ColormapNotify = 32,
		ClientMessage = 33,
		MappingNotify = 34,
		GenericEvent = 35,
		LASTEvent
	}


	internal enum SendEventValues
	{
		PointerWindow = 0,
		InputFocus = 1
	}

	internal enum CreateWindowArgs
	{
		CopyFromParent = 0,
		ParentRelative = 1,
		InputOutput = 1,
		InputOnly = 2
	}

	[Flags]
	internal enum EventMask
	{
		NoEventMask = 0,
		KeyPressMask = 1 << 0,
		KeyReleaseMask = 1 << 1,
		ButtonPressMask = 1 << 2,
		ButtonReleaseMask = 1 << 3,
		EnterWindowMask = 1 << 4,
		LeaveWindowMask = 1 << 5,
		PointerMotionMask = 1 << 6,
		PointerMotionHintMask = 1 << 7,
		Button1MotionMask = 1 << 8,
		Button2MotionMask = 1 << 9,
		Button3MotionMask = 1 << 10,
		Button4MotionMask = 1 << 11,
		Button5MotionMask = 1 << 12,
		ButtonMotionMask = 1 << 13,
		KeymapStateMask = 1 << 14,
		ExposureMask = 1 << 15,
		VisibilityChangeMask = 1 << 16,
		StructureNotifyMask = 1 << 17,
		ResizeRedirectMask = 1 << 18,
		SubstructureNotifyMask = 1 << 19,
		SubstructureRedirectMask = 1 << 20,
		FocusChangeMask = 1 << 21,
		PropertyChangeMask = 1 << 22,
		ColormapChangeMask = 1 << 23,
		OwnerGrabButtonMask = 1 << 24
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct XColor
	{
		internal IntPtr pixel;
		internal ushort red;
		internal ushort green;
		internal ushort blue;
		internal byte flags;
		internal byte pad;
	}

	internal enum Atom
	{
		AnyPropertyType = 0,

		XA_ATOM = 4,

	}

	internal enum PropertyMode
	{
		Replace = 0,
		Prepend = 1,
		Append = 2
	}

	internal enum PropertyState
	{
		NewValue = 0,
		Delete = 1
	}


	internal delegate int XErrorHandler(IntPtr DisplayHandle, ref XErrorEvent error_event);

	internal enum XRequest : byte
	{
		X_CreateWindow = 1,
		X_ChangeWindowAttributes = 2,
		X_GetWindowAttributes = 3,
		X_DestroyWindow = 4,
		X_DestroySubwindows = 5,
		X_ChangeSaveSet = 6,
		X_ReparentWindow = 7,
		X_MapWindow = 8,
		X_MapSubwindows = 9,
		X_UnmapWindow = 10,
		X_UnmapSubwindows = 11,
		X_ConfigureWindow = 12,
		X_CirculateWindow = 13,
		X_GetGeometry = 14,
		X_QueryTree = 15,
		X_InternAtom = 16,
		X_GetAtomName = 17,
		X_ChangeProperty = 18,
		X_DeleteProperty = 19,
		X_GetProperty = 20,
		X_ListProperties = 21,
		X_SetSelectionOwner = 22,
		X_GetSelectionOwner = 23,
		X_ConvertSelection = 24,
		X_SendEvent = 25,
		X_GrabPointer = 26,
		X_UngrabPointer = 27,
		X_GrabButton = 28,
		X_UngrabButton = 29,
		X_ChangeActivePointerGrab = 30,
		X_GrabKeyboard = 31,
		X_UngrabKeyboard = 32,
		X_GrabKey = 33,
		X_UngrabKey = 34,
		X_AllowEvents = 35,
		X_GrabServer = 36,
		X_UngrabServer = 37,
		X_QueryPointer = 38,
		X_GetMotionEvents = 39,
		X_TranslateCoords = 40,
		X_WarpPointer = 41,
		X_SetInputFocus = 42,
		X_GetInputFocus = 43,
		X_QueryKeymap = 44,
		X_OpenFont = 45,
		X_CloseFont = 46,
		X_QueryFont = 47,
		X_QueryTextExtents = 48,
		X_ListFonts = 49,
		X_ListFontsWithInfo = 50,
		X_SetFontPath = 51,
		X_GetFontPath = 52,
		X_CreatePixmap = 53,
		X_FreePixmap = 54,
		X_CreateGC = 55,
		X_ChangeGC = 56,
		X_CopyGC = 57,
		X_SetDashes = 58,
		X_SetClipRectangles = 59,
		X_FreeGC = 60,
		X_ClearArea = 61,
		X_CopyArea = 62,
		X_CopyPlane = 63,
		X_PolyPoint = 64,
		X_PolyLine = 65,
		X_PolySegment = 66,
		X_PolyRectangle = 67,
		X_PolyArc = 68,
		X_FillPoly = 69,
		X_PolyFillRectangle = 70,
		X_PolyFillArc = 71,
		X_PutImage = 72,
		X_GetImage = 73,
		X_PolyText8 = 74,
		X_PolyText16 = 75,
		X_ImageText8 = 76,
		X_ImageText16 = 77,
		X_CreateColormap = 78,
		X_FreeColormap = 79,
		X_CopyColormapAndFree = 80,
		X_InstallColormap = 81,
		X_UninstallColormap = 82,
		X_ListInstalledColormaps = 83,
		X_AllocColor = 84,
		X_AllocNamedColor = 85,
		X_AllocColorCells = 86,
		X_AllocColorPlanes = 87,
		X_FreeColors = 88,
		X_StoreColors = 89,
		X_StoreNamedColor = 90,
		X_QueryColors = 91,
		X_LookupColor = 92,
		X_CreateCursor = 93,
		X_CreateGlyphCursor = 94,
		X_FreeCursor = 95,
		X_RecolorCursor = 96,
		X_QueryBestSize = 97,
		X_QueryExtension = 98,
		X_ListExtensions = 99,
		X_ChangeKeyboardMapping = 100,
		X_GetKeyboardMapping = 101,
		X_ChangeKeyboardControl = 102,
		X_GetKeyboardControl = 103,
		X_Bell = 104,
		X_ChangePointerControl = 105,
		X_GetPointerControl = 106,
		X_SetScreenSaver = 107,
		X_GetScreenSaver = 108,
		X_ChangeHosts = 109,
		X_ListHosts = 110,
		X_SetAccessControl = 111,
		X_SetCloseDownMode = 112,
		X_KillClient = 113,
		X_RotateProperties = 114,
		X_ForceScreenSaver = 115,
		X_SetPointerMapping = 116,
		X_GetPointerMapping = 117,
		X_SetModifierMapping = 118,
		X_GetModifierMapping = 119,
		X_NoOperation = 127
	}

	[Flags]
	internal enum XIMProperties
	{
		XIMPreeditArea = 0x0001,
		XIMPreeditCallbacks = 0x0002,
		XIMPreeditPosition = 0x0004,
		XIMPreeditNothing = 0x0008,
		XIMPreeditNone = 0x0010,
		XIMStatusArea = 0x0100,
		XIMStatusCallbacks = 0x0200,
		XIMStatusNothing = 0x0400,
		XIMStatusNone = 0x0800,
	}

	[Flags]
	internal enum WindowType
	{
		Client = 1,
		Whole = 2,
		Both = 3
	}

	internal enum XEmbedMessage
	{
		EmbeddedNotify = 0,
		WindowActivate = 1,
		WindowDeactivate = 2,
		RequestFocus = 3,
		FocusIn = 4,
		FocusOut = 5,
		FocusNext = 6,
		FocusPrev = 7,
		/* 8-9 were used for XEMBED_GRAB_KEY/XEMBED_UNGRAB_KEY */
		ModalityOn = 10,
		ModalityOff = 11,
		RegisterAccelerator = 12,
		UnregisterAccelerator = 13,
		ActivateAccelerator = 14
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XcursorImage
	{
		public int version;
		public int size;       /* nominal size for matching */
		public int width;      /* actual width */
		public int height;     /* actual height */
		public int xhot;       /* hot spot x (must be inside image) */
		public int yhot;       /* hot spot y (must be inside image) */
		public int delay;       /* hot spot y (must be inside image) */
		public IntPtr pixels;    /* pointer to pixels */

		public override string ToString()
		{
			return $"XCursorImage (version: {version}, size: {size}, width: {width}, height: {height}, xhot: {xhot}, yhot: {yhot}, delay: {delay}, pixels: {pixels}";
		}
	};

	[StructLayout(LayoutKind.Sequential)]
	internal struct XcursorImages
	{
		public int nimage;     /* number of images */
		public IntPtr images;   /* array of XcursorImage pointers */
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct XIMStyles
	{
		public ushort count_styles;
		public IntPtr* supported_styles;
	}

	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	internal struct XPoint
	{
		public short X;
		public short Y;
	}

	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	internal struct XRectangle
	{
		public short X;
		public short Y;
		public short W;
		public short H;
	}


	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	internal class XIMCallback
	{
		public IntPtr client_data;
		public XIMProc callback;
		[NonSerialized] private GCHandle gch;

		public XIMCallback(IntPtr clientData, XIMProc proc)
		{
			this.client_data = clientData;
			this.gch = GCHandle.Alloc(proc);
			this.callback = proc;
		}

		~XIMCallback()
		{
			gch.Free();
		}
	}

	[StructLayout(LayoutKind.Sequential)]
#pragma warning disable CA1815 // Override equals and operator equals on value types
	internal unsafe struct XImage
#pragma warning restore CA1815 // Override equals and operator equals on value types
	{
		public int width, height; /* size of image */
		public int xoffset; /* number of pixels offset in X direction */
		public int format; /* XYBitmap, XYPixmap, ZPixmap */
		public IntPtr data; /* pointer to image data */
		public int byte_order; /* data byte order, LSBFirst, MSBFirst */
		public int bitmap_unit; /* quant. of scanline 8, 16, 32 */
		public int bitmap_bit_order; /* LSBFirst, MSBFirst */
		public int bitmap_pad; /* 8, 16, 32 either XY or ZPixmap */
		public int depth; /* depth of image */
		public int bytes_per_line; /* accelerator to next scanline */
		public int bits_per_pixel; /* bits per pixel (ZPixmap) */
		public ulong red_mask; /* bits in z arrangement */
		public ulong green_mask;
		public ulong blue_mask;
		private fixed byte funcs[128];
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XVisualInfo
	{
		internal IntPtr visual;
		internal IntPtr visualid;
		internal int screen;
		internal uint depth;
		internal int klass;
		internal IntPtr red_mask;
		internal IntPtr green_mask;
		internal IntPtr blue_mask;
		internal int colormap_size;
		internal int bits_per_rgb;
	}

	internal enum XIMFeedback
	{
		Reverse = 1,
		Underline = 2,
		Highlight = 4,
		Primary = 32,
		Secondary = 64,
		Tertiary = 128,
	}

	internal struct XIMFeedbackStruct
	{
		public byte FeedbackMask; // one or more of XIMFeedback enum
	}

	internal struct XIMText
	{
		public ushort Length;
		public IntPtr Feedback; // to XIMFeedbackStruct
		public int EncodingIsWChar;
		public IntPtr String; // it could be either char* or wchar_t*
	}

	internal struct XIMPreeditDrawCallbackStruct
	{
		public int Caret;
		public int ChangeFirst;
		public int ChangeLength;
		public IntPtr Text; // to XIMText
	}

	internal enum XIMCaretDirection
	{
		XIMForwardChar,
		XIMBackwardChar,
		XIMForwardWord,
		XIMBackwardWord,
		XIMCaretUp,
		XIMCaretDown,
		XIMNextLine,
		XIMPreviousLine,
		XIMLineStart,
		XIMLineEnd,
		XIMAbsolutePosition,
		XIMDontChange
	}

	internal enum XIMCaretStyle
	{
		IsInvisible,
		IsPrimary,
		IsSecondary
	}

	internal struct XIMPreeditCaretCallbackStruct
	{
		public int Position;
		public XIMCaretDirection Direction;
		public XIMCaretStyle Style;
	}

	// only PreeditStartCallback requires return value though.
	internal delegate int XIMProc(IntPtr xim, IntPtr clientData, IntPtr callData);

	internal static class XNames
	{
		public const string XNVaNestedList = "XNVaNestedList";
		public const string XNQueryInputStyle = "queryInputStyle";
		public const string XNClientWindow = "clientWindow";
		public const string XNInputStyle = "inputStyle";
		public const string XNFocusWindow = "focusWindow";
		public const string XNResourceName = "resourceName";
		public const string XNResourceClass = "resourceClass";

		// XIMPreeditCallbacks delegate names.
		public const string XNPreeditStartCallback = "preeditStartCallback";
		public const string XNPreeditDoneCallback = "preeditDoneCallback";
		public const string XNPreeditDrawCallback = "preeditDrawCallback";
		public const string XNPreeditCaretCallback = "preeditCaretCallback";
		public const string XNPreeditStateNotifyCallback = "preeditStateNotifyCallback";
		public const string XNPreeditAttributes = "preeditAttributes";
		// XIMStatusCallbacks delegate names.
		public const string XNStatusStartCallback = "statusStartCallback";
		public const string XNStatusDoneCallback = "statusDoneCallback";
		public const string XNStatusDrawCallback = "statusDrawCallback";
		public const string XNStatusAttributes = "statusAttributes";

		public const string XNArea = "area";
		public const string XNAreaNeeded = "areaNeeded";
		public const string XNSpotLocation = "spotLocation";
		public const string XNFontSet = "fontSet";
	}

	internal unsafe struct XRRMonitorInfo
	{
		public IntPtr Name;
		public int Primary;
		public int Automatic;
		public int NOutput;
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public int MWidth;
		public int MHeight;
		public IntPtr* Outputs;
	}
}
