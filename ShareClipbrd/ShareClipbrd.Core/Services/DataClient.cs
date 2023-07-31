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
        Task Connect();
        void Disconnect();
    }

    public class DataClient : IDataClient {
        readonly ISystemConfiguration systemConfiguration;
        readonly IDispatchService dispatchService;
        readonly IProgressService progressService;
        readonly IConnectStatusService connectStatusService;
        readonly IDialogService dialogService;
        readonly TcpClient client;
        CancellationTokenSource cts;

        public DataClient(
            ISystemConfiguration systemConfiguration,
            IDispatchService dispatchService,
            IProgressService progressService,
            IConnectStatusService connectStatusService,
            IDialogService dialogService
            ) {
            Guard.NotNull(systemConfiguration, nameof(systemConfiguration));
            Guard.NotNull(dispatchService, nameof(dispatchService));
            Guard.NotNull(progressService, nameof(progressService));
            Guard.NotNull(connectStatusService, nameof(connectStatusService));
            Guard.NotNull(dialogService, nameof(dialogService));
            this.systemConfiguration = systemConfiguration;
            this.dispatchService = dispatchService;
            this.progressService = progressService;
            this.connectStatusService = connectStatusService;
            this.dialogService = dialogService;

            client = new();
            cts = new();
        }

        async ValueTask<NetworkStream> Handshake() {
            var cancellationToken = cts.Token;
            var stream = client.GetStream();
            await stream.WriteAsync(CommunProtocol.Version, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessVersion) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException("Wrong version of the other side");
            }
            return stream;
        }

        public async Task SendFileDropList(StringCollection fileDropList) {
            var stream = await Handshake();
            var fileTransmitter = new FileTransmitter(progressService, stream);
            await fileTransmitter.Send(fileDropList, cts.Token);
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
                var stream = await Handshake();

                var cancellationToken = cts.Token;

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

        public async Task Connect() {
            cts.Cancel(true);
            cts = new();

            do {
                try {
                    var adr = NetworkHelper.ResolveHostName(systemConfiguration.PartnerAddress);
                    await client.ConnectAsync(adr.Address, adr.Port, cts.Token);

                } catch(SocketException ex) {
                    await dialogService.ShowError(ex);
                } catch(ArgumentException ex) {
                    await dialogService.ShowError(ex);
                }

            } while(!cts.IsCancellationRequested && !client.Connected);
        }

        public void Disconnect() {
            cts.Cancel();
        }
    }
}
