using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Input;
using static Avalonia.X11.XLib;
namespace Avalonia.X11
{
    internal class X11Clipboard : IDisposable
    {
        #region inner classes
        private class IncrDataReader
        {
            private readonly X11Atoms _atoms;
            public readonly IntPtr Property;
            private readonly int _total;
            private readonly Action<IntPtr, object?> _onCompleted;
            private readonly List<byte> _readData;

            public IncrDataReader(X11Atoms atoms, IntPtr property, int total, Action<IntPtr, object?> onCompleted)
            {
                _atoms = atoms;
                Property = property;
                _total = total;
                _onCompleted = onCompleted;
                _readData = new List<byte>();
            }

            public void Append(IntPtr data, int size)
            {
                if (size > 0)
                {
                    var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(size);
                    Marshal.Copy(data, buffer, 0, size);
                    _readData.AddRange(buffer.Take(size));
                    System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
                    return;
                }

                if (_readData.Count != _total)
                {
                    _onCompleted(Property, null);
                    return;
                }

                var textEnc = GetStringEncoding(_atoms, Property);
                var bytes = _readData.ToArray();
                if (textEnc != null)
                {
                    _onCompleted(Property, textEnc.GetString(bytes));
                }
                else
                {
                    _onCompleted(Property, bytes);
                }
            }
        }

        private class IncrDataWriter
        {
            private readonly IntPtr _target;
            private readonly Action<IntPtr> _onCompleted;
            private byte[] _data;

            public IncrDataWriter(IntPtr target, byte[] data, Action<IntPtr> onCompleted)
            {
                _target = target;
                _data = data;
                _onCompleted = onCompleted;
            }

            public void OnEvent(ref XEvent ev)
            {
                if (ev.type == XEventName.PropertyNotify && (PropertyState)ev.PropertyEvent.state == PropertyState.Delete)
                {
                    if (_data?.Length > 0)
                    {
                        var bytes = _data.Take(MaxRequestSize).ToArray();
                        _data = _data.Skip(bytes.Length).ToArray();
                        XChangeProperty(ev.PropertyEvent.display, ev.PropertyEvent.window, ev.PropertyEvent.atom, _target, 8, PropertyMode.Replace, bytes, bytes.Length);
                        return;
                    }

                    XChangeProperty(ev.PropertyEvent.display, ev.PropertyEvent.window, ev.PropertyEvent.atom, _target, 8, PropertyMode.Replace, IntPtr.Zero, 0);
                    _onCompleted(ev.PropertyEvent.window);
                }
            }
        }
        #endregion

        private IDataObject? _storedDataObject;
        private IntPtr _handle;
        private TaskCompletionSource<bool>? _storeAtomTcs;
        private readonly List<IntPtr> _requestedFormats;
        private TaskCompletionSource<object?>? _requestedDataTcs;
        private readonly IntPtr _avaloniaSaveTargetsAtom;

        private readonly IntPtr _display;
        private readonly X11Atoms _atoms;

        private const int MaxRequestSize = 0x40000;
        private readonly Dictionary<IntPtr, IncrDataReader> _incrDataReaders;

        private readonly CancellationTokenSource _cts;

        public X11Clipboard()
        {
            _display = XOpenDisplay(IntPtr.Zero);
            if (_display == IntPtr.Zero)
            {
                throw new Exception("XOpenDisplay failed");
            }
            _atoms = new X11Atoms(_display);

            _handle = XCreateSimpleWindow(_display, XDefaultRootWindow(_display),
                    0, 0, 1, 1, 0, IntPtr.Zero, IntPtr.Zero);

            XSelectInput(_display, _handle, new IntPtr((int)(EventMask.StructureNotifyMask | EventMask.PropertyChangeMask)));

            _avaloniaSaveTargetsAtom = XInternAtom(_display, "AVALONIA_SAVE_TARGETS_PROPERTY_ATOM", false);

            _incrDataReaders = new();

            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            _requestedFormats = new List<IntPtr>();
        }

        public void Dispose()
        {
            XCloseDisplay(_display);
        }

        private static Encoding? GetStringEncoding(X11Atoms atoms, IntPtr atom)
        {
            return (atom == atoms.XA_STRING
                    || atom == atoms.OEMTEXT)
                ? Encoding.ASCII
                : atom == atoms.UTF8_STRING
                    ? Encoding.UTF8
                    : atom == atoms.UTF16_STRING
                        ? Encoding.Unicode
                        : null;
        }

