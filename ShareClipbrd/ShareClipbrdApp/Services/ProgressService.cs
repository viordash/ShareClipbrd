using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;
using GuardNet;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Services {
    public class ProgressService : IProgressService {

        class ProgressSession : IAsyncDisposable {
            struct DiscontinuousProgress {
                public double max;
                public Int64 updatePeriod;
                public Int64 updateCounter;
                public double progress;
            }

            readonly Stopwatch stopwatch;
            readonly ProgressMode mode;
            readonly Action onDispose;
            DiscontinuousProgress major = new();
            DiscontinuousProgress minor = new();

            public ProgressSession(ProgressMode mode, Action onDispose) {
                this.mode = mode;
                this.onDispose = onDispose;
                stopwatch = Stopwatch.StartNew();
                major.progress = 0;
                major.max = 100.0;
                major.updatePeriod = 1;
                major.updateCounter = 0;
                minor.progress = 0;
                minor.max = 100.0;
                minor.updatePeriod = 1;
                minor.updateCounter = 0;


                Dispatcher.UIThread.InvokeAsync(new Action(() => {
                    if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                        return;
                    }
                    var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                    mainWindow.SetProgress(0);
                    mainWindow.SetProgressMinor(0);
                    mainWindow.SetProgressMode(mode);
                }), DispatcherPriority.Send);
            }

            public void SetMaxTick(Int64 max) {
                major.max = max;
                major.updatePeriod = max / 100;
                if(major.updatePeriod == 0) {
                    major.updatePeriod = 1;
                }

                Dispatcher.UIThread.InvokeAsync(new Action(() => {
                    if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                        return;
                    }
                    var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                    mainWindow.SetProgress(0);
                }), DispatcherPriority.Send);
            }

            public void Tick(Int64 steps) {
                major.updateCounter += steps;
                major.progress += steps;

                if(major.updateCounter > major.updatePeriod) {
                    Dispatcher.UIThread.InvokeAsync(new Action(() => {
                        if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                            return;
                        }
                        var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                        mainWindow.SetProgress(major.progress * 100.0 / major.max);
                    }), DispatcherPriority.Send);
                    major.updateCounter = 0;
                }
            }

            public void SetMaxMinorTick(long max) {
                minor.max = max;
                minor.updatePeriod = max / 100;
                if(minor.updatePeriod == 0) {
                    minor.updatePeriod = 1;
                }

                Dispatcher.UIThread.InvokeAsync(new Action(() => {
                    if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                        return;
                    }
                    var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                    mainWindow.SetProgressMinor(minor.progress * 100.0 / minor.max);
                }), DispatcherPriority.Send);

            }

            public void MinorTick(long steps) {
                minor.updateCounter += steps;
                minor.progress += steps;

                if(minor.updateCounter > minor.updatePeriod) {
                    Dispatcher.UIThread.InvokeAsync(new Action(() => {
                        if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                            return;
                        }
                        var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                        mainWindow.SetProgress(major.progress);
                    }), DispatcherPriority.Send);
                    minor.updateCounter = 0;
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
                    mainWindow.SetProgress(0);
                    mainWindow.SetProgressMinor(0);

                    if(major.progress < major.max) {
                        mainWindow.SetProgressMode(ProgressMode.Error);
                        await Task.Delay(500);
                        Debug.WriteLine(mode == ProgressMode.Send
                            ? "Data transmit error"
                            : "Data receive error");
                    }
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
