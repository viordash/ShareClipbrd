using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Input;
using static Avalonia.X11.XLib;
namespace Avalonia.X11 {
    internal class X11Clipboard : IDisposable {

        private IDataObject? _storedDataObject;
        private IntPtr _handle;
        private readonly List<IntPtr> _requestedFormats;
        private object? _requestedData;
        private readonly IntPtr _avaloniaSaveTargetsAtom;

        private readonly IntPtr _display;
        private readonly X11Atoms _atoms;

        private IntPtr _incrReadTargetAtom;
        private readonly List<byte> _incrReadData;
        private IntPtr _incrWriteTargetAtom;
        private IntPtr _incrWriteWindow;
        private IntPtr _incrWriteProperty;
        private byte[]? _incrWriteData;
        private const int MaxRequestSize = 0x40000;

        private readonly CancellationTokenSource _cts;

        public X11Clipboard() {
            _display = XOpenDisplay(IntPtr.Zero);
            if(_display == IntPtr.Zero) {
                throw new Exception("XOpenDisplay failed");
            }
            _atoms = new X11Atoms(_display);

            _handle = XCreateSimpleWindow(_display, XDefaultRootWindow(_display),
                    0, 0, 1, 1, 0, IntPtr.Zero, IntPtr.Zero);

            XSelectInput(_display, _handle, new IntPtr((int)(EventMask.StructureNotifyMask | EventMask.PropertyChangeMask)));

            _avaloniaSaveTargetsAtom = XInternAtom(_display, "AVALONIA_SAVE_TARGETS_PROPERTY_ATOM", false);

            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            _requestedFormats = new List<IntPtr>();
            _requestedData = null;

            _incrReadTargetAtom = IntPtr.Zero;
            _incrReadData = new List<byte>();
            _incrWriteTargetAtom = IntPtr.Zero;
            _incrWriteWindow = IntPtr.Zero;
            _incrWriteProperty = IntPtr.Zero;
            _incrWriteData = null;
        }

        public void Dispose() {
            XCloseDisplay(_display);
        }

        private Encoding? GetStringEncoding(IntPtr atom) {
            return (atom == _atoms.XA_STRING
                    || atom == _atoms.OEMTEXT)
                ? Encoding.ASCII
                : atom == _atoms.UTF8_STRING
                    ? Encoding.UTF8
                    : atom == _atoms.UTF16_STRING
                        ? Encoding.Unicode
                        : null;
        }

