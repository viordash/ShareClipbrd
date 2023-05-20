using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using GuardNet;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Extensions;
using ShareClipbrd.Core.Helpers;


namespace ShareClipbrd.Core.Services {
    public interface IDataClient {
        Task Send(ClipboardData clipboardData);
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

        static async Task SendFile(string? relativeTo, FileStream fileStream, NetworkStream stream, CancellationToken cancellationToken) {
            var filename = string.IsNullOrEmpty(relativeTo)
                ? Path.GetFileName(fileStream.Name)
                : Path.Combine(relativeTo, Path.GetFileName(fileStream.Name));

            await stream.WriteAsync(filename, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessFilename) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException($"Others can't receive file: {filename}");
            }
            fileStream.Position = 0;
            await fileStream.CopyToAsync(stream, cancellationToken);
        }


        static async Task SendDirectory(string directory, NetworkStream stream, CancellationToken cancellationToken) {
            var files = DirectoryHelper.RecursiveGetFiles(directory);
            var emptyFolders = DirectoryHelper.RecursiveGetEmptyFolders(directory);

            var parentPath = Path.GetDirectoryName(directory);
            var directoryName = Path.GetRelativePath(parentPath!, directory);

            foreach(var file in files) {
                var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                var clipboard = new ClipboardItem(ClipboardData.Format.FileDrop, fileStream);
                await SendHeader(clipboard, stream, cancellationToken);
                var fileDirectory = Path.GetDirectoryName(file);
                var relative = Path.GetRelativePath(parentPath!, fileDirectory!);

                await SendFile(relative, fileStream, stream, cancellationToken);
                fileStream.Close();
                if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessData) {
                    await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException($"Transfer data error");
                }
            }
            foreach(var folder in emptyFolders) {
                var relative = Path.GetRelativePath(directory, folder);
                var childDirectory = Path.Combine(directoryName, relative);

                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(childDirectory));
                var clipboard = new ClipboardItem(ClipboardData.Format.DirectoryDrop, memoryStream);
                await SendHeader(clipboard, stream, cancellationToken);
                await memoryStream.CopyToAsync(stream, cancellationToken);
                if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessData) {
                    await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException($"Transfer data error");
                }
            }

            if(!files.Any() && !emptyFolders.Any()) {
                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(directoryName));
                var clipboard = new ClipboardItem(ClipboardData.Format.DirectoryDrop, memoryStream);
                await SendHeader(clipboard, stream, cancellationToken);
                await memoryStream.CopyToAsync(stream, cancellationToken);
                if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessData) {
                    await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException($"Transfer data error");
                }
            }
        }

        static async Task SendHeader(ClipboardItem clipboard, NetworkStream stream, CancellationToken cancellationToken) {
            await stream.WriteAsync(clipboard.Format, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessFormat) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException($"Others do not support clipboard format: {clipboard.Format}");
            }

            var size = clipboard.Data.Length;
            await stream.WriteAsync(size, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessSize) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException($"Others do not support size: {size}");
            }
        }

        public async Task Send(ClipboardData clipboardData) {
            var cancellationToken = cts.Token;
            using TcpClient tcpClient = new();

            var adr = NetworkHelper.ResolveHostName(systemConfiguration.PartnerAddress);
            await tcpClient.ConnectAsync(adr.Address, adr.Port, cancellationToken);

            var stream = tcpClient.GetStream();

            await stream.WriteAsync(CommunProtocol.Version, cancellationToken);
            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessVersion) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException("Wrong version of the other side");
            }

            foreach(var clipboard in clipboardData.Formats) {
                if(clipboard.Format == ClipboardData.Format.DirectoryDrop) {
                    if(clipboard.Data is MemoryStream memoryStream) {
                        var directory = Encoding.UTF8.GetString(memoryStream.ToArray());
                        await SendDirectory(directory, stream, cancellationToken);
                        continue;
                    }
                    throw new NotSupportedException($"Data error, clipboard format: {clipboard.Format}");
                }

                await SendHeader(clipboard, stream, cancellationToken);

                if(clipboard.Data is MemoryStream dataMemoryStream) {
                    dataMemoryStream.Position = 0;

                    await dataMemoryStream.CopyToAsync(stream, cancellationToken);
                } else if(clipboard.Data is FileStream fileStream) {
                    await SendFile(string.Empty, fileStream, stream, cancellationToken);
                }

                if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessData) {
                    await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException($"Transfer data error");
                }
            }
        }
    }
}
