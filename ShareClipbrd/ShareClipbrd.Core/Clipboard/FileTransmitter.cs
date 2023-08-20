using System.Buffers;
using System.Collections.Specialized;
using System.Net.Sockets;
using System.Text;
using Clipboard.Core;
using ShareClipbrd.Core.Extensions;
using ShareClipbrd.Core.Helpers;
using ShareClipbrd.Core.Services;

namespace ShareClipbrd.Core.Clipboard {
    public class FileTransmitter {

        readonly IProgressService progressService;
        readonly NetworkStream networkStream;

        public FileTransmitter(
            IProgressService progressService,
            NetworkStream networkStream) {
            this.progressService = progressService;
            this.networkStream = networkStream;
        }

        async Task SendFile(string parent, string name, CancellationToken cancellationToken) {
            var attributes = File.GetAttributes(name);
            await networkStream.WriteAsync((Int32)attributes, cancellationToken);

            string relative;
            if(attributes.HasFlag(FileAttributes.Directory)) {
                if(string.IsNullOrEmpty(parent)) {
                    var parentPath = Path.GetDirectoryName(name);
                    relative = Path.GetRelativePath(parentPath!, name);
                } else {
                    var parentPath = Path.GetDirectoryName(parent);
                    relative = Path.GetRelativePath(parentPath!, name);
                }
            } else {
                if(string.IsNullOrEmpty(parent)) {
                    relative = Path.GetFileName(name)!;
                } else {
                    var parentPath = Path.GetDirectoryName(parent);
                    relative = Path.GetRelativePath(parentPath!, name);
                }
            }

            relative = relative.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var relativeBytes = Encoding.UTF8.GetBytes(relative);
            await networkStream.WriteAsync((Int32)relativeBytes.Length, cancellationToken);
            await networkStream.WriteAsync(relativeBytes, cancellationToken);

            if(attributes.HasFlag(FileAttributes.Directory)) {
                await networkStream.WriteAsync((Int64)0, cancellationToken);
            } else {

                using(var fileStream = new FileStream(name, FileMode.Open, FileAccess.Read)) {
                    progressService.SetMaxMinorTick(fileStream.Length);
                    await networkStream.WriteAsync((Int64)fileStream.Length, cancellationToken);
                    byte[] buffer = ArrayPool<byte>.Shared.Rent(CommunProtocol.ChunkSize);
                    try {
                        Int64 dataLength = 0;
                        while(dataLength < fileStream.Length) {
                            var sendCount = Math.Min(fileStream.Length - dataLength, (Int64)CommunProtocol.ChunkSize);
                            int sended = await fileStream.ReadAsync(buffer, 0, (int)sendCount, cancellationToken).ConfigureAwait(false);
                            await networkStream.WriteAsync(buffer, 0, sended, cancellationToken).ConfigureAwait(false);
                            progressService.MinorTick(sended);
                            dataLength += sended;
                        }
                    } finally {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
            }
        }

        public async Task Send(StringCollection fileDropList, CancellationToken cancellationToken) {
            await using(progressService.Begin(ProgressMode.Send)) {
                var flatFiles = FlatFilesList(fileDropList);

                var total = flatFiles.Values.Sum(x => x.Count);
                progressService.SetMaxTick(total);

                await networkStream.WriteAsync((Int64)total, cancellationToken);
                if(await networkStream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessSize) {
                    throw new NotSupportedException($"Others can't receive total files: {total}");
                }

                await networkStream.WriteAsync(ClipboardFile.Format.FileDrop, cancellationToken);
                if(await networkStream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessFormat) {
                    await networkStream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException($"Others do not support clipboard format: {ClipboardFile.Format.FileDrop}");
                }

                foreach(var entry in flatFiles) {
                    foreach(var file in entry.Value) {
                        progressService.Tick(1);
                        await SendFile(entry.Key, file, cancellationToken);

                        if(await networkStream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessData) {
                            await networkStream.WriteAsync(CommunProtocol.Error, cancellationToken);
                            throw new NotSupportedException($"File transfer error");
                        }
                    }
                }

            }
        }

        static Dictionary<string, List<string>> FlatFilesList(StringCollection fileDropList) {
            var files = new Dictionary<string, List<string>>();

            var filesWithAttributes = fileDropList
                .Cast<string>()
                .Distinct()
                .ToDictionary(k => k, v => File.GetAttributes(v));

            foreach(var item in filesWithAttributes.Where(x => !x.Value.HasFlag(FileAttributes.Directory))) {
                if(!files.TryGetValue(string.Empty, out List<string>? file) || file == null) {
                    file = new List<string>();
                    files[string.Empty] = file;
                }
                file.Add(item.Key);
            }

            foreach(var item in filesWithAttributes.Where(x => x.Value.HasFlag(FileAttributes.Directory))) {
                var dirFiles = DirectoryHelper.RecursiveGetFiles(item.Key);
                var emptyFolders = DirectoryHelper.RecursiveGetEmptyFolders(item.Key);

                if(!dirFiles.Any() && !emptyFolders.Any()) {
                    if(!files.TryGetValue(string.Empty, out List<string>? file) || file == null) {
                        file = new List<string>();
                        files[string.Empty] = file;
                    }
                    file.Add(item.Key);
                    continue;
                }

                files[item.Key] = new List<string>();

                if(dirFiles.Any()) {
                    files[item.Key].AddRange(dirFiles);
                }

                if(emptyFolders.Any()) {
                    files[item.Key].AddRange(emptyFolders);
                }
            }
            return files;
        }

    }
}
