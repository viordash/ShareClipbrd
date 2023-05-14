using System;
using Microsoft.Extensions.DependencyInjection;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Services;
using ShareClipbrdApp.Win.Clipboard;
using ShareClipbrdApp.Win.Configuration;
using ShareClipbrdApp.Win.Services;

namespace ShareClipbrdApp.Win {
    public class Startup {
        public static IServiceProvider BuildServiceProvider() {
            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>()
                    .AddSingleton<IDialogService, DialogService>()
                    .AddSingleton<ISystemConfiguration, SystemConfiguration>()
                    .AddSingleton<IClipboardSerializer, ClipboardSerializer>()
                    .AddSingleton<IDataServer, DataServer>()
                    .AddSingleton<IDataClient, DataClient>()
                    ;

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
