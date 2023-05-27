using System.Collections.Specialized;
using System.Diagnostics;
using System.IO.Compression;
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

        async ValueTask<NetworkStream> Connect(TcpClient tcpClient, Int64 total, CancellationToken cancellationToken) {
            var adr = NetworkHelper.ResolveHostName(systemConfiguration.PartnerAddress);
            await tcpClient.ConnectAsync(adr.Address, adr.Port, cancellationToken);

            var stream = tcpClient.GetStream();

            await stream.WriteAsync(CommunProtocol.Version, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessVersion) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException("Wrong version of the other side");
            }

            await stream.WriteAsync(total, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessSize) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException($"Others do not support total: {total}");
            }
            return stream;
        }

        public async Task SendFileDropList(StringCollection files) {
            await using(_ = progressService.Begin(files.Count, ProgressMode.Send)) {
                var cancellationToken = cts.Token;
                var compressionLevel = CompressionLevelHelper.GetLevel(systemConfiguration.Compression);
                using TcpClient tcpClient = new();
                var stream = await Connect(tcpClient, files.Count, cancellationToken);
                await SendFormat(ClipboardData.Format.ZipArchive, stream, cancellationToken);

                using(var archive = new ZipArchive(stream, ZipArchiveMode.Create)) {
                    foreach(var file in files.Cast<string>()) {
                        progressService.Tick(1);
                        if(File.GetAttributes(file).HasFlag(FileAttributes.Directory)) {
                            Debug.WriteLine($"dir: {file}");
                            ArchiveDirectory(file, archive, compressionLevel);
                        } else {
                            archive.CreateEntryFromFile(file, Path.GetFileName(file), compressionLevel);
                        }
                    }
                }
            }
        }

        static void ArchiveDirectory(string directory, ZipArchive archive, System.IO.Compression.CompressionLevel compressionLevel) {
            var files = DirectoryHelper.RecursiveGetFiles(directory);
            var emptyFolders = DirectoryHelper.RecursiveGetEmptyFolders(directory);

            var parentPath = Path.GetDirectoryName(directory);
            var directoryName = Path.GetRelativePath(parentPath!, directory);

            foreach(var file in files) {
                var fileDirectory = Path.GetDirectoryName(file);
                var relative = Path.GetRelativePath(parentPath!, fileDirectory!);
                archive.CreateEntryFromFile(file, Path.Combine(relative, Path.GetFileName(file)), compressionLevel);
            }

            foreach(var folder in emptyFolders) {
                var relative = Path.GetRelativePath(directory, folder);
                var childDirectory = Path.Combine(directoryName, relative);

                var directoryEntry = archive.CreateEntry(childDirectory, compressionLevel);
                directoryEntry.ExternalAttributes = (int)File.GetAttributes(folder);
            }

            if(!files.Any() && !emptyFolders.Any()) {
                var directoryEntry = archive.CreateEntry(directoryName, compressionLevel);
                directoryEntry.ExternalAttributes = (int)File.GetAttributes(directory);
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
            var totalLenght = clipboardData.GetTotalLenght();
            await using(_ = progressService.Begin(totalLenght, ProgressMode.Send)) {
                var cancellationToken = cts.Token;
                using TcpClient tcpClient = new();
                var stream = await Connect(tcpClient, totalLenght, cancellationToken);

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
