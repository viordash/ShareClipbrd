using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using GuardNet;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Extensions;

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

            Debug.WriteLine($"        --- tcpClient connected  {tcpClient.Client.LocalEndPoint}");

            var stream = tcpClient.GetStream();

            await stream.WriteAsync(CommunProtocol.Version, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessVersion) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException("Wrong version of the other side");
            }

            foreach(var format in clipboardData.Formats) {
                Debug.WriteLine($"        --- tcpClient send format: {format.Key}");
                await stream.WriteAsync(format.Key, cancellationToken);
                Debug.WriteLine($"        --- tcpClient read format ack");
                if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessFormat) {
                    await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException($"Others do not support clipboard format: {format.Key}");
                }

                Debug.WriteLine($"        --- tcpClient send size: {format.Value.Length}");
                var size = format.Value.Length;
                await stream.WriteAsync(size, cancellationToken);
                Debug.WriteLine($"        --- tcpClient read size ack");
                if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessSize) {
                    await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException($"Others do not support size: {size}");
                }
                int start = 0;
                while(size > 0) {
                    //Debug.WriteLine($"        --- tcpClient send data, transfered: {start}");
                    int end = start + Math.Min(CommunProtocol.ChunkSize, size);
                    var chunk = format.Value[start..end];
                    await stream.WriteAsync(chunk, cancellationToken);
                    size -= chunk.Length;
                    start = end;
                }

                Debug.WriteLine($"        --- tcpClient read data ack");

                if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessData) {
                    await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException($"Transfer data error");
                }
            }
            Debug.WriteLine($"        --- tcpClient success finished");
        }
    }
}
