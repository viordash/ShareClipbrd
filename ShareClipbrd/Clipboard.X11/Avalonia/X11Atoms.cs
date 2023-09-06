
using System;
using System.Collections.Generic;
using System.Linq;
using static Avalonia.X11.XLib;

#pragma warning disable 649

namespace Avalonia.X11
{

    internal partial class X11Atoms
    {
        private readonly IntPtr _display;
        // Our atoms
        public IntPtr AnyPropertyType;
        public IntPtr XA_PRIMARY = (IntPtr)1;

        public IntPtr XA_ATOM = (IntPtr)4;

        public IntPtr XA_STRING = (IntPtr)31;

        public IntPtr CLIPBOARD;
        public IntPtr CLIPBOARD_MANAGER;
        public IntPtr SAVE_TARGETS;
        public IntPtr MULTIPLE;

        public IntPtr OEMTEXT;
        public IntPtr UNICODETEXT;
        public IntPtr TARGETS;
        public IntPtr UTF8_STRING;
        public IntPtr UTF16_STRING;
        public IntPtr ATOM_PAIR;

        public IntPtr INCR;

        private readonly Dictionary<string, IntPtr> _namesToAtoms = new Dictionary<string, IntPtr>();
        private readonly Dictionary<IntPtr, string> _atomsToNames = new Dictionary<IntPtr, string>();
        public X11Atoms(IntPtr display)
        {
            _display = display;
            PopulateAtoms(display);
        }

        private void InitAtom(ref IntPtr field, string name, IntPtr value)
        {
            if (value != IntPtr.Zero)
            {
                field = value;
                _namesToAtoms[name] = value;
                _atomsToNames[value] = name;
            }
        }

        public IntPtr GetAtom(string name)
        {
            if (_namesToAtoms.TryGetValue(name, out var rv))
                return rv;
            var atom = XInternAtom(_display, name, false);
            _namesToAtoms[name] = atom;
            _atomsToNames[atom] = name;
            return atom;
        }

        public string? GetAtomName(IntPtr atom)
        {
            if (_atomsToNames.TryGetValue(atom, out var rv))
                return rv;
            var name = XLib.GetAtomName(_display, atom);
            if (name == null)
                return null;
            _atomsToNames[atom] = name;
            _namesToAtoms[name] = atom;
            return name;
        }


        private void PopulateAtoms(IntPtr display)
        {
            var atomNames = new string[] {
                "AnyPropertyType",
                "XA_PRIMARY",

                "XA_ATOM",

                "XA_STRING",

                "CLIPBOARD",
                "CLIPBOARD_MANAGER",
                "SAVE_TARGETS",
                "MULTIPLE",

                "OEMTEXT",
                "UNICODETEXT",
                "TARGETS",
                "UTF8_STRING",
                "UTF16_STRING",
                "ATOM_PAIR",

                "INCR",
            };
            var atoms = new IntPtr[atomNames.Length];

            XInternAtoms(display, atomNames, atomNames.Length, true, atoms);
            InitAtom(ref AnyPropertyType, "AnyPropertyType", atoms[0]);
            InitAtom(ref XA_PRIMARY, "XA_PRIMARY", atoms[1]);

            InitAtom(ref XA_ATOM, "XA_ATOM", atoms[2]);

            InitAtom(ref XA_STRING, "XA_STRING", atoms[3]);

            InitAtom(ref CLIPBOARD, "CLIPBOARD", atoms[4]);
            InitAtom(ref CLIPBOARD_MANAGER, "CLIPBOARD_MANAGER", atoms[5]);
            InitAtom(ref SAVE_TARGETS, "SAVE_TARGETS", atoms[6]);
            InitAtom(ref MULTIPLE, "MULTIPLE", atoms[7]);
            InitAtom(ref OEMTEXT, "OEMTEXT", atoms[8]);
            InitAtom(ref UNICODETEXT, "UNICODETEXT", atoms[9]);
            InitAtom(ref TARGETS, "TARGETS", atoms[10]);
            InitAtom(ref UTF8_STRING, "UTF8_STRING", atoms[11]);
            InitAtom(ref UTF16_STRING, "UTF16_STRING", atoms[12]);
            InitAtom(ref ATOM_PAIR, "ATOM_PAIR", atoms[13]);
            InitAtom(ref INCR, "INCR", atoms[14]);
        }
    }
}
