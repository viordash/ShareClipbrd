using System;
using System.Buffers;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
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
        readonly IClipboardSerializer clipboardSerializer;
        readonly CancellationTokenSource cts;

        public DataServer(
            ISystemConfiguration systemConfiguration,
            IDialogService dialogService,
            IClipboardSerializer clipboardSerializer
            ) {
            Guard.NotNull(systemConfiguration, nameof(systemConfiguration));
            Guard.NotNull(dialogService, nameof(dialogService));
            Guard.NotNull(clipboardSerializer, nameof(clipboardSerializer));
            this.systemConfiguration = systemConfiguration;
            this.dialogService = dialogService;
            this.clipboardSerializer = clipboardSerializer;

            cts = new CancellationTokenSource();
        }

        static async ValueTask<Stream> HandleFile(NetworkStream stream, int dataSize, CancellationToken cancellationToken) {
            Debug.WriteLine($"tcpServer read filename");
            var filename = await stream.ReadASCIIStringAsync(cancellationToken);
            Debug.WriteLine($"tcpServer readed filename: '{filename}'");
            if(string.IsNullOrEmpty(filename)) {
                throw new NotSupportedException("Filename receive error");
            }
            await stream.WriteAsync(CommunProtocol.SuccessFilename, cancellationToken);

            var tempFilename = Path.Combine(Path.GetTempPath(), filename);
            using(var fileStream = new FileStream(tempFilename, FileMode.OpenOrCreate)) {
                byte[] receiveBuffer = ArrayPool<byte>.Shared.Rent(CommunProtocol.ChunkSize);
                while(fileStream.Length < dataSize) {
                    int receivedBytes = await stream.ReadAsync(receiveBuffer, cancellationToken);
                    if(receivedBytes == 0) {
                        break;
                    }
                    await fileStream.WriteAsync(new ReadOnlyMemory<byte>(receiveBuffer, 0, receivedBytes), cancellationToken);
                }
                await stream.WriteAsync(CommunProtocol.SuccessData, cancellationToken);
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(tempFilename));
        }

        static async ValueTask<Stream> HandleData(NetworkStream stream, int dataSize, CancellationToken cancellationToken) {
            var memoryStream = new MemoryStream(dataSize);
            byte[] receiveBuffer = ArrayPool<byte>.Shared.Rent(CommunProtocol.ChunkSize);
            while(memoryStream.Length < dataSize) {
                int receivedBytes = await stream.ReadAsync(receiveBuffer, cancellationToken);
                if(receivedBytes == 0) {
                    break;
                }
                await memoryStream.WriteAsync(new ReadOnlyMemory<byte>(receiveBuffer, 0, receivedBytes), cancellationToken);
            }

            await stream.WriteAsync(CommunProtocol.SuccessData, cancellationToken);
            return memoryStream;
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


                    if(clipboardSerializer.FormatForFiles(format)) {
                        clipboardData.Add(format, await HandleFile(stream, (int)size, cancellationToken));
                    } else if(clipboardSerializer.FormatForImage(format)) {

                    } else if(clipboardSerializer.FormatForAudio(format)) {

                    } else {
                        clipboardData.Add(format, await HandleData(stream, (int)size, cancellationToken));
                    }

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
