using System.Net.Sockets;
using System.Text;
using GuardNet;
using ShareClipbrd.Core.Configuration;

namespace ShareClipbrd.Core.Services {
    public interface IDataClient {
        Task Send(ClipboardData clipboardData);
    }

    public class DataClient : IDataClient {
        readonly ISystemConfiguration systemConfiguration;
        readonly IDialogService dialogService;
        readonly CancellationTokenSource cts;

        public DataClient(
            ISystemConfiguration systemConfiguration,
            IDialogService dialogService
            ) {
            Guard.NotNull(systemConfiguration, nameof(systemConfiguration));
            Guard.NotNull(dialogService, nameof(dialogService));
            this.systemConfiguration = systemConfiguration;
            this.dialogService = dialogService;

            cts = new CancellationTokenSource();
        }

        public async Task Send(ClipboardData clipboardData) {
            var cancellationToken = cts.Token;
            using TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(systemConfiguration.HostAddress.Address, systemConfiguration.HostAddress.Port, cancellationToken);

            var stream = tcpClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);

            foreach(var format in clipboardData.Formats) {
                await stream.WriteAsync(Encoding.ASCII.GetBytes(format.Key), cancellationToken);
                var response = await reader.ReadToEndAsync(cancellationToken);
                if(response != format.Key) {
                    continue;
                }
                await stream.WriteAsync(format.Value, cancellationToken);
                response = await reader.ReadToEndAsync(cancellationToken);
                if(response != "ok") {
                    break;
                }
            }

        }
    }
}
