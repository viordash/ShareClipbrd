using System.Diagnostics;
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

        static async Task SendFile(FileStream fileStream, NetworkStream stream, CancellationToken cancellationToken) {
            var filename = Path.GetFileName(fileStream.Name);
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
            foreach(var file in files) {
                var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                var clipboard = new ClipboardItem(ClipboardData.Format.FileDrop, fileStream);
                await SendClipboardItem(clipboard, stream, cancellationToken);
                fileStream.Close();
            }
            foreach(var folder in emptyFolders) {
                var relative = Path.GetRelativePath(directory, folder);
                var clipboard = new ClipboardItem(ClipboardData.Format.DirectoryDrop, new MemoryStream(Encoding.UTF8.GetBytes(relative)));
                await SendClipboardItem(clipboard, stream, cancellationToken);
            }
        }


        static async Task SendClipboardItem(ClipboardItem clipboard, NetworkStream stream, CancellationToken cancellationToken) {
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

            if(clipboard.Data is MemoryStream dataMemoryStream) {
                dataMemoryStream.Position = 0;

                await dataMemoryStream.CopyToAsync(stream, cancellationToken);
            } else if(clipboard.Data is FileStream fileStream) {
                await SendFile(fileStream, stream, cancellationToken);
            }

            if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessData) {
                await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                throw new NotSupportedException($"Transfer data error");
            }
        }

        public async Task Send(ClipboardData clipboardData) {
            var cancellationToken = cts.Token;
            using TcpClient tcpClient = new();

            var adr = NetworkHelper.ResolveHostName(systemConfiguration.PartnerAddress);
            Debug.WriteLine($"        --- tcpClient connecting  {adr.ToString()}");
            await tcpClient.ConnectAsync(adr.Address, adr.Port, cancellationToken);

            Debug.WriteLine($"        --- tcpClient connected  {tcpClient.Client.LocalEndPoint}");

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

                await SendClipboardItem(clipboard, stream, cancellationToken);
            }
        }
    }
}