        private unsafe void OnEvent(ref XEvent ev)
        {
            System.Diagnostics.Debug.WriteLine($"--------- X11Clipboard.OnEvent {ev}");
            if (ev.type == XEventName.SelectionClear)
            {
                System.Diagnostics.Debug.WriteLine("--- XEventName.SelectionClear");
                _storeAtomTcs?.TrySetResult(true);
                return;
            }

            if (ev.type == XEventName.SelectionRequest)
            {
                var sel = ev.SelectionRequestEvent;
                var resp = new XEvent
                {
                    SelectionEvent =
                    {
                        type = XEventName.SelectionNotify,
                        send_event = 1,
                        display = _display,
                        selection = sel.selection,
                        target = sel.target,
                        requestor = sel.requestor,
                        time = sel.time,
                        property = IntPtr.Zero
                    }
                };
                if (sel.selection == _atoms.CLIPBOARD)
                {
                    resp.SelectionEvent.property = WriteTargetToProperty(sel.target, sel.requestor, sel.property);
                }

                XSendEvent(_display, sel.requestor, false, new IntPtr((int)EventMask.NoEventMask), ref resp);
                return;
            }

            if (ev.type == XEventName.SelectionNotify && ev.SelectionEvent.selection == _atoms.CLIPBOARD)
            {
                var sel = ev.SelectionEvent;
                if (sel.property == IntPtr.Zero)
                {
                    _requestedDataTcs?.TrySetResult(null);
                    return;
                }
                XGetWindowProperty(_display, _handle, sel.property, IntPtr.Zero, new IntPtr(0x7fffffff), true, (IntPtr)Atom.AnyPropertyType,
                    out var actualTypeAtom, out var actualFormat, out var nitems, out var bytes_after, out var prop);
                Encoding? textEnc = null;
                if (nitems == IntPtr.Zero)
                {
                    _requestedDataTcs?.TrySetResult(null);
                }
                else
                {
                    if (sel.property == _atoms.TARGETS)
                    {
                        if (actualFormat == 32)
                        {
                            var formatsArr = new IntPtr[nitems.ToInt32()];
                            Marshal.Copy(prop, formatsArr, 0, formatsArr.Length);
                            _requestedFormats.AddRange(formatsArr);
                            System.Diagnostics.Debug.WriteLine("----------- _requestedFormatsTcs?.TrySetResult(formats) 0");
                        }
                    }
                    else if ((textEnc = GetStringEncoding(_atoms, actualTypeAtom)) != null)
                    {
                        var text = textEnc.GetString((byte*)prop.ToPointer(), nitems.ToInt32());
                        _requestedDataTcs?.TrySetResult(text);
                    }
                    else
                    {
                        if (actualTypeAtom == _atoms.INCR)
                        {
                            if (actualFormat != 32 || (int)nitems != 1)
                                _requestedDataTcs?.TrySetResult(null);
                            else
                            {
                                _incrDataReaders[sel.property] = new IncrDataReader(_atoms, sel.property, *(int*)prop.ToPointer(),
                                    (property, obj) =>
                                    {
                                        _incrDataReaders.Remove(property);
                                        _requestedDataTcs?.TrySetResult(obj);
                                    });
                            }
                        }
                        else
                        {
                            var data = new byte[(int)nitems * (actualFormat / 8)];
                            Marshal.Copy(prop, data, 0, data.Length);
                            _requestedDataTcs?.TrySetResult(data);
                        }
                    }
                }

                XFree(prop);
                return;
            }

            if (ev.type == XEventName.PropertyNotify)
            {
                if ((PropertyState)ev.PropertyEvent.state == PropertyState.NewValue && _incrDataReaders.TryGetValue(ev.PropertyEvent.atom, out var incrDataReader))
                {
                    XGetWindowProperty(_display, _handle, incrDataReader.Property, IntPtr.Zero, new IntPtr(0x7fffffff), true, (IntPtr)Atom.AnyPropertyType,
                            out var actualTypeAtom, out var actualFormat, out var nitems, out var bytes_after, out var prop);
                    incrDataReader.Append(prop, (int)nitems * (actualFormat / 8));

                    XFree(prop);
                    return;
                }
            }
        }

