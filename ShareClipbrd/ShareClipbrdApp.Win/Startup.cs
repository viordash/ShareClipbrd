using System;
using Microsoft.Extensions.DependencyInjection;

namespace ShareClipbrdApp.Win {
    public class Startup {
        public static IServiceProvider BuildServiceProvider() {
            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
