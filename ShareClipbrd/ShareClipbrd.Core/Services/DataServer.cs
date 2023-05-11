using System.Diagnostics;
using System.Net.Sockets;
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
        readonly CancellationTokenSource cts;

        public DataServer(
            ISystemConfiguration systemConfiguration,
            IDialogService dialogService
            ) {
            Guard.NotNull(systemConfiguration, nameof(systemConfiguration));
            Guard.NotNull(dialogService, nameof(dialogService));
            this.systemConfiguration = systemConfiguration;
            this.dialogService = dialogService;

            cts = new CancellationTokenSource();
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

                            var stream = tcpClient.GetStream();

                            using var binaryReader = new BinaryReader(stream);
                            using var binaryWriter = new BinaryWriter(stream);

                            var format = binaryReader.ReadString();

                            string id = Guid.NewGuid().ToString();

                            binaryWriter.Write(id);
                            binaryWriter.Flush();

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
