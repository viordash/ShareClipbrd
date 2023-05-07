using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using GuardNet;

namespace ShareClipbrdApp.Win {
    internal class ProcessIndicator : IAsyncDisposable {
        public static ProcessIndicator Indicate(MainWindow mainWindow) {
            return new ProcessIndicator(mainWindow);
        }

        readonly MainWindow mainWindow;
        readonly Stopwatch stopwatch;

        private ProcessIndicator(MainWindow mainWindow) {
            Guard.NotNull(mainWindow, nameof(mainWindow));
            this.mainWindow = mainWindow;
            this.stopwatch = Stopwatch.StartNew();
            mainWindow.Background = Brushes.GreenYellow;
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
