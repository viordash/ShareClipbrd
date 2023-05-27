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

        public DataClient(
            ISystemConfiguration systemConfiguration
            ) {
            Guard.NotNull(systemConfiguration, nameof(systemConfiguration));
            this.systemConfiguration = systemConfiguration;

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

        public async Task SendFileDropList(StringCollection files) {
            var cancellationToken = cts.Token;
            using TcpClient tcpClient = new();
            var stream = await Connect(tcpClient, cancellationToken);
            await SendHeader(ClipboardData.Format.ZipArchive, files.Count, stream, cancellationToken);

            using(var archive = new ZipArchive(stream, ZipArchiveMode.Create)) {
                foreach(var file in files.Cast<string>()) {
                    if(File.GetAttributes(file).HasFlag(FileAttributes.Directory)) {
                        Debug.WriteLine($"dir: {file}");
                        ArchiveDirectory(file, archive);
                    } else {
                        archive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Fastest);
                    }
                }
            }
        }

        static void ArchiveDirectory(string directory, ZipArchive archive) {
            var files = DirectoryHelper.RecursiveGetFiles(directory);
            var emptyFolders = DirectoryHelper.RecursiveGetEmptyFolders(directory);

            var parentPath = Path.GetDirectoryName(directory);
            var directoryName = Path.GetRelativePath(parentPath!, directory);

            foreach(var file in files) {
                var fileDirectory = Path.GetDirectoryName(file);
                var relative = Path.GetRelativePath(parentPath!, fileDirectory!);
                archive.CreateEntryFromFile(file, Path.Combine(relative, Path.GetFileName(file)), CompressionLevel.Fastest);
            }

            foreach(var folder in emptyFolders) {
                var relative = Path.GetRelativePath(directory, folder);
                var childDirectory = Path.Combine(directoryName, relative);

                var directoryEntry = archive.CreateEntry(childDirectory, CompressionLevel.Fastest);
                directoryEntry.ExternalAttributes = (int)File.GetAttributes(folder);
            }

            if(!files.Any() && !emptyFolders.Any()) {
                var directoryEntry = archive.CreateEntry(directoryName, CompressionLevel.Fastest);
                directoryEntry.ExternalAttributes = (int)File.GetAttributes(directory);
            }
        }

        static async Task SendHeader(string format, Int64 dataSize, NetworkStream stream, CancellationToken cancellationToken) {
            await stream.WriteAsync(format, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessFormat) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException($"Others do not support clipboard format: {format}");
            }

            var size = dataSize;
            await stream.WriteAsync(size, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessSize) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException($"Others do not support size: {size}");
            }
        }

        public async Task SendData(ClipboardData clipboardData) {
            var cancellationToken = cts.Token;
            using TcpClient tcpClient = new();
            var stream = await Connect(tcpClient, cancellationToken);

            foreach(var clipboard in clipboardData.Formats) {
                await SendHeader(clipboard.Format, clipboard.Stream.Length, stream, cancellationToken);
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
