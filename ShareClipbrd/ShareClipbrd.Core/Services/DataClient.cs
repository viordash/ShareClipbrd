using Clipboard.Core;
using GuardNet;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Extensions;
using ShareClipbrd.Core.Helpers;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Timers;


namespace ShareClipbrd.Core.Services {
    public interface IDataClient {
        Task SendFileDropList(StringCollection files);
        Task SendData(ClipboardData clipboardData);
        void Start();
        void Stop();
    }

    public class DataClient : IDataClient {
        public const string OnFlyPrefix = "on-fly";
        readonly ISystemConfiguration systemConfiguration;
        readonly IProgressService progressService;
        readonly IConnectStatusService connectStatusService;
        readonly IDialogService dialogService;
        readonly System.Timers.Timer pingTimer;
        readonly ITimeService timeService;
        readonly IAddressDiscoveryService addressDiscoveryService;
        TcpClient client;
        CancellationTokenSource cts;
        readonly SemaphoreSlim semaphore = new(1);
        readonly HashSet<IPAddress> badIpAdresses = new();

        public DataClient(
            ISystemConfiguration systemConfiguration,
            IProgressService progressService,
            IConnectStatusService connectStatusService,
            IDialogService dialogService,
            ITimeService timeService,
            IAddressDiscoveryService addressDiscoveryService
            ) {
            Guard.NotNull(systemConfiguration, nameof(systemConfiguration));
            Guard.NotNull(progressService, nameof(progressService));
            Guard.NotNull(connectStatusService, nameof(connectStatusService));
            Guard.NotNull(dialogService, nameof(dialogService));
            Guard.NotNull(timeService, nameof(timeService));
            Guard.NotNull(addressDiscoveryService, nameof(addressDiscoveryService));
            this.systemConfiguration = systemConfiguration;
            this.progressService = progressService;
            this.connectStatusService = connectStatusService;
            this.dialogService = dialogService;
            this.timeService = timeService;
            this.addressDiscoveryService = addressDiscoveryService;

            client = new();
            cts = new();
            pingTimer = new();
            pingTimer.AutoReset = false;
            pingTimer.Elapsed += OnPingTimerEvent;
        }

        async ValueTask<NetworkStream> Handshake(CancellationToken cancellationToken) {
            var stream = client.GetStream();
            await stream.WriteAsync(CommunProtocol.Version, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessVersion) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException("Wrong version of the other side");
            }
            connectStatusService.ClientOnline();
            return stream;
        }

