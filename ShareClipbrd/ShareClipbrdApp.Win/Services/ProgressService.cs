using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using GuardNet;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Win.Services {
    public class ProgressService : IProgressService {

        class ProgressSession : IAsyncDisposable {
            static readonly Dictionary<ProgressMode, Brush> brushes = new(){
                { ProgressMode.Send, Brushes.GreenYellow },
                { ProgressMode.Receive, Brushes.LightSeaGreen },
            };

            readonly IDialogService dialogService;
            readonly Stopwatch stopwatch;
            readonly ProgressMode mode;
            readonly Action onDispose;
            readonly MainWindow mainWindow;
            readonly Int64 max;
            readonly Int64 updatePeriod;
            Int64 updateCounter;
            Int64 progress;

            public ProgressSession(IDialogService dialogService, ProgressMode mode, Int64 max, Action onDispose) {
                this.dialogService = dialogService;
                this.mode = mode;
                this.max = max;
                this.onDispose = onDispose;
                stopwatch = Stopwatch.StartNew();
                var mw = Application.Current.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                mainWindow = mw;
                updateCounter = 0;
                progress = 0;
                updatePeriod = max / 100;
                if(updatePeriod == 0) {
                    updatePeriod = 1;
                }
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => {
                    mainWindow.pbOperation.Background = brushes[mode];
                    mainWindow.pbOperation.Maximum = max;
                    mainWindow.pbOperation.Value = 0;
                }));
            }

            public void Tick(Int64 steps) {
                updateCounter += steps;
                progress += steps;

                if(updateCounter > updatePeriod) {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => {
                        mainWindow.pbOperation.Value = progress;
                    }));
                    updateCounter = 0;
                }
            }

            public async ValueTask DisposeAsync() {
                var elapsed = 100 - stopwatch.ElapsedMilliseconds;
                if(elapsed > 0) {
                    await Task.Delay((int)elapsed);
                }

                mainWindow.Dispatcher.Invoke(DispatcherPriority.Render, new Action(async () => {
                    mainWindow.pbOperation.Value = 0;
                    if(progress < max) {
                        mainWindow.pbOperation.Background = Brushes.IndianRed;
                        dialogService.ShowMessage(mode == ProgressMode.Send
                            ? "Data transmit error"
                            : "Data receive error");
                    }
                    mainWindow.pbOperation.Background = Brushes.LightSteelBlue;
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

        public IAsyncDisposable Begin(Int64 max, ProgressMode mode) {
            lock(lockObj) {
                if(progressSession != null) {
                    throw new InvalidOperationException("Progress does not support multithreading");
                }
                progressSession = new(dialogService, mode, max, () => {
                    lock(lockObj) {
                        progressSession = null;
                    }
                });
                return progressSession;
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

    }
}