        private unsafe IntPtr WriteTargetToProperty(IntPtr target, IntPtr window, IntPtr property)
        {
            if (target == _atoms.TARGETS)
            {
                if (_storedDataObject != null)
                {
                    var atoms = ConvertDataObject(_storedDataObject);
                    XChangeProperty(_display, window, property,
                        _atoms.XA_ATOM, 32, PropertyMode.Replace, atoms, atoms.Length);

                    if (UseIncrProtocol(_storedDataObject))
                    {
                        System.Diagnostics.Debug.WriteLine("--- _atoms.TARGETS");
                        _storeAtomTcs?.TrySetResult(true);
                    }
                }
                return property;
            }

            if (target == _atoms.SAVE_TARGETS && _atoms.SAVE_TARGETS != IntPtr.Zero)
            {
                return property;
            }

            if (target == _atoms.MULTIPLE && _atoms.MULTIPLE != IntPtr.Zero)
            {
                XGetWindowProperty(_display, window, property, IntPtr.Zero, new IntPtr(0x7fffffff), false,
                    _atoms.ATOM_PAIR, out _, out var actualFormat, out var nitems, out _, out var prop);

                if (nitems != IntPtr.Zero && actualFormat == 32)
                {
                    var data = (IntPtr*)prop.ToPointer();
                    for (var c = 0; c < nitems.ToInt32(); c += 2)
                    {
                        var subTarget = data[c];
                        var subProp = data[c + 1];
                        var converted = WriteTargetToProperty(subTarget, window, subProp);
                        data[c + 1] = converted;
                    }

                    XChangeProperty(_display, window, property, _atoms.ATOM_PAIR, 32, PropertyMode.Replace,
                        prop.ToPointer(), nitems.ToInt32());
                }

                XFree(prop);
                return property;
            }

            if (_storedDataObject?.Contains(_atoms.GetAtomName(target)) == true)
            {
                var objValue = _storedDataObject.Get(_atoms.GetAtomName(target));

                if (!(objValue is byte[] bytes))
                {
                    if (objValue is string s)
                    {
                        var textEnc = GetStringEncoding(_atoms, target) ?? Encoding.UTF8;
                        bytes = textEnc.GetBytes(s);
                    }
                    else
                    {
                        _storeAtomTcs?.TrySetResult(true);
                        System.Diagnostics.Debug.WriteLine("--- IntPtr.Zero");
                        return IntPtr.Zero;
                    }
                }

                if (bytes.Length > MaxRequestSize && window != _handle)
                {
                    var incrDataWriter = new IncrDataWriter(target, bytes,
                         (w) =>
                         {
                             System.Diagnostics.Debug.WriteLine("--- IncrDataWriter completed");
                             _storeAtomTcs?.TrySetResult(true);

                         });

                    XSelectInput(_display, window, new IntPtr((int)EventMask.PropertyChangeMask));
                    var total = new IntPtr[] { (IntPtr)bytes.Length };
                    XChangeProperty(_display, window, property, _atoms.INCR, 32, PropertyMode.Replace, total, total.Length);
                }
                else
                {
                    XChangeProperty(_display, window, property, target, 8, PropertyMode.Replace, bytes, bytes.Length);
                    System.Diagnostics.Debug.WriteLine("--- NORM completed");
                    _storeAtomTcs?.TrySetResult(true);
                }
                return property;
            }
            return IntPtr.Zero;
        }

        private Task SendFormatRequest()
        {
            System.Diagnostics.Debug.WriteLine("----------- SendFormatRequest 0");

            XConvertSelection(_display, _atoms.CLIPBOARD, _atoms.TARGETS, _atoms.TARGETS, _handle,
                IntPtr.Zero);

            return HandleEvents(_cts.Token);
        }

        private Task<object?> SendDataRequest(IntPtr format)
        {
            if (_requestedDataTcs == null || _requestedDataTcs.Task.IsCompleted)
                _requestedDataTcs = new TaskCompletionSource<object?>();
            XConvertSelection(_display, _atoms.CLIPBOARD, format, format, _handle, IntPtr.Zero);
            return _requestedDataTcs.Task;
        }

