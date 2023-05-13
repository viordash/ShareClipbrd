using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GuardNet;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Extensions;

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

        async ValueTask HandleClient(TcpClient tcpClient, CancellationToken cancellationToken) {
            var clipboardData = new ClipboardData();
            var receiveBuffer = new byte[CommunProtocol.ChunkSize];

            try {
                var stream = tcpClient.GetStream();
                int receivedBytes;


                if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.Version) {
                    await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException("Wrong version of the other side");
                }
                await stream.WriteAsync(CommunProtocol.SuccessVersion, cancellationToken);

                while(!cancellationToken.IsCancellationRequested) {
                    Debug.WriteLine($"tcpServer read format");
                    var format = await stream.ReadASCIIStringAsync(cancellationToken);
                    Debug.WriteLine($"tcpServer readed format: '{format}'");
                    if(string.IsNullOrEmpty(format)) {
                        break;
                    }
                    if(!clipboardService.SupportedFormat(format)) {
                        await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                        throw new NotSupportedException($"Not supported clipboard format: {format}");
                    }
                    await stream.WriteAsync(CommunProtocol.SuccessFormat, cancellationToken);


                    Debug.WriteLine($"tcpServer read size");
                    var size = await stream.ReadInt32Async(cancellationToken);
                    Debug.WriteLine($"tcpServer readed size: {size}");
                    if(!clipboardService.SupportedDataSize(size)) {
                        await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                        throw new NotSupportedException($"Clipboard data size error: {size}");
                    }
                    await stream.WriteAsync(CommunProtocol.SuccessSize, cancellationToken);

                    var memoryStream = new MemoryStream(size);

                    int start = 0;
                    while(start < size) {
                        receivedBytes = await stream.ReadAsync(receiveBuffer, cancellationToken);
                        if(receivedBytes == 0) {
                            break;
                        }
                        memoryStream.Write(receiveBuffer, 0, receivedBytes);
                        start += receivedBytes;
                        //Debug.WriteLine($"tcpServer read data, received: {start}");
                    }

                    if(start != size) {
                        await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                        throw new InvalidDataException($"Clipboard data receive error, {start}!={size}");
                    }
                    await stream.WriteAsync(CommunProtocol.SuccessData, cancellationToken);
                    clipboardData.Add(format, memoryStream.ToArray());
                }
                clipboardService.SetClipboardData(clipboardData);
                Debug.WriteLine($"tcpServer success finished");

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
                            using var tcpClient = await tcpServer.AcceptTcpClientAsync(cancellationToken);
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
