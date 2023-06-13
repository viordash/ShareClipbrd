using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Services {
    public class ProgressService : IProgressService {

        class ProgressSession : IAsyncDisposable {
            readonly Stopwatch stopwatch;
            readonly ProgressMode mode;
            readonly Action onDispose;

            double majorMax;
            double majorProgress;
            double minorMax;
            double minorProgress;
            double prevPercent;

            public ProgressSession(ProgressMode mode, Action onDispose) {
                this.mode = mode;
                this.onDispose = onDispose;
                stopwatch = Stopwatch.StartNew();
                majorProgress = 0;
                majorMax = 100.0;
                minorProgress = 0;
                minorMax = 100.0;
                prevPercent = 0;

                Dispatcher.UIThread.InvokeAsync(new Action(() => {
                    if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                        return;
                    }
                    var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                    mainWindow.SetProgress(0);
                    mainWindow.SetProgressMode(mode);
                }), DispatcherPriority.Send);
            }

            public void SetMaxTick(Int64 max) {
                majorMax = max;
                majorProgress = 0;
                Tick();
            }

            public void Tick(Int64 steps) {
                majorProgress += steps;
                Tick();
            }

            public void SetMaxMinorTick(long max) {
                minorMax = max;
                minorProgress = 0;
            }

            public void MinorTick(long steps) {
                minorProgress += steps;
                Tick();
            }

            void Tick() {
                var percMajor = (majorProgress - 1) * 100.0 / majorMax;
                var percMinor = minorProgress * (100.0 / majorMax) / minorMax;
                var percent = percMajor + percMinor;

                if(Math.Abs(prevPercent - percent) > 0.05) {
                    Dispatcher.UIThread.InvokeAsync(new Action(() => {
                        if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                            return;
                        }
                        var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");

                        switch(mode) {
                            case ProgressMode.Receive:
                                mainWindow.SetProgress(percent - 100);
                                break;

                            default:
                                mainWindow.SetProgress(percent);
                                break;
                        }

                    }), DispatcherPriority.Send);
                    prevPercent = percent;
                }
            }

            public async ValueTask DisposeAsync() {
                var elapsed = 100 - stopwatch.ElapsedMilliseconds;
                if(elapsed > 0) {
                    await Task.Delay((int)elapsed);
                }


                await Dispatcher.UIThread.InvokeAsync(new Action(async () => {
                    if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                        return;
                    }
                    var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                    mainWindow.SetProgress(100.0);

                    if(majorProgress < majorMax) {
                        mainWindow.SetProgressMode(ProgressMode.Error);
                        await Task.Delay(500);
                        Debug.WriteLine(mode == ProgressMode.Send
                            ? "Data transmit error"
                            : "Data receive error");
                    }

                    await Task.Delay(100);
                    mainWindow.SetProgressMode(ProgressMode.None);
                }), DispatcherPriority.Send);

                onDispose();
            }
        }

        readonly object lockObj = new();
        static ProgressSession? progressSession = null;

        public IAsyncDisposable Begin(ProgressMode mode) {
            lock(lockObj) {
                if(progressSession != null) {
                    throw new InvalidOperationException("Progress does not support multithreading");
                }
                progressSession = new(mode, () => {
                    lock(lockObj) {
                        progressSession = null;
                    }
                });
                return progressSession;
            }
        }

        public void SetMaxTick(Int64 max) {
            lock(lockObj) {
                if(progressSession == null) {
                    throw new InvalidOperationException("Progress is out of scope");
                }
                progressSession.SetMaxTick(max);
            }
        }

        public void Tick(Int64 steps) {
            lock(lockObj) {
                if(progressSession == null) {
                    throw new InvalidOperationException("Progress is out of scope");
                }
                progressSession.Tick(steps);
            }
        }

        public void SetMaxMinorTick(long max) {
            lock(lockObj) {
                if(progressSession == null) {
                    throw new InvalidOperationException("Progress is out of scope");
                }
                progressSession.SetMaxMinorTick(max);
            }
        }

        public void MinorTick(long steps) {
            lock(lockObj) {
                if(progressSession == null) {
                    throw new InvalidOperationException("Progress is out of scope");
                }
                progressSession.MinorTick(steps);
            }
        }
    }
}
