// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
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
// Copyright (c) 2006 Novell, Inc. (https://www.novell.com)
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using static Avalonia.X11.XLib;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable IdentifierTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable CommentTypo
// ReSharper disable ArrangeThisQualifier
// ReSharper disable NotAccessedField.Global
// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo
#pragma warning disable 649

namespace Avalonia.X11 {

    internal partial class X11Atoms {
        private readonly IntPtr _display;

        // Our atoms
        public IntPtr AnyPropertyType = (IntPtr)0;
        public IntPtr XA_PRIMARY = (IntPtr)1;
        public IntPtr XA_SECONDARY = (IntPtr)2;
        public IntPtr XA_ARC = (IntPtr)3;
        public IntPtr XA_ATOM = (IntPtr)4;
        public IntPtr XA_BITMAP = (IntPtr)5;
        public IntPtr XA_CARDINAL = (IntPtr)6;
        public IntPtr XA_COLORMAP = (IntPtr)7;
        public IntPtr XA_CURSOR = (IntPtr)8;
        public IntPtr XA_CUT_BUFFER0 = (IntPtr)9;
        public IntPtr XA_CUT_BUFFER1 = (IntPtr)10;
        public IntPtr XA_CUT_BUFFER2 = (IntPtr)11;
        public IntPtr XA_CUT_BUFFER3 = (IntPtr)12;
        public IntPtr XA_CUT_BUFFER4 = (IntPtr)13;
        public IntPtr XA_CUT_BUFFER5 = (IntPtr)14;
        public IntPtr XA_CUT_BUFFER6 = (IntPtr)15;
        public IntPtr XA_CUT_BUFFER7 = (IntPtr)16;
        public IntPtr XA_DRAWABLE = (IntPtr)17;
        public IntPtr XA_FONT = (IntPtr)18;
        public IntPtr XA_INTEGER = (IntPtr)19;
        public IntPtr XA_PIXMAP = (IntPtr)20;
        public IntPtr XA_POINT = (IntPtr)21;
        public IntPtr XA_RECTANGLE = (IntPtr)22;
        public IntPtr XA_RESOURCE_MANAGER = (IntPtr)23;
        public IntPtr XA_RGB_COLOR_MAP = (IntPtr)24;
        public IntPtr XA_RGB_BEST_MAP = (IntPtr)25;
        public IntPtr XA_RGB_BLUE_MAP = (IntPtr)26;
        public IntPtr XA_RGB_DEFAULT_MAP = (IntPtr)27;
        public IntPtr XA_RGB_GRAY_MAP = (IntPtr)28;
        public IntPtr XA_RGB_GREEN_MAP = (IntPtr)29;
        public IntPtr XA_RGB_RED_MAP = (IntPtr)30;
        public IntPtr XA_STRING = (IntPtr)31;
        public IntPtr XA_VISUALID = (IntPtr)32;
        public IntPtr XA_WINDOW = (IntPtr)33;
        public IntPtr XA_WM_COMMAND = (IntPtr)34;
        public IntPtr XA_WM_HINTS = (IntPtr)35;
        public IntPtr XA_WM_CLIENT_MACHINE = (IntPtr)36;
        public IntPtr XA_WM_ICON_NAME = (IntPtr)37;
        public IntPtr XA_WM_ICON_SIZE = (IntPtr)38;
        public IntPtr XA_WM_NAME = (IntPtr)39;
        public IntPtr XA_WM_NORMAL_HINTS = (IntPtr)40;
        public IntPtr XA_WM_SIZE_HINTS = (IntPtr)41;
        public IntPtr XA_WM_ZOOM_HINTS = (IntPtr)42;
        public IntPtr XA_MIN_SPACE = (IntPtr)43;
        public IntPtr XA_NORM_SPACE = (IntPtr)44;
        public IntPtr XA_MAX_SPACE = (IntPtr)45;
        public IntPtr XA_END_SPACE = (IntPtr)46;
        public IntPtr XA_SUPERSCRIPT_X = (IntPtr)47;
        public IntPtr XA_SUPERSCRIPT_Y = (IntPtr)48;
        public IntPtr XA_SUBSCRIPT_X = (IntPtr)49;
        public IntPtr XA_SUBSCRIPT_Y = (IntPtr)50;
        public IntPtr XA_UNDERLINE_POSITION = (IntPtr)51;
        public IntPtr XA_UNDERLINE_THICKNESS = (IntPtr)52;
        public IntPtr XA_STRIKEOUT_ASCENT = (IntPtr)53;
        public IntPtr XA_STRIKEOUT_DESCENT = (IntPtr)54;
        public IntPtr XA_ITALIC_ANGLE = (IntPtr)55;
        public IntPtr XA_X_HEIGHT = (IntPtr)56;
        public IntPtr XA_QUAD_WIDTH = (IntPtr)57;
        public IntPtr XA_WEIGHT = (IntPtr)58;
        public IntPtr XA_POINT_SIZE = (IntPtr)59;
        public IntPtr XA_RESOLUTION = (IntPtr)60;
        public IntPtr XA_COPYRIGHT = (IntPtr)61;
        public IntPtr XA_NOTICE = (IntPtr)62;
        public IntPtr XA_FONT_NAME = (IntPtr)63;
        public IntPtr XA_FAMILY_NAME = (IntPtr)64;
        public IntPtr XA_FULL_NAME = (IntPtr)65;
        public IntPtr XA_CAP_HEIGHT = (IntPtr)66;
        public IntPtr XA_WM_CLASS = (IntPtr)67;
        public IntPtr XA_WM_TRANSIENT_FOR = (IntPtr)68;