        public async Task SendFileDropList(StringCollection fileDropList) {
            cts.Cancel();
            cts = new();
            var cancellationToken = cts.Token;
            try {
                await Connect(cancellationToken);
                if(!IsSocketConnected(client.Client)) {
                    return;
                }
                MarkAsGoodAddress();
                var stream = await Handshake(cancellationToken);
                var fileTransmitter = new FileTransmitter(progressService, stream);
                await fileTransmitter.Send(fileDropList, cancellationToken);
            } catch(SocketException ex) {
                await dialogService.ShowError(ex);
            } catch(IOException ex) {
                await dialogService.ShowError(ex);
            } catch(ArgumentException ex) {
                await dialogService.ShowError(ex);
            } catch(OperationCanceledException) {
                client.Close();
            } finally {
                pingTimer.Enabled = !cancellationToken.IsCancellationRequested;
            }
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
            cts.Cancel();
            cts = new();
            var cancellationToken = cts.Token;
            try {
                await Connect(cancellationToken);
                if(!IsSocketConnected(client.Client)) {
                    return;
                }
                MarkAsGoodAddress();
                await using(progressService.Begin(ProgressMode.Send)) {
                    var totalLenght = clipboardData.GetTotalLenght();
                    progressService.SetMaxTick(totalLenght);
                    var stream = await Handshake(cancellationToken);


                    await stream.WriteAsync(totalLenght, cancellationToken);
                    if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessSize) {
                        await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                        throw new NotSupportedException($"Others do not support total: {totalLenght}");
                    }

                    for(var i = 0; i < clipboardData.Formats.Count; i++) {
                        var clipboard = clipboardData.Formats[i];
                        progressService.Tick(clipboard.Stream.Length);
                        await SendFormat(clipboard.Format, stream, cancellationToken);
                        await SendSize(clipboard.Stream.Length, stream, cancellationToken);
                        clipboard.Stream.Position = 0;

                        await clipboard.Stream.CopyToAsync(stream, cancellationToken);

                        if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessData) {
                            await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                            throw new NotSupportedException($"Transfer data error");
                        }

                        var moreData = i < clipboardData.Formats.Count - 1;
                        if(moreData) {
                            await stream.WriteAsync(CommunProtocol.MoreData, cancellationToken);
                        }
                    }
                    await stream.WriteAsync(CommunProtocol.Finish, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }
            } catch(SocketException ex) {
                await dialogService.ShowError(ex);
            } catch(IOException ex) {
                Debug.WriteLine($"tcpClient IO exception {ex}");
            } catch(ArgumentException ex) {
                await dialogService.ShowError(ex);
            } catch(InvalidOperationException ex) {
                await dialogService.ShowError(ex);
            } catch(OperationCanceledException) {
                client.Close();
            } finally {
                pingTimer.Enabled = !cancellationToken.IsCancellationRequested;
            }
        }

        async Task Connect(CancellationToken cancellationToken) {
            pingTimer.Enabled = false;
            var connected = IsSocketConnected(client.Client);
            if(connected) {
                return;
            }
            connectStatusService.ClientOffline();
            IPEndPoint ipEndPoint;
            var partnerId = string.Empty;
            var hostId = string.Empty;
            bool useHostAddressOrDefaulId = string.IsNullOrEmpty(systemConfiguration.PartnerAddress);
            if((!useHostAddressOrDefaulId && AddressResolver.UseAddressDiscoveryService(systemConfiguration.PartnerAddress, out partnerId, out var mandatoryPort))
                    || AddressResolver.UseAddressDiscoveryService(systemConfiguration.HostAddress, out hostId, out mandatoryPort)) {
                if(string.IsNullOrEmpty(partnerId) && string.IsNullOrEmpty(hostId)) {
                    return;
                }
                string id;

                if(!string.IsNullOrEmpty(partnerId)) {
                    id = partnerId;
                } else if(!string.IsNullOrEmpty(hostId)) {
                    id = hostId;
                } else {
                    return;
                }

                try {
                    ipEndPoint = await addressDiscoveryService.Discover(id, badIpAdresses.ToList());
                } catch(OperationCanceledException) {
                    badIpAdresses.Clear();
                    Debug.WriteLine($"badIpAdresses cleared");
                    throw;
                }
            } else {
                ipEndPoint = NetworkHelper.ResolveHostName(systemConfiguration.PartnerAddress);
            }

            await semaphore.WaitAsync();
            try {
                client.Close();
                if(string.IsNullOrEmpty(partnerId) && string.IsNullOrEmpty(hostId)) {
                    return;
                }
                client = new();

                using var timed_cts = new CancellationTokenSource(systemConfiguration.ClientTimeout);
                using var ccts = CancellationTokenSource.CreateLinkedTokenSource(timed_cts.Token, cancellationToken);
                badIpAdresses.Add(ipEndPoint.Address.MapToIPv6());
                await client.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port, ccts.Token);
            } finally {
                semaphore.Release();
            }
        }

        static bool IsSocketConnected(Socket s) {
            if(s == null) {
                return false;
            }
            try {
                return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
            } catch(ObjectDisposedException) {
                return false;
            }
        }

        async Task Ping() {
            var cancellationToken = cts.Token;
            try {
                await Connect(cancellationToken);
                if(!IsSocketConnected(client.Client)) {
                    return;
                }
                MarkAsGoodAddress();
                var stream = await Handshake(cancellationToken);
                await stream.WriteAsync((Int64)0, cancellationToken);
                await stream.ReadUInt16Async(cancellationToken);
            } catch(ArgumentException ex) {
                await dialogService.ShowError(ex);
            } catch(OperationCanceledException) {
                Debug.WriteLine($"Ping canceled");
            } catch(Exception) {
            }
            pingTimer.Enabled = !cancellationToken.IsCancellationRequested;
            if(pingTimer.Interval != timeService.DataClientPingPeriod.TotalMilliseconds) {
                pingTimer.Interval = timeService.DataClientPingPeriod.TotalMilliseconds;
            }
        }

        void OnPingTimerEvent(object? source, ElapsedEventArgs e) {
            _ = Ping();
        }

        public void Start() {
            pingTimer.Enabled = true;
        }

        public void Stop() {
            pingTimer.Enabled = false;
            cts.Cancel();
            cts = new();
            client.Close();
            badIpAdresses.Clear();
        }

        void MarkAsGoodAddress() {
            if(client.Client?.RemoteEndPoint is IPEndPoint iPEndPoint) {
                badIpAdresses.Remove(iPEndPoint.Address.MapToIPv6());
                Debug.WriteLine($"MarkAsGoodAddress IpAdress:{iPEndPoint.Address}");
            }
        }
    }
}
