using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp {
    public partial class App : Application {
        IServiceProvider? serviceProvider;

        public override void Initialize() {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

            TaskScheduler.UnobservedTaskException += CurrentDomain_UnobservedTaskException;

            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            serviceProvider = Startup.BuildServiceProvider();

            if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = serviceProvider?.GetRequiredService<MainWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            var ex = (Exception)e.ExceptionObject;
            var dialogService = serviceProvider?.GetRequiredService<IDialogService>();
            dialogService?.ShowError(ex);
        }

        private void CurrentDomain_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) {
            var ex = e.Exception;
            var dialogService = serviceProvider?.GetRequiredService<IDialogService>();
            dialogService?.ShowError(ex);
        }
    }
}