        public IntPtr EDID;

        public IntPtr WM_PROTOCOLS;
        public IntPtr WM_DELETE_WINDOW;
        public IntPtr WM_TAKE_FOCUS;
        public IntPtr _NET_SUPPORTED;
        public IntPtr _NET_CLIENT_LIST;
        public IntPtr _NET_NUMBER_OF_DESKTOPS;
        public IntPtr _NET_DESKTOP_GEOMETRY;
        public IntPtr _NET_DESKTOP_VIEWPORT;
        public IntPtr _NET_CURRENT_DESKTOP;
        public IntPtr _NET_DESKTOP_NAMES;
        public IntPtr _NET_ACTIVE_WINDOW;
        public IntPtr _NET_WORKAREA;
        public IntPtr _NET_SUPPORTING_WM_CHECK;
        public IntPtr _NET_VIRTUAL_ROOTS;
        public IntPtr _NET_DESKTOP_LAYOUT;
        public IntPtr _NET_SHOWING_DESKTOP;
        public IntPtr _NET_CLOSE_WINDOW;
        public IntPtr _NET_MOVERESIZE_WINDOW;
        public IntPtr _NET_WM_MOVERESIZE;
        public IntPtr _NET_RESTACK_WINDOW;
        public IntPtr _NET_REQUEST_FRAME_EXTENTS;
        public IntPtr _NET_WM_NAME;
        public IntPtr _NET_WM_VISIBLE_NAME;
        public IntPtr _NET_WM_ICON_NAME;
        public IntPtr _NET_WM_VISIBLE_ICON_NAME;
        public IntPtr _NET_WM_DESKTOP;
        public IntPtr _NET_WM_WINDOW_TYPE;
        public IntPtr _NET_WM_STATE;
        public IntPtr _NET_WM_ALLOWED_ACTIONS;
        public IntPtr _NET_WM_STRUT;
        public IntPtr _NET_WM_STRUT_PARTIAL;
        public IntPtr _NET_WM_ICON_GEOMETRY;
        public IntPtr _NET_WM_ICON;
        public IntPtr _NET_WM_PID;
        public IntPtr _NET_WM_HANDLED_ICONS;
        public IntPtr _NET_WM_USER_TIME;
        public IntPtr _NET_FRAME_EXTENTS;
        public IntPtr _NET_WM_PING;
        public IntPtr _NET_WM_SYNC_REQUEST;
        public IntPtr _NET_WM_SYNC_REQUEST_COUNTER;
        public IntPtr _NET_SYSTEM_TRAY_S;
        public IntPtr _NET_SYSTEM_TRAY_ORIENTATION;
        public IntPtr _NET_SYSTEM_TRAY_OPCODE;
        public IntPtr _NET_WM_STATE_MAXIMIZED_HORZ;
        public IntPtr _NET_WM_STATE_MAXIMIZED_VERT;
        public IntPtr _NET_WM_STATE_FULLSCREEN;
        public IntPtr _XEMBED;
        public IntPtr _XEMBED_INFO;
        public IntPtr _MOTIF_WM_HINTS;
        public IntPtr _NET_WM_STATE_SKIP_TASKBAR;
        public IntPtr _NET_WM_STATE_ABOVE;
        public IntPtr _NET_WM_STATE_MODAL;
        public IntPtr _NET_WM_STATE_HIDDEN;
        public IntPtr _NET_WM_CONTEXT_HELP;
        public IntPtr _NET_WM_WINDOW_OPACITY;
        public IntPtr _NET_WM_WINDOW_TYPE_DESKTOP;
        public IntPtr _NET_WM_WINDOW_TYPE_DOCK;
        public IntPtr _NET_WM_WINDOW_TYPE_TOOLBAR;
        public IntPtr _NET_WM_WINDOW_TYPE_MENU;
        public IntPtr _NET_WM_WINDOW_TYPE_UTILITY;
        public IntPtr _NET_WM_WINDOW_TYPE_SPLASH;
        public IntPtr _NET_WM_WINDOW_TYPE_DIALOG;
        public IntPtr _NET_WM_WINDOW_TYPE_NORMAL;
        public IntPtr CLIPBOARD;
        public IntPtr CLIPBOARD_MANAGER;
        public IntPtr SAVE_TARGETS;
        public IntPtr MULTIPLE;
        public IntPtr PRIMARY;
        public IntPtr OEMTEXT;
        public IntPtr UNICODETEXT;
        public IntPtr TARGETS;
        public IntPtr UTF8_STRING;
        public IntPtr UTF16_STRING;
        public IntPtr ATOM_PAIR;
        public IntPtr MANAGER;
        public IntPtr _KDE_NET_WM_BLUR_BEHIND_REGION;
        public IntPtr INCR;

