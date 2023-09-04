using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Input;
using static Avalonia.X11.XLib;
namespace Avalonia.X11 {
    internal class X11Clipboard : IDisposable {

        #region inner classes
        private class IncrDataReader {
            private readonly X11Atoms _atoms;
            public readonly IntPtr Property;
            private readonly int _total;
            private readonly Action<IntPtr, object?> _onCompleted;
            private readonly List<byte> _readData;

            public IncrDataReader(X11Atoms atoms, IntPtr property, int total, Action<IntPtr, object?> onCompleted) {
                _atoms = atoms;
                Property = property;
                _total = total;
                _onCompleted = onCompleted;
                _readData = new List<byte>();
            }

            public void Append(IntPtr data, int size) {
                if(size > 0) {
                    var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(size);
                    Marshal.Copy(data, buffer, 0, size);
                    _readData.AddRange(buffer.Take(size));
                    System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
                    return;
                }

                if(_readData.Count != _total) {
                    _onCompleted(Property, null);
                    return;
                }

                var textEnc = GetStringEncoding(_atoms, Property);
                var bytes = _readData.ToArray();
                if(textEnc != null) {
                    _onCompleted(Property, textEnc.GetString(bytes));
                } else {
                    _onCompleted(Property, bytes);
                }
            }
        }

        private class IncrDataWriter {
            private readonly IntPtr _target;
            private readonly Action<IntPtr> _onCompleted;
            private byte[] _data;

            public IncrDataWriter(IntPtr target, byte[] data, Action<IntPtr> onCompleted) {
                _target = target;
                _data = data;
                _onCompleted = onCompleted;
            }

