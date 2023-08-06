using System;
using Microsoft.Extensions.DependencyInjection;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Services;
using ShareClipbrdApp.Configuration;
using ShareClipbrdApp.Services;


namespace ShareClipbrdApp {
    public class Startup {
        public static IServiceProvider BuildServiceProvider() {
            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>()
                    .AddSingleton<IDialogService, DialogService>()
                    .AddSingleton<ISystemConfiguration, SystemConfiguration>()
                    .AddSingleton<IDataServer, DataServer>()
                    .AddSingleton<IDataClient, DataClient>()
                    .AddSingleton<IDispatchService, DispatchService>()
                    .AddSingleton<IProgressService, ProgressService>()
                    .AddSingleton<IConnectStatusService, ConnectStatusService>()
                    .AddSingleton<ITimeService, TimeService>()
                    ;

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