        private readonly Dictionary<string, IntPtr> _namesToAtoms = new Dictionary<string, IntPtr>();
        private readonly Dictionary<IntPtr, string> _atomsToNames = new Dictionary<IntPtr, string>();
        public X11Atoms(IntPtr display) {
            _display = display;
            PopulateAtoms(display);
        }

        private void InitAtom(ref IntPtr field, string name, IntPtr value) {
            if(value != IntPtr.Zero) {
                field = value;
                _namesToAtoms[name] = value;
                _atomsToNames[value] = name;
            }
        }

        public IntPtr GetAtom(string name) {
            if(_namesToAtoms.TryGetValue(name, out var rv))
                return rv;
            var atom = XInternAtom(_display, name, false);
            _namesToAtoms[name] = atom;
            _atomsToNames[atom] = name;
            return atom;
        }

        public string GetAtomName(IntPtr atom) {
            if(_atomsToNames.TryGetValue(atom, out var rv))
                return rv;
            var name = XLib.GetAtomName(_display, atom);
            if(name == null)
                return null;
            _atomsToNames[atom] = name;
            _namesToAtoms[name] = atom;
            return name;
        }


        private void PopulateAtoms(IntPtr display) {
            var atoms = new IntPtr[147];
            var atomNames = new string[147] {
            "AnyPropertyType",
            "XA_PRIMARY",
            "XA_SECONDARY",
            "XA_ARC",
            "XA_ATOM",
            "XA_BITMAP",
            "XA_CARDINAL",
            "XA_COLORMAP",
            "XA_CURSOR",
            "XA_CUT_BUFFER0",
            "XA_CUT_BUFFER1",
            "XA_CUT_BUFFER2",
            "XA_CUT_BUFFER3",
            "XA_CUT_BUFFER4",
            "XA_CUT_BUFFER5",
            "XA_CUT_BUFFER6",
            "XA_CUT_BUFFER7",
            "XA_DRAWABLE",
            "XA_FONT",
            "XA_INTEGER",
            "XA_PIXMAP",
            "XA_POINT",
            "XA_RECTANGLE",
            "XA_RESOURCE_MANAGER",
            "XA_RGB_COLOR_MAP",
            "XA_RGB_BEST_MAP",
            "XA_RGB_BLUE_MAP",
            "XA_RGB_DEFAULT_MAP",
            "XA_RGB_GRAY_MAP",
            "XA_RGB_GREEN_MAP",
            "XA_RGB_RED_MAP",
            "XA_STRING",
            "XA_VISUALID",
            "XA_WINDOW",
            "XA_WM_COMMAND",
            "XA_WM_HINTS",
            "XA_WM_CLIENT_MACHINE",
            "XA_WM_ICON_NAME",
            "XA_WM_ICON_SIZE",
            "XA_WM_NAME",
            "XA_WM_NORMAL_HINTS",
            "XA_WM_SIZE_HINTS",
            "XA_WM_ZOOM_HINTS",
            "XA_MIN_SPACE",
            "XA_NORM_SPACE",
            "XA_MAX_SPACE",
            "XA_END_SPACE",
            "XA_SUPERSCRIPT_X",
            "XA_SUPERSCRIPT_Y",
            "XA_SUBSCRIPT_X",
            "XA_SUBSCRIPT_Y",
            "XA_UNDERLINE_POSITION",
            "XA_UNDERLINE_THICKNESS",
            "XA_STRIKEOUT_ASCENT",
            "XA_STRIKEOUT_DESCENT",
            "XA_ITALIC_ANGLE",
            "XA_X_HEIGHT",
            "XA_QUAD_WIDTH",
            "XA_WEIGHT",
            "XA_POINT_SIZE",
            "XA_RESOLUTION",
            "XA_COPYRIGHT",
            "XA_NOTICE",
            "XA_FONT_NAME",
            "XA_FAMILY_NAME",
            "XA_FULL_NAME",
            "XA_CAP_HEIGHT",
            "XA_WM_CLASS",
            "XA_WM_TRANSIENT_FOR",
            "EDID",
            "WM_PROTOCOLS",
            "WM_DELETE_WINDOW",
            "WM_TAKE_FOCUS",
            "_NET_SUPPORTED",
            "_NET_CLIENT_LIST",
            "_NET_NUMBER_OF_DESKTOPS",
            "_NET_DESKTOP_GEOMETRY",
            "_NET_DESKTOP_VIEWPORT",
            "_NET_CURRENT_DESKTOP",
            "_NET_DESKTOP_NAMES",
            "_NET_ACTIVE_WINDOW",
            "_NET_WORKAREA",
            "_NET_SUPPORTING_WM_CHECK",
            "_NET_VIRTUAL_ROOTS",
            "_NET_DESKTOP_LAYOUT",
            "_NET_SHOWING_DESKTOP",
            "_NET_CLOSE_WINDOW",
            "_NET_MOVERESIZE_WINDOW",
            "_NET_WM_MOVERESIZE",
            "_NET_RESTACK_WINDOW",
            "_NET_REQUEST_FRAME_EXTENTS",
            "_NET_WM_NAME",
            "_NET_WM_VISIBLE_NAME",
            "_NET_WM_ICON_NAME",
            "_NET_WM_VISIBLE_ICON_NAME",
            "_NET_WM_DESKTOP",
            "_NET_WM_WINDOW_TYPE",
            "_NET_WM_STATE",
            "_NET_WM_ALLOWED_ACTIONS",
            "_NET_WM_STRUT",
            "_NET_WM_STRUT_PARTIAL",
            "_NET_WM_ICON_GEOMETRY",
            "_NET_WM_ICON",
            "_NET_WM_PID",
            "_NET_WM_HANDLED_ICONS",
            "_NET_WM_USER_TIME",
            "_NET_FRAME_EXTENTS",
            "_NET_WM_PING",
            "_NET_WM_SYNC_REQUEST",
            "_NET_WM_SYNC_REQUEST_COUNTER",
            "_NET_SYSTEM_TRAY_S",
            "_NET_SYSTEM_TRAY_ORIENTATION",
            "_NET_SYSTEM_TRAY_OPCODE",
            "_NET_WM_STATE_MAXIMIZED_HORZ",
            "_NET_WM_STATE_MAXIMIZED_VERT",
            "_NET_WM_STATE_FULLSCREEN",
            "_XEMBED",
            "_XEMBED_INFO",
            "_MOTIF_WM_HINTS",
            "_NET_WM_STATE_SKIP_TASKBAR",
            "_NET_WM_STATE_ABOVE",
            "_NET_WM_STATE_MODAL",
            "_NET_WM_STATE_HIDDEN",
            "_NET_WM_CONTEXT_HELP",
            "_NET_WM_WINDOW_OPACITY",
            "_NET_WM_WINDOW_TYPE_DESKTOP",
            "_NET_WM_WINDOW_TYPE_DOCK",
            "_NET_WM_WINDOW_TYPE_TOOLBAR",
            "_NET_WM_WINDOW_TYPE_MENU",
            "_NET_WM_WINDOW_TYPE_UTILITY",
            "_NET_WM_WINDOW_TYPE_SPLASH",
            "_NET_WM_WINDOW_TYPE_DIALOG",
            "_NET_WM_WINDOW_TYPE_NORMAL",
            "CLIPBOARD",
            "CLIPBOARD_MANAGER",
            "SAVE_TARGETS",
            "MULTIPLE",
            "PRIMARY",
            "OEMTEXT",
            "UNICODETEXT",
            "TARGETS",
            "UTF8_STRING",
            "UTF16_STRING",
            "ATOM_PAIR",
            "MANAGER",
            "_KDE_NET_WM_BLUR_BEHIND_REGION",
            "INCR",
        };
            XInternAtoms(display, atomNames, atomNames.Length, true, atoms);
            InitAtom(ref AnyPropertyType, "AnyPropertyType", atoms[0]);
            InitAtom(ref XA_PRIMARY, "XA_PRIMARY", atoms[1]);
            InitAtom(ref XA_SECONDARY, "XA_SECONDARY", atoms[2]);
            InitAtom(ref XA_ARC, "XA_ARC", atoms[3]);
            InitAtom(ref XA_ATOM, "XA_ATOM", atoms[4]);
            InitAtom(ref XA_BITMAP, "XA_BITMAP", atoms[5]);
            InitAtom(ref XA_CARDINAL, "XA_CARDINAL", atoms[6]);
            InitAtom(ref XA_COLORMAP, "XA_COLORMAP", atoms[7]);
            InitAtom(ref XA_CURSOR, "XA_CURSOR", atoms[8]);
            InitAtom(ref XA_CUT_BUFFER0, "XA_CUT_BUFFER0", atoms[9]);
            InitAtom(ref XA_CUT_BUFFER1, "XA_CUT_BUFFER1", atoms[10]);
            InitAtom(ref XA_CUT_BUFFER2, "XA_CUT_BUFFER2", atoms[11]);
            InitAtom(ref XA_CUT_BUFFER3, "XA_CUT_BUFFER3", atoms[12]);
            InitAtom(ref XA_CUT_BUFFER4, "XA_CUT_BUFFER4", atoms[13]);
            InitAtom(ref XA_CUT_BUFFER5, "XA_CUT_BUFFER5", atoms[14]);
            InitAtom(ref XA_CUT_BUFFER6, "XA_CUT_BUFFER6", atoms[15]);
            InitAtom(ref XA_CUT_BUFFER7, "XA_CUT_BUFFER7", atoms[16]);
            InitAtom(ref XA_DRAWABLE, "XA_DRAWABLE", atoms[17]);
            InitAtom(ref XA_FONT, "XA_FONT", atoms[18]);
            InitAtom(ref XA_INTEGER, "XA_INTEGER", atoms[19]);
            InitAtom(ref XA_PIXMAP, "XA_PIXMAP", atoms[20]);
            InitAtom(ref XA_POINT, "XA_POINT", atoms[21]);
            InitAtom(ref XA_RECTANGLE, "XA_RECTANGLE", atoms[22]);
            InitAtom(ref XA_RESOURCE_MANAGER, "XA_RESOURCE_MANAGER", atoms[23]);
            InitAtom(ref XA_RGB_COLOR_MAP, "XA_RGB_COLOR_MAP", atoms[24]);
            InitAtom(ref XA_RGB_BEST_MAP, "XA_RGB_BEST_MAP", atoms[25]);
            InitAtom(ref XA_RGB_BLUE_MAP, "XA_RGB_BLUE_MAP", atoms[26]);
            InitAtom(ref XA_RGB_DEFAULT_MAP, "XA_RGB_DEFAULT_MAP", atoms[27]);
            InitAtom(ref XA_RGB_GRAY_MAP, "XA_RGB_GRAY_MAP", atoms[28]);
            InitAtom(ref XA_RGB_GREEN_MAP, "XA_RGB_GREEN_MAP", atoms[29]);
            InitAtom(ref XA_RGB_RED_MAP, "XA_RGB_RED_MAP", atoms[30]);
            InitAtom(ref XA_STRING, "XA_STRING", atoms[31]);
            InitAtom(ref XA_VISUALID, "XA_VISUALID", atoms[32]);
            InitAtom(ref XA_WINDOW, "XA_WINDOW", atoms[33]);
            InitAtom(ref XA_WM_COMMAND, "XA_WM_COMMAND", atoms[34]);
            InitAtom(ref XA_WM_HINTS, "XA_WM_HINTS", atoms[35]);
            InitAtom(ref XA_WM_CLIENT_MACHINE, "XA_WM_CLIENT_MACHINE", atoms[36]);
            InitAtom(ref XA_WM_ICON_NAME, "XA_WM_ICON_NAME", atoms[37]);
            InitAtom(ref XA_WM_ICON_SIZE, "XA_WM_ICON_SIZE", atoms[38]);
            InitAtom(ref XA_WM_NAME, "XA_WM_NAME", atoms[39]);
            InitAtom(ref XA_WM_NORMAL_HINTS, "XA_WM_NORMAL_HINTS", atoms[40]);
            InitAtom(ref XA_WM_SIZE_HINTS, "XA_WM_SIZE_HINTS", atoms[41]);
            InitAtom(ref XA_WM_ZOOM_HINTS, "XA_WM_ZOOM_HINTS", atoms[42]);
            InitAtom(ref XA_MIN_SPACE, "XA_MIN_SPACE", atoms[43]);
            InitAtom(ref XA_NORM_SPACE, "XA_NORM_SPACE", atoms[44]);
            InitAtom(ref XA_MAX_SPACE, "XA_MAX_SPACE", atoms[45]);
            InitAtom(ref XA_END_SPACE, "XA_END_SPACE", atoms[46]);
            InitAtom(ref XA_SUPERSCRIPT_X, "XA_SUPERSCRIPT_X", atoms[47]);
            InitAtom(ref XA_SUPERSCRIPT_Y, "XA_SUPERSCRIPT_Y", atoms[48]);
            InitAtom(ref XA_SUBSCRIPT_X, "XA_SUBSCRIPT_X", atoms[49]);
            InitAtom(ref XA_SUBSCRIPT_Y, "XA_SUBSCRIPT_Y", atoms[50]);
            InitAtom(ref XA_UNDERLINE_POSITION, "XA_UNDERLINE_POSITION", atoms[51]);
            InitAtom(ref XA_UNDERLINE_THICKNESS, "XA_UNDERLINE_THICKNESS", atoms[52]);
            InitAtom(ref XA_STRIKEOUT_ASCENT, "XA_STRIKEOUT_ASCENT", atoms[53]);
            InitAtom(ref XA_STRIKEOUT_DESCENT, "XA_STRIKEOUT_DESCENT", atoms[54]);
            InitAtom(ref XA_ITALIC_ANGLE, "XA_ITALIC_ANGLE", atoms[55]);
            InitAtom(ref XA_X_HEIGHT, "XA_X_HEIGHT", atoms[56]);
            InitAtom(ref XA_QUAD_WIDTH, "XA_QUAD_WIDTH", atoms[57]);
            InitAtom(ref XA_WEIGHT, "XA_WEIGHT", atoms[58]);
            InitAtom(ref XA_POINT_SIZE, "XA_POINT_SIZE", atoms[59]);
            InitAtom(ref XA_RESOLUTION, "XA_RESOLUTION", atoms[60]);
            InitAtom(ref XA_COPYRIGHT, "XA_COPYRIGHT", atoms[61]);
            InitAtom(ref XA_NOTICE, "XA_NOTICE", atoms[62]);
            InitAtom(ref XA_FONT_NAME, "XA_FONT_NAME", atoms[63]);
            InitAtom(ref XA_FAMILY_NAME, "XA_FAMILY_NAME", atoms[64]);
            InitAtom(ref XA_FULL_NAME, "XA_FULL_NAME", atoms[65]);
            InitAtom(ref XA_CAP_HEIGHT, "XA_CAP_HEIGHT", atoms[66]);
            InitAtom(ref XA_WM_CLASS, "XA_WM_CLASS", atoms[67]);
            InitAtom(ref XA_WM_TRANSIENT_FOR, "XA_WM_TRANSIENT_FOR", atoms[68]);
            InitAtom(ref EDID, "EDID", atoms[69]);
            InitAtom(ref WM_PROTOCOLS, "WM_PROTOCOLS", atoms[70]);
            InitAtom(ref WM_DELETE_WINDOW, "WM_DELETE_WINDOW", atoms[71]);
            InitAtom(ref WM_TAKE_FOCUS, "WM_TAKE_FOCUS", atoms[72]);
            InitAtom(ref _NET_SUPPORTED, "_NET_SUPPORTED", atoms[73]);
            InitAtom(ref _NET_CLIENT_LIST, "_NET_CLIENT_LIST", atoms[74]);
            InitAtom(ref _NET_NUMBER_OF_DESKTOPS, "_NET_NUMBER_OF_DESKTOPS", atoms[75]);
            InitAtom(ref _NET_DESKTOP_GEOMETRY, "_NET_DESKTOP_GEOMETRY", atoms[76]);
            InitAtom(ref _NET_DESKTOP_VIEWPORT, "_NET_DESKTOP_VIEWPORT", atoms[77]);
            InitAtom(ref _NET_CURRENT_DESKTOP, "_NET_CURRENT_DESKTOP", atoms[78]);
            InitAtom(ref _NET_DESKTOP_NAMES, "_NET_DESKTOP_NAMES", atoms[79]);
            InitAtom(ref _NET_ACTIVE_WINDOW, "_NET_ACTIVE_WINDOW", atoms[80]);
            InitAtom(ref _NET_WORKAREA, "_NET_WORKAREA", atoms[81]);
            InitAtom(ref _NET_SUPPORTING_WM_CHECK, "_NET_SUPPORTING_WM_CHECK", atoms[82]);
            InitAtom(ref _NET_VIRTUAL_ROOTS, "_NET_VIRTUAL_ROOTS", atoms[83]);
            InitAtom(ref _NET_DESKTOP_LAYOUT, "_NET_DESKTOP_LAYOUT", atoms[84]);
            InitAtom(ref _NET_SHOWING_DESKTOP, "_NET_SHOWING_DESKTOP", atoms[85]);
            InitAtom(ref _NET_CLOSE_WINDOW, "_NET_CLOSE_WINDOW", atoms[86]);
            InitAtom(ref _NET_MOVERESIZE_WINDOW, "_NET_MOVERESIZE_WINDOW", atoms[87]);
            InitAtom(ref _NET_WM_MOVERESIZE, "_NET_WM_MOVERESIZE", atoms[88]);
            InitAtom(ref _NET_RESTACK_WINDOW, "_NET_RESTACK_WINDOW", atoms[89]);
            InitAtom(ref _NET_REQUEST_FRAME_EXTENTS, "_NET_REQUEST_FRAME_EXTENTS", atoms[90]);
            InitAtom(ref _NET_WM_NAME, "_NET_WM_NAME", atoms[91]);
            InitAtom(ref _NET_WM_VISIBLE_NAME, "_NET_WM_VISIBLE_NAME", atoms[92]);
            InitAtom(ref _NET_WM_ICON_NAME, "_NET_WM_ICON_NAME", atoms[93]);
            InitAtom(ref _NET_WM_VISIBLE_ICON_NAME, "_NET_WM_VISIBLE_ICON_NAME", atoms[94]);
            InitAtom(ref _NET_WM_DESKTOP, "_NET_WM_DESKTOP", atoms[95]);
            InitAtom(ref _NET_WM_WINDOW_TYPE, "_NET_WM_WINDOW_TYPE", atoms[96]);
            InitAtom(ref _NET_WM_STATE, "_NET_WM_STATE", atoms[97]);
            InitAtom(ref _NET_WM_ALLOWED_ACTIONS, "_NET_WM_ALLOWED_ACTIONS", atoms[98]);
            InitAtom(ref _NET_WM_STRUT, "_NET_WM_STRUT", atoms[99]);
            InitAtom(ref _NET_WM_STRUT_PARTIAL, "_NET_WM_STRUT_PARTIAL", atoms[100]);
            InitAtom(ref _NET_WM_ICON_GEOMETRY, "_NET_WM_ICON_GEOMETRY", atoms[101]);
            InitAtom(ref _NET_WM_ICON, "_NET_WM_ICON", atoms[102]);
            InitAtom(ref _NET_WM_PID, "_NET_WM_PID", atoms[103]);
            InitAtom(ref _NET_WM_HANDLED_ICONS, "_NET_WM_HANDLED_ICONS", atoms[104]);
            InitAtom(ref _NET_WM_USER_TIME, "_NET_WM_USER_TIME", atoms[105]);
            InitAtom(ref _NET_FRAME_EXTENTS, "_NET_FRAME_EXTENTS", atoms[106]);
            InitAtom(ref _NET_WM_PING, "_NET_WM_PING", atoms[107]);
            InitAtom(ref _NET_WM_SYNC_REQUEST, "_NET_WM_SYNC_REQUEST", atoms[108]);
            InitAtom(ref _NET_WM_SYNC_REQUEST_COUNTER, "_NET_WM_SYNC_REQUEST_COUNTER", atoms[109]);
            InitAtom(ref _NET_SYSTEM_TRAY_S, "_NET_SYSTEM_TRAY_S", atoms[110]);
            InitAtom(ref _NET_SYSTEM_TRAY_ORIENTATION, "_NET_SYSTEM_TRAY_ORIENTATION", atoms[111]);
            InitAtom(ref _NET_SYSTEM_TRAY_OPCODE, "_NET_SYSTEM_TRAY_OPCODE", atoms[112]);
            InitAtom(ref _NET_WM_STATE_MAXIMIZED_HORZ, "_NET_WM_STATE_MAXIMIZED_HORZ", atoms[113]);
            InitAtom(ref _NET_WM_STATE_MAXIMIZED_VERT, "_NET_WM_STATE_MAXIMIZED_VERT", atoms[114]);
            InitAtom(ref _NET_WM_STATE_FULLSCREEN, "_NET_WM_STATE_FULLSCREEN", atoms[115]);
            InitAtom(ref _XEMBED, "_XEMBED", atoms[116]);
            InitAtom(ref _XEMBED_INFO, "_XEMBED_INFO", atoms[117]);
            InitAtom(ref _MOTIF_WM_HINTS, "_MOTIF_WM_HINTS", atoms[118]);
            InitAtom(ref _NET_WM_STATE_SKIP_TASKBAR, "_NET_WM_STATE_SKIP_TASKBAR", atoms[119]);
            InitAtom(ref _NET_WM_STATE_ABOVE, "_NET_WM_STATE_ABOVE", atoms[120]);
            InitAtom(ref _NET_WM_STATE_MODAL, "_NET_WM_STATE_MODAL", atoms[121]);
            InitAtom(ref _NET_WM_STATE_HIDDEN, "_NET_WM_STATE_HIDDEN", atoms[122]);
            InitAtom(ref _NET_WM_CONTEXT_HELP, "_NET_WM_CONTEXT_HELP", atoms[123]);
            InitAtom(ref _NET_WM_WINDOW_OPACITY, "_NET_WM_WINDOW_OPACITY", atoms[124]);
            InitAtom(ref _NET_WM_WINDOW_TYPE_DESKTOP, "_NET_WM_WINDOW_TYPE_DESKTOP", atoms[125]);
            InitAtom(ref _NET_WM_WINDOW_TYPE_DOCK, "_NET_WM_WINDOW_TYPE_DOCK", atoms[126]);
            InitAtom(ref _NET_WM_WINDOW_TYPE_TOOLBAR, "_NET_WM_WINDOW_TYPE_TOOLBAR", atoms[127]);
            InitAtom(ref _NET_WM_WINDOW_TYPE_MENU, "_NET_WM_WINDOW_TYPE_MENU", atoms[128]);
            InitAtom(ref _NET_WM_WINDOW_TYPE_UTILITY, "_NET_WM_WINDOW_TYPE_UTILITY", atoms[129]);
            InitAtom(ref _NET_WM_WINDOW_TYPE_SPLASH, "_NET_WM_WINDOW_TYPE_SPLASH", atoms[130]);
            InitAtom(ref _NET_WM_WINDOW_TYPE_DIALOG, "_NET_WM_WINDOW_TYPE_DIALOG", atoms[131]);
            InitAtom(ref _NET_WM_WINDOW_TYPE_NORMAL, "_NET_WM_WINDOW_TYPE_NORMAL", atoms[132]);
            InitAtom(ref CLIPBOARD, "CLIPBOARD", atoms[133]);
            InitAtom(ref CLIPBOARD_MANAGER, "CLIPBOARD_MANAGER", atoms[134]);
            InitAtom(ref SAVE_TARGETS, "SAVE_TARGETS", atoms[135]);
            InitAtom(ref MULTIPLE, "MULTIPLE", atoms[136]);
            InitAtom(ref PRIMARY, "PRIMARY", atoms[137]);
            InitAtom(ref OEMTEXT, "OEMTEXT", atoms[138]);
            InitAtom(ref UNICODETEXT, "UNICODETEXT", atoms[139]);
            InitAtom(ref TARGETS, "TARGETS", atoms[140]);
            InitAtom(ref UTF8_STRING, "UTF8_STRING", atoms[141]);
            InitAtom(ref UTF16_STRING, "UTF16_STRING", atoms[142]);
            InitAtom(ref ATOM_PAIR, "ATOM_PAIR", atoms[143]);
            InitAtom(ref MANAGER, "MANAGER", atoms[144]);
            InitAtom(ref _KDE_NET_WM_BLUR_BEHIND_REGION, "_KDE_NET_WM_BLUR_BEHIND_REGION", atoms[145]);
            InitAtom(ref INCR, "INCR", atoms[146]);
        }
    }
}
