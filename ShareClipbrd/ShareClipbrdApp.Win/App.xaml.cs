using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Win {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        readonly IServiceProvider serviceProvider;
        App() {

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;
            serviceProvider = Win.Startup.BuildServiceProvider();
            InitializeComponent();
        }

        private void OnStartup(object sender, StartupEventArgs e) {
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            var ex = (Exception)e.ExceptionObject;
            var dialogService = serviceProvider.GetRequiredService<IDialogService>();
            dialogService.ShowError(ex);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            var ex = e.Exception;
            var dialogService = serviceProvider.GetRequiredService<IDialogService>();
            dialogService.ShowError(ex);
            e.Handled = true;
        }
    }
}
