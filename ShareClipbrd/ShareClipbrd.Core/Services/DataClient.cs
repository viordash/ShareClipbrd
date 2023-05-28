using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
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

        static void FlatFilesList(StringCollection fileDropList, out Dictionary<string, List<string>> files,
                out Dictionary<string, List<string>?> directories) {
            files = new Dictionary<string, List<string>>() {
                { string.Empty, fileDropList
                                .Cast<string>()
                                .Where(x => !File.GetAttributes(x).HasFlag(FileAttributes.Directory))
                                .Distinct()
                                .ToList()
                }
            };

            directories = new Dictionary<string, List<string>?>();

            foreach(var dir in fileDropList.Cast<string>()
                                .Where(x => File.GetAttributes(x).HasFlag(FileAttributes.Directory))
                                .Distinct()) {

                var dirFiles = DirectoryHelper.RecursiveGetFiles(dir);
                var emptyFolders = DirectoryHelper.RecursiveGetEmptyFolders(dir);

                if(!dirFiles.Any() && !emptyFolders.Any()) {
                    directories[dir] = null;
                    continue;
                }

                if(dirFiles.Any()) {
                    files[dir] = dirFiles;
                }

                if(emptyFolders.Any()) {
                    directories[dir] = emptyFolders;
                }
            }
        }

        public async Task SendFileDropList(StringCollection fileDropList) {
            FlatFilesList(fileDropList, out Dictionary<string, List<string>> files, out Dictionary<string, List<string>?> directories);

            var total = files.Values.Sum(x => x.Count) + directories.Values.Sum(x => x?.Count ?? 1);

            await using(_ = progressService.Begin(total, ProgressMode.Send)) {
                var cancellationToken = cts.Token;
                var compressionLevel = CompressionLevelHelper.GetLevel(systemConfiguration.Compression);
                using TcpClient tcpClient = new();

                var stream = await Connect(tcpClient, total, cancellationToken);
                await SendFormat(ClipboardData.Format.ZipArchive, stream, cancellationToken);

                using(var archive = new ZipArchive(stream, ZipArchiveMode.Create)) {
                    foreach(var parent in directories) {
                        var parentPath = Path.GetDirectoryName(parent.Key);
                        var directoryName = Path.GetRelativePath(parentPath!, parent.Key);

                        if(parent.Value == null) {
                            progressService.Tick(1);
                            var archiveEntry = archive.CreateEntry(directoryName, compressionLevel);
                            archiveEntry.ExternalAttributes = (int)File.GetAttributes(parent.Key);
                        } else {
                            foreach(var directory in parent.Value) {
                                progressService.Tick(1);
                                var relative = Path.GetRelativePath(parent.Key, directory);
                                var childDirectory = Path.Combine(directoryName, relative);
                                var archiveEntry = archive.CreateEntry(childDirectory, compressionLevel);
                                archiveEntry.ExternalAttributes = (int)File.GetAttributes(directory);
                            }
                        }
                    }

                    var filesInRoot = files
                        .Where(x => string.IsNullOrEmpty(x.Key))
                        .SelectMany(x => x.Value);
                    foreach(var file in filesInRoot) {
                        progressService.Tick(1);
                        var archiveEntry = archive.CreateEntryFromFile(file, Path.GetFileName(file), compressionLevel);
                        archiveEntry.ExternalAttributes = (int)File.GetAttributes(file);
                    }

                    foreach(var parent in files.Where(x => !string.IsNullOrEmpty(x.Key))) {
                        var parentPath = Path.GetDirectoryName(parent.Key);
                        var directoryName = Path.GetRelativePath(parentPath!, parent.Key);

                        foreach(var file in parent.Value) {
                            progressService.Tick(1);
                            var fileDirectory = Path.GetDirectoryName(file);
                            var relative = Path.GetRelativePath(parentPath!, fileDirectory!);
                            var archiveEntry = archive.CreateEntryFromFile(file, Path.Combine(relative, Path.GetFileName(file)), compressionLevel);
                            archiveEntry.ExternalAttributes = (int)File.GetAttributes(file);
                        }
                    }
                }
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