        private bool HasOwner => XGetSelectionOwner(_display, _atoms.CLIPBOARD) != IntPtr.Zero;

        private IntPtr[] ConvertDataObject(IDataObject data)
        {
            var atoms = new HashSet<IntPtr> { _atoms.TARGETS, _atoms.MULTIPLE };
            if (data != null)
            {
                foreach (var fmt in data.GetDataFormats())
                {
                    if (fmt != null)
                    {
                        atoms.Add(_atoms.GetAtom(fmt));
                    }
                }
            }
            return atoms.ToArray();
        }

        private void StoreAtomsInClipboardManager(IDataObject data)
        {
            if (_atoms.CLIPBOARD_MANAGER != IntPtr.Zero && _atoms.SAVE_TARGETS != IntPtr.Zero)
            {
                var clipboardManager = XGetSelectionOwner(_display, _atoms.CLIPBOARD_MANAGER);
                if (clipboardManager != IntPtr.Zero)
                {
                    var atoms = ConvertDataObject(data);
                    XChangeProperty(_display, _handle, _avaloniaSaveTargetsAtom, _atoms.XA_ATOM, 32,
                        PropertyMode.Replace,
                        atoms, atoms.Length);
                    XConvertSelection(_display, _atoms.CLIPBOARD_MANAGER, _atoms.SAVE_TARGETS,
                        _avaloniaSaveTargetsAtom, _handle, IntPtr.Zero);
                }
            }
        }

        private bool UseIncrProtocol(IDataObject data)
        {
            foreach (var fmt in data.GetDataFormats())
            {
                var objValue = _storedDataObject?.Get(fmt);
                var dataSize = objValue switch
                {
                    byte[] bytes => bytes.Length,
                    string str => str.Length,
                    _ => 0
                };
                if (dataSize > MaxRequestSize)
                    return true;
            }
            return false;
        }

        public Task ClearAsync()
        {
            var data = new DataObject();
            data.Set(Clipboard.Core.ClipboardData.Format.Text, null);
            return SetDataObjectAsync(data);
        }

        public Task SetDataObjectAsync(IDataObject data)
        {
            _storedDataObject = data;
            if (_storeAtomTcs == null || _storeAtomTcs.Task.IsCompleted)
                _storeAtomTcs = new TaskCompletionSource<bool>();

            XSetSelectionOwner(_display, _atoms.CLIPBOARD, _handle, IntPtr.Zero);

            if (!UseIncrProtocol(data))
                StoreAtomsInClipboardManager(data);

            return _storeAtomTcs.Task;
        }

        public async Task<string[]> GetFormatsAsync()
        {
            if (!HasOwner)
            {
                return Array.Empty<string>();
            }
            await SendFormatRequest();

            return _requestedFormats
                .Select(x => _atoms.GetAtomName(x))
                .Where(x => x != null)
                .ToArray();
        }

        public async Task<object?> GetDataAsync(string format)
        {
            if (!HasOwner)
            {
                return null;
            }

            var formatAtom = _atoms.GetAtom(format);
            await SendFormatRequest();
            if (_requestedFormats.Contains(formatAtom) == false)
            {
                return null;
            }

            return await SendDataRequest(formatAtom);
        }

        unsafe Task HandleEvents(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine($"----------- RunLoop 0");
                int counter = 0;
                // XSetSelectionOwner(_display, _atoms.CLIPBOARD, _handle, IntPtr.Zero);
                // if (XGetSelectionOwner(_display, _atoms.CLIPBOARD) != _handle)
                // {
                //     throw new Exception($"Failed to take ownership of selection");
                // }

                while (!cancellationToken.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine($"----------- RunLoop 1 {counter++}");

                    XNextEvent(_display, out var xev);
                    if (XFilterEvent(ref xev, IntPtr.Zero))
                    {
                        continue;
                    }
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (xev.AnyEvent.window == _handle)
                    {
                        OnEvent(ref xev);
                    }

                    var pending = XPending(_display);
                    if (pending == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"----------- RunLoop 2 (pending == 0) {counter++}");
                        break;
                    }
                }
                // XSetSelectionOwner(_display, _atoms.CLIPBOARD, IntPtr.Zero, IntPtr.Zero);
                System.Diagnostics.Debug.WriteLine($"----------- RunLoop 3 {counter++}");
            });
        }
    }
}
