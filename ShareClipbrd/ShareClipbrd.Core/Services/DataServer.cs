using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GuardNet;
using ShareClipbrd.Core.Configuration;

namespace ShareClipbrd.Core.Services {
    public interface IDataServer {
        void Start();
        void Stop();
    }

    public class DataServer : IDataServer {
        readonly ISystemConfiguration systemConfiguration;
        readonly IDialogService dialogService;
        readonly IClipboardService clipboardService;
        readonly CancellationTokenSource cts;

        public DataServer(
            ISystemConfiguration systemConfiguration,
            IDialogService dialogService,
            IClipboardService clipboardService
            ) {
            Guard.NotNull(systemConfiguration, nameof(systemConfiguration));
            Guard.NotNull(dialogService, nameof(dialogService));
            Guard.NotNull(clipboardService, nameof(clipboardService));
            this.systemConfiguration = systemConfiguration;
            this.dialogService = dialogService;
            this.clipboardService = clipboardService;

            cts = new CancellationTokenSource();
        }

        async Task HandleClient(TcpClient tcpClient, CancellationToken cancellationToken) {
            Memory<byte> receiveBuffer = new byte[65536];
            try {
                var stream = tcpClient.GetStream();
                int receivedBytes;
                while(!cancellationToken.IsCancellationRequested) {
                    Debug.WriteLine($"tcpServer read format");

                    receivedBytes = await stream.ReadAsync(receiveBuffer, cancellationToken);
                    if(receivedBytes == 0) {
                        break;
                    }
                    var format = Encoding.ASCII.GetString(receiveBuffer[..receivedBytes].ToArray());

                    Debug.WriteLine($"tcpServer readed format: {format}");
                    if(!clipboardService.SupportedFormat(format)) {
                        await stream.WriteAsync(Encoding.ASCII.GetBytes("Err"), cancellationToken);
                        continue;
                    }
                    await stream.WriteAsync(Encoding.ASCII.GetBytes(format), cancellationToken);

                    receivedBytes = await stream.ReadAsync(receiveBuffer, cancellationToken);
                    if(receivedBytes == 0) {
                        break;
                    }
                    Debug.WriteLine($"tcpServer readed data size: {receivedBytes}");

                    await stream.WriteAsync(Encoding.ASCII.GetBytes("Ok"), cancellationToken);
                }
                tcpClient.Close();
            } catch(OperationCanceledException ex) {
                Debug.WriteLine($"tcpServer canceled {ex}");
            } catch(Exception ex) {
                dialogService.ShowError(ex);
            }
        }

        public void Start() {
            var cancellationToken = cts.Token;
            Task.Run(async () => {

                while(!cancellationToken.IsCancellationRequested) {
                    var tcpServer = new TcpListener(systemConfiguration.HostAddress.Address, systemConfiguration.HostAddress.Port);
                    try {
                        Debug.WriteLine($"start tcpServer: {systemConfiguration.HostAddress}");
                        tcpServer.Start();

                        while(!cancellationToken.IsCancellationRequested) {
                            var tcpClient = await tcpServer.AcceptTcpClientAsync(cancellationToken);
                            Debug.WriteLine($"tcpServer accept  {tcpClient.Client.RemoteEndPoint}");

                            await HandleClient(tcpClient, cancellationToken);
                        }
                    } catch(OperationCanceledException ex) {
                        Debug.WriteLine($"tcpServer canceled {ex}");
                    } catch(Exception ex) {
                        dialogService.ShowError(ex);
                    }

                    Debug.WriteLine($"tcpServer stop");
                    tcpServer.Stop();
                }

                Debug.WriteLine($"tcpServer stopped");
            }, cancellationToken);
        }

        public void Stop() {
            Debug.WriteLine($"tcpServer request to stop");
            cts.Cancel();
        }
    }
}