        private unsafe void OnEvent(ref XEvent ev) {
            System.Diagnostics.Debug.WriteLine($"--------- X11Clipboard.OnEvent {ev}");
            if(ev.type == XEventName.SelectionClear) {
                System.Diagnostics.Debug.WriteLine("--- XEventName.SelectionClear");
                return;
            }

            if(ev.type == XEventName.SelectionRequest) {
                var sel = ev.SelectionRequestEvent;
                var resp = new XEvent {
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
                resp.SelectionEvent.property = WriteTargetToProperty(sel.target, sel.requestor, sel.property);

                XSendEvent(_display, sel.requestor, false, new IntPtr((int)EventMask.NoEventMask), ref resp);
                return;
            }

            if(ev.type == XEventName.SelectionNotify) {
                var sel = ev.SelectionEvent;
                if(sel.property == IntPtr.Zero) {
                    return;
                }
                XGetWindowProperty(_display, _handle, sel.property, IntPtr.Zero, new IntPtr(0x7fffffff), true, (IntPtr)Atom.AnyPropertyType,
                    out var actualTypeAtom, out var actualFormat, out var nitems, out var bytes_after, out var prop);
                Encoding? textEnc = null;

                if(nitems != IntPtr.Zero) {
                    if(sel.property == _atoms.TARGETS) {
                        if(actualFormat == 32) {
                            var formatsArr = new IntPtr[nitems.ToInt32()];
                            Marshal.Copy(prop, formatsArr, 0, formatsArr.Length);
                            _requestedFormats.AddRange(formatsArr);
                            System.Diagnostics.Debug.WriteLine("----------- _requestedFormatsTcs?.TrySetResult(formats) 0");
                        }
                    } else if((textEnc = GetStringEncoding(actualTypeAtom)) != null) {
                        var text = textEnc.GetString((byte*)prop.ToPointer(), nitems.ToInt32());
                        _requestedData = text;
                    } else {
                        if(actualTypeAtom == _atoms.INCR) {
                            if(actualFormat == 32 && (int)nitems == 1) {
                                _incrReadTargetAtom = sel.property;
                                var total = *((int*)prop.ToPointer());
                                _incrReadData.Capacity = total;
                            }
                        } else {
                            var data = new byte[(int)nitems * (actualFormat / 8)];
                            Marshal.Copy(prop, data, 0, data.Length);
                            _requestedData = data;
                        }
                    }
                }

                XFree(prop);
                return;
            }

            if(ev.type == XEventName.PropertyNotify) {
                if(_incrReadTargetAtom == ev.PropertyEvent.atom && (PropertyState)ev.PropertyEvent.state == PropertyState.NewValue) {
                    XGetWindowProperty(_display, _handle, _incrReadTargetAtom, IntPtr.Zero, new IntPtr(0x7fffffff), true, (IntPtr)Atom.AnyPropertyType,
                                out var actualTypeAtom, out var actualFormat, out var nitems, out var bytes_after, out var prop);

                    if(_incrReadTargetAtom == actualTypeAtom && (int)nitems > 0) {
                        var chunkSize = (int)nitems * (actualFormat / 8);
                        var buffer = new byte[chunkSize];
                        Marshal.Copy(prop, buffer, 0, chunkSize);
                        _incrReadData.AddRange(buffer);
                    } else {
                        var bytes = _incrReadData.ToArray();

                        if(_incrReadData.Count != 0) {
                            var textEnc = GetStringEncoding(_incrReadTargetAtom);
                            if(textEnc != null) {
                                var text = textEnc.GetString(bytes);
                                _requestedData = text;
                            } else {
                                _requestedData = bytes;
                            }
                        }
                        _incrReadData.Clear();
                        _incrReadTargetAtom = IntPtr.Zero;

                    }
                    XFree(prop);
                    return;
                }
            }
        }

        private void OnIncrWritePropertyEvent(ref XEvent ev) {
            if(ev.type == XEventName.PropertyNotify && (PropertyState)ev.PropertyEvent.state == PropertyState.Delete) {
                if(_incrWriteData?.Length > 0) {
                    var bytes = _incrWriteData.Take(MaxRequestSize).ToArray();
                    _incrWriteData = _incrWriteData.Skip(bytes.Length).ToArray();
                    XChangeProperty(_display, _incrWriteWindow, _incrWriteProperty, _incrWriteTargetAtom, 8, PropertyMode.Replace, bytes, bytes.Length);
                } else {
                    XChangeProperty(_display, _incrWriteWindow, _incrWriteProperty, _incrWriteTargetAtom, 8, PropertyMode.Replace, IntPtr.Zero, 0);
                    _incrWriteTargetAtom = IntPtr.Zero;
                    _incrWriteData = null;
                    _incrWriteWindow = IntPtr.Zero;
                }
            }
        }

        private unsafe IntPtr WriteTargetToProperty(IntPtr target, IntPtr window, IntPtr property) {

            System.Diagnostics.Debug.WriteLine($"--------- WriteTargetToProperty 0 {target}");
            if(target == _atoms.TARGETS) {
                if(_storedDataObject != null) {
                    var atoms = ConvertDataObject(_storedDataObject);
                    XChangeProperty(_display, window, property,
                        _atoms.XA_ATOM, 32, PropertyMode.Replace, atoms, atoms.Length);

                    if(UseIncrProtocol(_storedDataObject)) {
                        System.Diagnostics.Debug.WriteLine("--- WriteTargetToProperty _atoms.TARGETS");
                    }
                }
                return property;
            }

            if(target == _atoms.SAVE_TARGETS && _atoms.SAVE_TARGETS != IntPtr.Zero) {
                return property;
            }

            if(target == _atoms.MULTIPLE && _atoms.MULTIPLE != IntPtr.Zero) {
                XGetWindowProperty(_display, window, property, IntPtr.Zero, new IntPtr(0x7fffffff), false,
                    _atoms.ATOM_PAIR, out _, out var actualFormat, out var nitems, out _, out var prop);

                if(nitems != IntPtr.Zero && actualFormat == 32) {
                    var data = (IntPtr*)prop.ToPointer();
                    for(var c = 0; c < nitems.ToInt32(); c += 2) {
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

            if(_storedDataObject?.Contains(_atoms.GetAtomName(target)) == true) {

                System.Diagnostics.Debug.WriteLine($"----------- WriteTargetToProperty 5 {_atoms.GetAtomName(target)}");
                var objValue = _storedDataObject.Get(_atoms.GetAtomName(target));

                if(!(objValue is byte[] bytes)) {
                    if(objValue is string s) {
                        var textEnc = GetStringEncoding(target) ?? Encoding.UTF8;
                        bytes = textEnc.GetBytes(s);
                    } else {
                        System.Diagnostics.Debug.WriteLine("--- IntPtr.Zero");
                        return property;
                        // return IntPtr.Zero;
                    }
                }

                if(bytes.Length > MaxRequestSize && window != _handle) {
                    _incrWriteTargetAtom = target;
                    _incrWriteWindow = window;
                    _incrWriteProperty = property;
                    _incrWriteData = bytes;
                    var total = new IntPtr[] { (IntPtr)bytes.Length };
                    XChangeProperty(_display, window, property, _atoms.INCR, 32, PropertyMode.Replace, total, total.Length);
                    XSelectInput(_display, window, new IntPtr((int)EventMask.PropertyChangeMask));
                } else {
                    XChangeProperty(_display, window, property, target, 8, PropertyMode.Replace, bytes, bytes.Length);
                    System.Diagnostics.Debug.WriteLine("--- NORM completed");
                }
                return property;
            }
            return IntPtr.Zero;
        }

        private Task SendFormatRequest() {
            System.Diagnostics.Debug.WriteLine("----------- SendFormatRequest 0");

            XConvertSelection(_display, _atoms.CLIPBOARD, _atoms.TARGETS, _atoms.TARGETS, _handle,
                IntPtr.Zero);

            return HandleEvents(_cts.Token);
        }

        private Task SendDataRequest(IntPtr format) {

            System.Diagnostics.Debug.WriteLine("----------- SendDataRequest 0");
            XConvertSelection(_display, _atoms.CLIPBOARD, format, format, _handle, IntPtr.Zero);
            return HandleEvents(_cts.Token);
        }

        private bool HasOwner => XGetSelectionOwner(_display, _atoms.CLIPBOARD) != IntPtr.Zero;

        private IntPtr[] ConvertDataObject(IDataObject data) {
            var atoms = new HashSet<IntPtr> { _atoms.TARGETS, _atoms.MULTIPLE };
            if(data != null) {
                foreach(var fmt in data.GetDataFormats()) {
                    if(fmt != null) {
                        atoms.Add(_atoms.GetAtom(fmt));
                    }
                }
            }
            return atoms.ToArray();
        }

        private void StoreAtomsInClipboardManager(IDataObject data) {
            if(_atoms.CLIPBOARD_MANAGER != IntPtr.Zero && _atoms.SAVE_TARGETS != IntPtr.Zero) {
                var clipboardManager = XGetSelectionOwner(_display, _atoms.CLIPBOARD_MANAGER);
                if(clipboardManager != IntPtr.Zero) {
                    var atoms = ConvertDataObject(data);
                    XChangeProperty(_display, _handle, _avaloniaSaveTargetsAtom, _atoms.XA_ATOM, 32,
                        PropertyMode.Replace,
                        atoms, atoms.Length);
                    XConvertSelection(_display, _atoms.CLIPBOARD_MANAGER, _atoms.SAVE_TARGETS,
                        _avaloniaSaveTargetsAtom, _handle, IntPtr.Zero);
                }
            }
        }

        private bool UseIncrProtocol(IDataObject data) {
            foreach(var fmt in data.GetDataFormats()) {
                var objValue = _storedDataObject?.Get(fmt);
                var dataSize = objValue switch {
                    byte[] bytes => bytes.Length,
                    string str => str.Length,
                    _ => 0
                };
                if(dataSize > MaxRequestSize)
                    return true;
            }
            return false;
        }

        public Task ClearAsync() {
            var data = new DataObject();
            data.Set(Clipboard.Core.ClipboardData.Format.Text_x11, null);
            return SetDataObjectAsync(data);
        }

        public async Task SetDataObjectAsync(IDataObject data) {
            _storedDataObject = data;

            XSetSelectionOwner(_display, _atoms.CLIPBOARD, _handle, IntPtr.Zero);
            if(XGetSelectionOwner(_display, _atoms.CLIPBOARD) != _handle) {
                throw new Exception($"Failed to take ownership of selection");
            }

            if(!UseIncrProtocol(data)) {
                StoreAtomsInClipboardManager(data);
            }

            await HandleEvents(_cts.Token);

            // XSetSelectionOwner(_display, _atoms.CLIPBOARD, IntPtr.Zero, IntPtr.Zero);
        }

        public async Task<string[]> GetFormatsAsync() {
            if(!HasOwner) {
                return Array.Empty<string>();
            }
            await SendFormatRequest();

            return _requestedFormats
                .Select(x => _atoms.GetAtomName(x))
                .Where(x => x != null)
                .Cast<string>()
                .ToArray();
        }

        public async Task<object?> GetDataAsync(string format) {
            if(!HasOwner) {
                return null;
            }

            var formatAtom = _atoms.GetAtom(format);
            await SendFormatRequest();
            if(_requestedFormats.Contains(formatAtom) == false) {
                return null;
            }

            await SendDataRequest(formatAtom);
            return _requestedData;
        }

        unsafe Task HandleEvents(CancellationToken cancellationToken) {

            return Task.Run(() => {
                var fd = XConnectionNumber(_display);
                var ev = new epoll_event() {
                    events = EPOLLIN,
                    data = { u32 = (int)EventCodes.X11 }
                };
                var _epoll = epoll_create1(0);
                if(_epoll == -1) {
                    throw new X11Exception("epoll_create1 failed");
                }

                if(epoll_ctl(_epoll, EPOLL_CTL_ADD, fd, ref ev) == -1) {
                    throw new X11Exception("Unable to attach X11 connection handle to epoll");
                }

                var fds = stackalloc int[2];
                pipe2(fds, O_NONBLOCK);
                var _sigread = fds[0];
                var _sigwrite = fds[1];

                ev = new epoll_event {
                    events = EPOLLIN,
                    data = { u32 = (int)EventCodes.Signal }
                };
                if(epoll_ctl(_epoll, EPOLL_CTL_ADD, _sigread, ref ev) == -1) {
                    throw new X11Exception("Unable to attach signal pipe to epoll");
                }

                System.Diagnostics.Debug.WriteLine($"----------- RunLoop 0");
                int counter = 0;

                while(!cancellationToken.IsCancellationRequested) {
                    System.Diagnostics.Debug.WriteLine($"----------- RunLoop 1 {counter++}");

                    XFlush(_display);

                    if(XPending(_display) == 0) {
                        var timeout = 100;
                        var epoll_res = epoll_wait(_epoll, &ev, 1, (int)Math.Min(int.MaxValue, timeout));

                        System.Diagnostics.Debug.WriteLine($"----------------------- RunLoop 1.5 epoll_res={epoll_res}");
                        if(epoll_res == 0) {
                            break;
                        }
                        // Drain the signaled pipe
                        int buf = 0;
                        while(read(_sigread, &buf, new IntPtr(4)).ToInt64() > 0) {
                        }

                    }

                    if(cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    XNextEvent(_display, out var xev);

                    if(xev.AnyEvent.window == _handle) {
                        OnEvent(ref xev);
                    }
                    if(xev.AnyEvent.window == _incrWriteWindow) {
                        OnIncrWritePropertyEvent(ref xev);
                    }
                }
                System.Diagnostics.Debug.WriteLine($"----------- RunLoop 3 {counter++}");
            });
        }
    }
}
