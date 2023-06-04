using System.Collections.Specialized;
using System.Net.Sockets;
using GuardNet;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Extensions;
using ShareClipbrd.Core.Helpers;


namespace ShareClipbrd.Core.Services {
    public interface IDataClient {
        Task SendFileDropList(StringCollection files);
        Task SendData(ClipboardData clipboardData);
    }

    public class DataClient : IDataClient {
        readonly ISystemConfiguration systemConfiguration;
        readonly CancellationTokenSource cts;
        readonly IDispatchService dispatchService;
        readonly IProgressService progressService;

        public DataClient(
            ISystemConfiguration systemConfiguration,
            IDispatchService dispatchService,
            IProgressService progressService
            ) {
            Guard.NotNull(systemConfiguration, nameof(systemConfiguration));
            Guard.NotNull(dispatchService, nameof(dispatchService));
            Guard.NotNull(progressService, nameof(progressService));
            this.systemConfiguration = systemConfiguration;
            this.dispatchService = dispatchService;
            this.progressService = progressService;

            cts = new CancellationTokenSource();
        }

        async ValueTask<NetworkStream> Connect(TcpClient tcpClient, CancellationToken cancellationToken) {
            var adr = NetworkHelper.ResolveHostName(systemConfiguration.PartnerAddress);
            await tcpClient.ConnectAsync(adr.Address, adr.Port, cancellationToken);

            var stream = tcpClient.GetStream();

            await stream.WriteAsync(CommunProtocol.Version, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessVersion) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException("Wrong version of the other side");
            }
            return stream;
        }

        public async Task SendFileDropList(StringCollection fileDropList) {
            using TcpClient tcpClient = new();

            var cancellationToken = cts.Token;
            var stream = await Connect(tcpClient, cancellationToken);
            var fileTransmitter = new FileTransmitter(progressService, stream);
            await fileTransmitter.Send(fileDropList, cancellationToken);
        }

        static async Task SendFormat(string format, NetworkStream stream, CancellationToken cancellationToken) {
            await stream.WriteAsync(format, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessFormat) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException($"Others do not support clipboard format: {format}");
            }
        }

        static async Task SendSize(Int64 size, NetworkStream stream, CancellationToken cancellationToken) {
            await stream.WriteAsync(size, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessSize) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException($"Others do not support size: {size}");
            }
        }

        public async Task SendData(ClipboardData clipboardData) {
            await using(progressService.Begin(ProgressMode.Send)) {
                var totalLenght = clipboardData.GetTotalLenght();
                progressService.SetMaxTick(totalLenght);
                var cancellationToken = cts.Token;
                using TcpClient tcpClient = new();
                var stream = await Connect(tcpClient, cancellationToken);

                await stream.WriteAsync(totalLenght, cancellationToken);
                if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessSize) {
                    await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException($"Others do not support total: {totalLenght}");
                }

                foreach(var clipboard in clipboardData.Formats) {
                    progressService.Tick(clipboard.Stream.Length);
                    await SendFormat(clipboard.Format, stream, cancellationToken);
                    await SendSize(clipboard.Stream.Length, stream, cancellationToken);
                    clipboard.Stream.Position = 0;

                    await clipboard.Stream.CopyToAsync(stream, cancellationToken);

                    if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessData) {
                        await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                        throw new NotSupportedException($"Transfer data error");
                    }
                }
            }
        }
    }
}
