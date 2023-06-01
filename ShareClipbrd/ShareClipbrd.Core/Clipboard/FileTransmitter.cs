using System.Collections.Specialized;
using ShareClipbrd.Core.Extensions;
using ShareClipbrd.Core.Helpers;
using ShareClipbrd.Core.Services;

namespace ShareClipbrd.Core.Clipboard {
    public class FileTransmitter {

        readonly IProgressService progressService;
        readonly Stream stream;

        public FileTransmitter(
            IProgressService progressService,
            Stream stream) {
            this.progressService = progressService;
            this.stream = stream;
        }

        async Task SendFile(string name, CancellationToken cancellationToken) {
            var attributes = File.GetAttributes(name);
            await stream.WriteAsync((int)attributes, cancellationToken);

            await stream.WriteAsync(name.Length, cancellationToken);

            await stream.WriteAsync(name, cancellationToken);


            if(attributes.HasFlag(FileAttributes.Directory)) {
                await stream.WriteAsync((Int64)0, cancellationToken);
            } else {
                using(var fileStream = new FileStream(name, FileMode.Open)) {
                    await stream.WriteAsync((Int64)fileStream.Length, cancellationToken);
                    await fileStream.CopyToAsync(stream, cancellationToken);
                }
            }
        }

        public async Task Send(StringCollection fileDropList, CancellationToken cancellationToken) {
            await using(progressService.Begin(ProgressMode.Send)) {
                var flatFiles = FlatFilesList(fileDropList.Cast<string>().Distinct());

                var total = flatFiles.Values.Sum(x => x.Count);
                progressService.SetMaxTick(total);

                await stream.WriteAsync((Int64)total, cancellationToken);
                if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessSize) {
                    await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException($"Others can't receive total files: {total}");
                }

                foreach(var entry in flatFiles) {
                    foreach(var file in entry.Value) {
                        await SendFile(file, cancellationToken);
                    }
                }
                if(await stream.ReadUInt16Async(cancellationToken) != CommunProtocol.SuccessData) {
                    await stream.WriteAsync(CommunProtocol.Error, cancellationToken);
                    throw new NotSupportedException($"File transfer error");
                }
            }
        }

        Dictionary<string, List<string>> FlatFilesList(IEnumerable<string> fileDropList) {
            var files = new Dictionary<string, List<string>>();

            var filesWithAttributes = fileDropList.ToDictionary(k => k, v => File.GetAttributes(v));

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

                if(dirFiles.Any()) {
                    files[item.Key] = dirFiles;
                }

                if(emptyFolders.Any()) {
                    files[item.Key] = emptyFolders;
                }
            }
            return files;
        }

    }
}
