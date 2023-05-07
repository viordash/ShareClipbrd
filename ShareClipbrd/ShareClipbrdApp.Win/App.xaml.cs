using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace ShareClipbrdApp.Win {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        readonly IServiceProvider serviceProvider;
        App() {
            serviceProvider = Win.Startup.BuildServiceProvider();
            InitializeComponent();
        }

        private void OnStartup(object sender, StartupEventArgs e) {
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
