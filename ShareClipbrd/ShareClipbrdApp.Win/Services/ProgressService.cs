using System;
using GuardNet;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using ShareClipbrd.Core.Services;
using System.Windows;

namespace ShareClipbrdApp.Win.Services {
    public class ProgressService : IProgressService {

        class ProgressSession : IAsyncDisposable {
            static readonly Dictionary<ProgressMode, Brush> brushes = new(){
                { ProgressMode.Send, Brushes.GreenYellow },
                { ProgressMode.Receive, Brushes.LightSeaGreen },
            };

            readonly Stopwatch stopwatch;
            readonly Action onDispose;

            public ProgressSession(ProgressMode mode, Action onDispose) {
                this.stopwatch = Stopwatch.StartNew();
                this.onDispose = onDispose;
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow!.Background = brushes[mode];
            }

            public async ValueTask DisposeAsync() {
                var elapsed = 100 - stopwatch.ElapsedMilliseconds;
                if(elapsed > 0) {
                    await Task.Delay((int)elapsed);
                }
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow!.Background = Brushes.LightSteelBlue;
                onDispose();
            }
        }

        readonly object lockObj = new object();
        static ProgressSession? progressSession = null;

        public IAsyncDisposable Begin(Int64 max, ProgressMode mode) {
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

        public void Tick(Int64 steps) {
            throw new NotImplementedException();
        }

    }
}
