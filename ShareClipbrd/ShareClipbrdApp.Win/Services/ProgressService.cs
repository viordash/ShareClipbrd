using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using GuardNet;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Win.Services {
    public class ProgressService : IProgressService {

        class ProgressSession : IAsyncDisposable {
            struct DiscontinuousProgress {
                public Int64 max;
                public Int64 updatePeriod;
                public Int64 updateCounter;
                public Int64 progress;
            }

            static readonly Dictionary<ProgressMode, Brush> brushes = new(){
                { ProgressMode.Send, Brushes.GreenYellow },
                { ProgressMode.Receive, Brushes.LightYellow },
            };

            readonly IDialogService dialogService;
            readonly Stopwatch stopwatch;
            readonly ProgressMode mode;
            readonly Action onDispose;
            DiscontinuousProgress major = new();
            DiscontinuousProgress minor = new();

            public ProgressSession(IDialogService dialogService, ProgressMode mode, Action onDispose) {
                this.dialogService = dialogService;
                this.mode = mode;
                this.onDispose = onDispose;
                stopwatch = Stopwatch.StartNew();
                major.progress = 0;
                major.max = 100;
                major.updatePeriod = 1;
                major.updateCounter = 0;
                minor.progress = 0;
                minor.max = 100;
                minor.updatePeriod = 1;
                minor.updateCounter = 0;


                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                    var mainWindow = Application.Current.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                    mainWindow.pbOperation.Background = brushes[mode];
                    mainWindow.pbOperation.Maximum = major.max;
                    mainWindow.pbOperation.Value = 0;

                    mainWindow.pbOperationMinor.Background = brushes[mode];
                    mainWindow.pbOperationMinor.Maximum = minor.max;
                    mainWindow.pbOperationMinor.Value = 0;
                }));
            }

            public void SetMaxTick(Int64 max) {
                major.max = max;
                major.updatePeriod = max / 100;
                if(major.updatePeriod == 0) {
                    major.updatePeriod = 1;
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                    var mainWindow = Application.Current.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                    mainWindow.pbOperation.Maximum = major.max;
                    mainWindow.pbOperation.Value = 0;
                }));
            }

            public void Tick(Int64 steps) {
                major.updateCounter += steps;
                major.progress += steps;

                if(major.updateCounter > major.updatePeriod) {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                        var mainWindow = Application.Current.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                        mainWindow.pbOperation.Value = major.progress;
                    }));
                    major.updateCounter = 0;
                }
            }

            public void SetMaxMinorTick(long max) {
                minor.max = max;
                minor.updatePeriod = max / 100;
                if(minor.updatePeriod == 0) {
                    minor.updatePeriod = 1;
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                    var mainWindow = Application.Current.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                    mainWindow.pbOperationMinor.Maximum = minor.max;
                    mainWindow.pbOperationMinor.Value = 0;
                }));
            }

            public void MinorTick(long steps) {
                minor.updateCounter += steps;
                minor.progress += steps;

                if(minor.updateCounter > minor.updatePeriod) {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => {
                        var mainWindow = Application.Current.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                        mainWindow.pbOperationMinor.Value = minor.progress;
                    }));
                    minor.updateCounter = 0;
                }
            }

            public async ValueTask DisposeAsync() {
                var elapsed = 100 - stopwatch.ElapsedMilliseconds;
                if(elapsed > 0) {
                    await Task.Delay((int)elapsed);
                }

                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(async () => {
                    var mainWindow = Application.Current.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                    mainWindow.pbOperation.Value = 0;
                    mainWindow.pbOperationMinor.Value = 0;
                    if(major.progress < major.max) {
                        mainWindow.pbOperation.Background = Brushes.IndianRed;
                        mainWindow.pbOperationMinor.Background = Brushes.IndianRed;
                        await Task.Delay(500);
                        Debug.WriteLine(mode == ProgressMode.Send
                            ? "Data transmit error"
                            : "Data receive error");
                    }
                    mainWindow.pbOperation.Background = Brushes.LightSteelBlue;
                    mainWindow.pbOperationMinor.Background = Brushes.LightSteelBlue;
                }));

                onDispose();
            }
        }

        readonly IDialogService dialogService;
        readonly object lockObj = new();
        static ProgressSession? progressSession = null;


        public ProgressService(IDialogService dialogService) {
            Guard.NotNull(dialogService, nameof(dialogService));
            this.dialogService = dialogService;
        }

        public IAsyncDisposable Begin(ProgressMode mode) {
            lock(lockObj) {
                if(progressSession != null) {
                    throw new InvalidOperationException("Progress does not support multithreading");
                }
                progressSession = new(dialogService, mode, () => {
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
