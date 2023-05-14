using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using GuardNet;

namespace ShareClipbrdApp.Win {
    internal class ProcessIndicator : IAsyncDisposable {

        public enum Mode {
            Send,
            Receive
        };

        static readonly Dictionary<Mode, Brush> brushes = new(){
            { Mode.Send, Brushes.GreenYellow },
            { Mode.Receive, Brushes.LightSeaGreen },
        };


        public static ProcessIndicator Indicate(MainWindow mainWindow, Mode mode) {
            return new ProcessIndicator(mainWindow, mode);
        }

        readonly MainWindow mainWindow;
        readonly Stopwatch stopwatch;

        private ProcessIndicator(MainWindow mainWindow, Mode mode) {
            Guard.NotNull(mainWindow, nameof(mainWindow));
            this.mainWindow = mainWindow;
            this.stopwatch = Stopwatch.StartNew();
            mainWindow.Background = brushes[mode];
        }

        public async ValueTask DisposeAsync() {
            var elapsed = 100 - stopwatch.ElapsedMilliseconds;
            if(elapsed > 0) {
                await Task.Delay((int)elapsed);
            }
            mainWindow.Background = Brushes.LightSteelBlue;
        }
    }
}
