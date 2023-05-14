using System;
using System.Buffers;
using System.Diagnostics;
using System.Net.Sockets;
using GuardNet;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Extensions;
using ShareClipbrd.Core.Helpers;

namespace ShareClipbrd.Core.Services {
    public interface IDataServer {
        void Start(Action<ClipboardData> onReceiveCb);
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

        async ValueTask HandleClient(TcpClient tcpClient, Action<ClipboardData> onReceiveCb, CancellationToken cancellationToken) {
            var clipboardData = new ClipboardData();

            try {
                var stream = tcpClient.GetStream();


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
                    await stream.WriteAsync(CommunProtocol.SuccessFormat, cancellationToken);

                    Debug.WriteLine($"tcpServer read size");
                    var size = await stream.ReadInt64Async(cancellationToken);
                    Debug.WriteLine($"tcpServer readed size: {size}");
                    await stream.WriteAsync(CommunProtocol.SuccessSize, cancellationToken);


                   
                    var memoryStream = new MemoryStream((int)size);

                    byte[] receiveBuffer = ArrayPool<byte>.Shared.Rent(CommunProtocol.ChunkSize);
                    while(memoryStream.Length < size) {
                        int receivedBytes = await stream.ReadAsync(receiveBuffer, cancellationToken);
                        if(receivedBytes == 0) {
                            break;
                        }
                        await memoryStream.WriteAsync(new ReadOnlyMemory<byte>(receiveBuffer, 0, receivedBytes), cancellationToken);
                    }

                    await stream.WriteAsync(CommunProtocol.SuccessData, cancellationToken);
                    clipboardData.Add(format, memoryStream);
                }
                onReceiveCb(clipboardData);
                Debug.WriteLine($"tcpServer success finished");

            } catch(OperationCanceledException ex) {
                Debug.WriteLine($"tcpServer canceled {ex}");
            } catch(Exception ex) {
                dialogService.ShowError(ex);
            }
        }

        public void Start(Action<ClipboardData> onReceiveCb) {
            var cancellationToken = cts.Token;
            Task.Run(async () => {

                while(!cancellationToken.IsCancellationRequested) {
                    var adr = NetworkHelper.ResolveHostName(systemConfiguration.HostAddress);
                    var tcpServer = new TcpListener(adr.Address, adr.Port);
                    try {
                        Debug.WriteLine($"start tcpServer: {adr}");
                        tcpServer.Start();

                        while(!cancellationToken.IsCancellationRequested) {
                            using var tcpClient = await tcpServer.AcceptTcpClientAsync(cancellationToken);
                            Debug.WriteLine($"tcpServer accept  {tcpClient.Client.RemoteEndPoint}");

                            await HandleClient(tcpClient, onReceiveCb, cancellationToken);
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
