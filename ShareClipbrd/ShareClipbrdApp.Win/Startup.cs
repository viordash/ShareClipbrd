using System;
using Microsoft.Extensions.DependencyInjection;
using ShareClipbrd.Core.Services;
using ShareClipbrdApp.Win.Services;

namespace ShareClipbrdApp.Win {
    public class Startup {
        public static IServiceProvider BuildServiceProvider() {
            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>()
                    .AddSingleton<IDataTransferService, DataTransferService>();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