            public void OnEvent(ref XEvent ev) {
                if(ev.type == XEventName.PropertyNotify && (PropertyState)ev.PropertyEvent.state == PropertyState.Delete) {
                    if(_data?.Length > 0) {
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
        private TaskCompletionSource<IntPtr[]?>? _requestedFormatsTcs;
        private TaskCompletionSource<object?>? _requestedDataTcs;

        private readonly IntPtr _display;
        private readonly X11Atoms _atoms;

        private const int MaxRequestSize = 0x40000;
        private readonly Dictionary<IntPtr, IncrDataReader> _incrDataReaders;
        private readonly Dictionary<IntPtr, IncrDataWriter> _incrDataWriters;

        private readonly CancellationTokenSource _cts;

        public X11Clipboard() {
            _display = XOpenDisplay(IntPtr.Zero);
            if(_display == IntPtr.Zero) {
                throw new Exception("XOpenDisplay failed");
            }
            _atoms = new X11Atoms(_display);

            System.Diagnostics.Debug.WriteLine($"---- X11Clipboard {_display:X}");

            _handle = XCreateSimpleWindow(_display, XDefaultRootWindow(_display),
                    0, 0, 1, 1, 0, IntPtr.Zero, IntPtr.Zero);

            XSelectInput(_display, _handle, new IntPtr((int)(EventMask.StructureNotifyMask | EventMask.PropertyChangeMask)));

            _cts = new CancellationTokenSource();

            _incrDataReaders = new();
            _incrDataWriters = new();

            HandleEvents(_cts.Token);
        }

        public void Dispose() {
            _cts.Dispose();
            XDestroyWindow(_display, _handle);
            XCloseDisplay(_display);
        }

        private static Encoding? GetStringEncoding(X11Atoms atoms, IntPtr atom) {
            return (atom == atoms.XA_STRING
                    || atom == atoms.OEMTEXT)
                ? Encoding.ASCII
                : atom == atoms.UTF8_STRING
                    ? Encoding.UTF8
                    : atom == atoms.UTF16_STRING
                        ? Encoding.Unicode
                        : null;
        }

        private unsafe void OnEvent(ref XEvent ev) {
            // System.Diagnostics.Debug.WriteLine($"--------- X11Clipboard.OnEvent {ev}");
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
                    _requestedFormatsTcs?.TrySetResult(null);
                    _requestedDataTcs?.TrySetResult(null);
                    return;
                }
                XGetWindowProperty(_display, _handle, sel.property, IntPtr.Zero, new IntPtr(0x7fffffff), true, (IntPtr)Atom.AnyPropertyType,
                    out var actualTypeAtom, out var actualFormat, out var nitems, out var bytes_after, out var prop);
                Encoding? textEnc = null;

                if(nitems == IntPtr.Zero) {
                    _requestedFormatsTcs?.TrySetResult(null);
                    _requestedDataTcs?.TrySetResult(null);
                } else {
                    if(sel.property == _atoms.TARGETS) {
                        if(actualFormat != 32) {
                            _requestedFormatsTcs?.TrySetResult(null);
                        } else {
                            var formats = new IntPtr[nitems.ToInt32()];
                            Marshal.Copy(prop, formats, 0, formats.Length);
                            _requestedFormatsTcs?.TrySetResult(formats);
                            System.Diagnostics.Debug.WriteLine("----------- _requestedFormatsTcs?.TrySetResult(formats) 0");
                        }
                    } else if((textEnc = GetStringEncoding(_atoms, actualTypeAtom)) != null) {
                        var text = textEnc.GetString((byte*)prop.ToPointer(), nitems.ToInt32());
                        _requestedDataTcs?.TrySetResult(text);
                    } else {

                        if(actualTypeAtom == _atoms.INCR) {
                            if(actualFormat != 32 || (int)nitems != 1) {
                                _requestedDataTcs?.TrySetResult(null);
                            } else {
                                _incrDataReaders[sel.property] = new IncrDataReader(_atoms, sel.property, *(int*)prop.ToPointer(),
                                    (property, obj) => {
                                        _incrDataReaders.Remove(property);
                                        _requestedDataTcs?.TrySetResult(obj);
                                    });
                            }
                        } else {
                            var data = new byte[(int)nitems * (actualFormat / 8)];
                            Marshal.Copy(prop, data, 0, data.Length);
                            _requestedDataTcs?.TrySetResult(data);
                        }
                    }
                }

                XFree(prop);
                return;
            }

            if(ev.type == XEventName.PropertyNotify) {
                if((PropertyState)ev.PropertyEvent.state == PropertyState.NewValue && _incrDataReaders.TryGetValue(ev.PropertyEvent.atom, out var incrDataReader)) {
                    XGetWindowProperty(_display, _handle, incrDataReader.Property, IntPtr.Zero, new IntPtr(0x7fffffff), true, (IntPtr)Atom.AnyPropertyType,
                            out var actualTypeAtom, out var actualFormat, out var nitems, out var bytes_after, out var prop);

                    System.Diagnostics.Debug.WriteLine($"--------- INCR get 0 _incrReadTargetAtom:{_atoms.GetAtomName(incrDataReader.Property)}(0x{incrDataReader.Property:X}), "
                        + $"actualTypeAtom:{_atoms.GetAtomName(actualTypeAtom)}(0x{actualTypeAtom:X}), nitems:{nitems}");

                    incrDataReader.Append(prop, (int)nitems * (actualFormat / 8));

                    XFree(prop);
                    return;
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
                    _storeAtomTcs?.TrySetResult(true);
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
                        var textEnc = GetStringEncoding(_atoms, target) ?? Encoding.UTF8;
                        bytes = textEnc.GetBytes(s);
                    } else {
                        System.Diagnostics.Debug.WriteLine("--- IntPtr.Zero");
                        return property;
                    }
                }

                if(bytes.Length > MaxRequestSize && window != _handle) {
                    _incrDataWriters[window] = new IncrDataWriter(target, bytes,
                         (w) => {
                             _incrDataWriters.Remove(w);
                             System.Diagnostics.Debug.WriteLine("--- IncrDataWriter completed");
                         });

                    XSelectInput(_display, window, new IntPtr((int)EventMask.PropertyChangeMask));
                    var total = new IntPtr[] { (IntPtr)bytes.Length };
                    XChangeProperty(_display, window, property, _atoms.INCR, 32, PropertyMode.Replace, total, total.Length);
                } else {
                    XChangeProperty(_display, window, property, target, 8, PropertyMode.Replace, bytes, bytes.Length);
                    System.Diagnostics.Debug.WriteLine("--- NORM completed");
                }
                return property;
            }
            return IntPtr.Zero;
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

        public Task SetDataObjectAsync(IDataObject data) {
            _storedDataObject = data;
            if(_storeAtomTcs == null || _storeAtomTcs.Task.IsCompleted) {
                _storeAtomTcs = new TaskCompletionSource<bool>();
            }

            XSetSelectionOwner(_display, _atoms.CLIPBOARD, _handle, IntPtr.Zero);
            if(XGetSelectionOwner(_display, _atoms.CLIPBOARD) != _handle) {
                throw new Exception($"Failed to take ownership of selection");
            }

            return _storeAtomTcs.Task;
        }

        void StartRequestWorkaround() {
            var ddd = XGetSelectionOwner(_display, _atoms.CLIPBOARD);
            System.Diagnostics.Debug.WriteLine($"----------- StartRequest 0 {ddd}");
        }

        private Task<IntPtr[]?> SendFormatRequest() {
            System.Diagnostics.Debug.WriteLine($"----------- SendFormatRequest 0 {_requestedFormatsTcs}");

            if(_requestedFormatsTcs == null || _requestedFormatsTcs.Task.IsCompleted) {
                _requestedFormatsTcs = new();
            }


            XConvertSelection(_display, _atoms.CLIPBOARD, _atoms.TARGETS, _atoms.TARGETS, _handle, IntPtr.Zero);
            StartRequestWorkaround();
            return _requestedFormatsTcs.Task;

        }

        public async Task<string[]> GetFormatsAsync() {
            if(!HasOwner) {
                return Array.Empty<string>();
            }
            // System.Diagnostics.Debug.WriteLine($"----------- GetFormatsAsync 0");

            var formats = await SendFormatRequest();
            // System.Diagnostics.Debug.WriteLine($"----------- GetFormatsAsync 1");
            if(formats == null) {
                return Array.Empty<string>();
            }
            return formats
                .Select(x => _atoms.GetAtomName(x))
                .Where(x => x != null)
                .Cast<string>()
                .ToArray();
        }

        public async Task<object?> GetDataAsync(string format) {
            // System.Diagnostics.Debug.WriteLine($"----------- GetDataAsync 0 {format} {_requestedDataTcs}");
            if(!HasOwner) {
                return null;
            }

            var formatAtom = _atoms.GetAtom(format);
            var formats = await SendFormatRequest();
            if(formats == null || !formats.Contains(formatAtom)) {
                return null;
            }


            if(_requestedDataTcs == null || _requestedDataTcs.Task.IsCompleted) {
                _requestedDataTcs = new();
            }
            XConvertSelection(_display, _atoms.CLIPBOARD, formatAtom, formatAtom, _handle, IntPtr.Zero);
            StartRequestWorkaround();
            return await _requestedDataTcs.Task;
        }

        unsafe void HandleEvents(CancellationToken cancellationToken) {

            _ = Task.Run(() => {
                // var fd = XConnectionNumber(_display);
                // var ev = new epoll_event() {
                //     events = EPOLLIN,
                //     data = { u32 = (int)EventCodes.X11 }
                // };
                // var _epoll = epoll_create1(0);
                // if(_epoll == -1) {
                //     throw new X11Exception("epoll_create1 failed");
                // }

                // if(epoll_ctl(_epoll, EPOLL_CTL_ADD, fd, ref ev) == -1) {
                //     throw new X11Exception("Unable to attach X11 connection handle to epoll");
                // }

                // var fds = stackalloc int[2];
                // pipe2(fds, O_NONBLOCK);
                // var _sigread = fds[0];
                // var _sigwrite = fds[1];

                // ev = new epoll_event {
                //     events = EPOLLIN,
                //     data = { u32 = (int)EventCodes.Signal }
                // };
                // if(epoll_ctl(_epoll, EPOLL_CTL_ADD, _sigread, ref ev) == -1) {
                //     throw new X11Exception("Unable to attach signal pipe to epoll");
                // }


                System.Diagnostics.Debug.WriteLine($"----------- RunLoop 0");
                int counter = 0;
                while(!cancellationToken.IsCancellationRequested) {
                    System.Diagnostics.Debug.WriteLine($"----------- RunLoop 1 {counter++}");

                    // XFlush(_display);

                    // if(XPending(_display) == 0) {
                    //     var timeout = -1;
                    //     System.Diagnostics.Debug.WriteLine($"----------------------- RunLoop 1.3");
                    //     var epoll_res = epoll_wait(_epoll, &ev, 1, (int)Math.Min(int.MaxValue, timeout));

                    //     System.Diagnostics.Debug.WriteLine($"----------------------- RunLoop 1.5 epoll_res={epoll_res}");
                    //     // if(epoll_res == 0) {
                    //     //     break;
                    //     // }
                    //     // Drain the signaled pipe
                    //     int buf = 0;
                    //     while(read(_sigread, &buf, new IntPtr(4)).ToInt64() > 0) {
                    //     }
                    // }

                    // System.Diagnostics.Debug.WriteLine($"----------------------- RunLoop 2");
                    XNextEvent(_display, out var xev);
                    // System.Diagnostics.Debug.WriteLine($"----------------------- RunLoop 2.2");

                    if(xev.AnyEvent.window == _handle) {
                        OnEvent(ref xev);
                    }
                    if(_incrDataWriters.TryGetValue(xev.AnyEvent.window, out IncrDataWriter? incrDataWriter)) {
                        incrDataWriter.OnEvent(ref xev);
                    }
                }
                System.Diagnostics.Debug.WriteLine($"----------- RunLoop 3 {counter++} cancel:{cancellationToken.IsCancellationRequested}");
            });
        }
    }
}
