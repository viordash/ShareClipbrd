using System.Diagnostics;
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
            Memory<byte> receiveBuffer = new byte[65536];
            var cancellationToken = cts.Token;
            using TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(systemConfiguration.HostAddress.Address, systemConfiguration.HostAddress.Port, cancellationToken);

            Debug.WriteLine($"tcpClient connected  {tcpClient.Client.LocalEndPoint}");

            int receivedBytes;
            var stream = tcpClient.GetStream();

            foreach(var format in clipboardData.Formats) {
                await stream.WriteAsync(Encoding.ASCII.GetBytes(format.Key));
                await stream.FlushAsync();

                Debug.WriteLine($"tcpClient send format  {format.Key}");
                Debug.WriteLine($"tcpClient read response");

                receivedBytes = await stream.ReadAsync(receiveBuffer, cancellationToken);
                if(receivedBytes == 0) {
                    break;
                }

                var response = Encoding.ASCII.GetString(receiveBuffer[..receivedBytes].ToArray());
                if(response != format.Key) {
                    continue;
                }
                await stream.WriteAsync(format.Value, cancellationToken);
                await stream.FlushAsync();

                response = Encoding.ASCII.GetString(receiveBuffer[..receivedBytes].ToArray());
                if(response != "Ok") {
                    break;
                }
            }
        }
    }
}
