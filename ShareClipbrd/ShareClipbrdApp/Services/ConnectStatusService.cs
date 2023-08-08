using System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Services {
    public class ConnectStatusService : IConnectStatusService {
        public void Online() {
            Dispatcher.UIThread.InvokeAsync(new Action(() => {
                if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                    return;
                }
                var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                mainWindow.ShowConnectStatus(true);
            }), DispatcherPriority.Send);
        }

        public void Offline() {
            Dispatcher.UIThread.InvokeAsync(new Action(() => {
                if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                    return;
                }
                var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                mainWindow.ShowConnectStatus(false);
            }), DispatcherPriority.Send);
        }

        public void ClientOnline() {
            Dispatcher.UIThread.InvokeAsync(new Action(() => {
                if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                    return;
                }
                var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                mainWindow.ShowClientConnectStatus(true);
            }), DispatcherPriority.Send);
        }

        public void ClientOffline() {
            Dispatcher.UIThread.InvokeAsync(new Action(() => {
                if(!(Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)) {
                    return;
                }
                var mainWindow = desktop.MainWindow as MainWindow ?? throw new InvalidOperationException("MainWindow not found");
                mainWindow.ShowClientConnectStatus(false);
            }), DispatcherPriority.Send);
        }
    }
}
