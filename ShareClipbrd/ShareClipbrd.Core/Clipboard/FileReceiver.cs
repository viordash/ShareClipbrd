using System.Buffers;
using System.Collections.Specialized;
using System.IO;
using System.Net.Sockets;
using System.Text;
using ShareClipbrd.Core.Extensions;
using ShareClipbrd.Core.Services;

namespace ShareClipbrd.Core.Clipboard {
    public class FileReceiver {
        readonly IProgressService progressService;
        readonly NetworkStream networkStream;
        readonly string sessionDir;
        readonly Int64 total;
        readonly StringCollection fileDropList;
        readonly CancellationToken cancellationToken;

        public FileReceiver(
            IProgressService progressService,
            NetworkStream networkStream,
            string sessionDir,
            Int64 total,
            StringCollection fileDropList,
            CancellationToken cancellationToken) {
            this.progressService = progressService;
            this.networkStream = networkStream;
            this.sessionDir = sessionDir;
            this.total = total;
            this.fileDropList = fileDropList;
            this.cancellationToken = cancellationToken;
        }

        async Task ReceiveFile() {
            var attributes = (FileAttributes)await networkStream.ReadInt32Async(cancellationToken);
            ValidateAttributes(attributes);

            var nameLength = await networkStream.ReadInt32Async(cancellationToken);
            ValidateNameLength(nameLength);

            var nameBuffer = ArrayPool<byte>.Shared.Rent(nameLength);
            try {
                var receivedBytes = await networkStream.ReadAsync(nameBuffer, 0, nameLength, cancellationToken);
                var name = Encoding.UTF8.GetString(nameBuffer, 0, receivedBytes);
                ValidateName(name);


                if(attributes.HasFlag(FileAttributes.Directory)) {
                    var dataLength = await networkStream.ReadInt64Async(cancellationToken);
                    ValidateDirectoryDataLength(dataLength);

                    var tempDirectory = Path.Combine(sessionDir, name);
                    Directory.CreateDirectory(tempDirectory);
                    fileDropList.Add(tempDirectory);
                } else {
                    var dataLength = await networkStream.ReadInt64Async(cancellationToken);
                    ValidateFileDataLength(dataLength);

                    var tempFilename = Path.Combine(sessionDir, name);
                    var directory = Path.GetDirectoryName(tempFilename);
                    if(!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) {
                        Directory.CreateDirectory(directory);
                    }

                    byte[] buffer = ArrayPool<byte>.Shared.Rent(CommunProtocol.ChunkSize);
                    try {
                        using(var fileStream = new FileStream(tempFilename, FileMode.Create)) {
                            progressService.SetMaxMinorTick(dataLength);
                            while(fileStream.Length < dataLength) {
                                var readCount = Math.Min(dataLength - fileStream.Length, (Int64)CommunProtocol.ChunkSize);
                                int readed = await networkStream.ReadAsync(buffer, 0, (int)readCount, cancellationToken).ConfigureAwait(false);
                                await fileStream.WriteAsync(buffer, 0, readed, cancellationToken).ConfigureAwait(false);
                                progressService.MinorTick(readed);
                            }
                            ValidateFile(fileStream, dataLength);
                        }
                    } finally {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                    fileDropList.Add(tempFilename);
                }
            } finally {
                ArrayPool<byte>.Shared.Return(nameBuffer);
            }
        }

        public async Task Receive() {
            ValidateTotal(total);
            progressService.SetMaxTick(total);

            for(int i = 0; i < total; i++) {
                progressService.Tick(1);
                await ReceiveFile();
                await networkStream.WriteAsync(CommunProtocol.SuccessData, cancellationToken);
            }

            if(networkStream.DataAvailable) {
                throw new InvalidOperationException("Receive buffer is not empty");
            }
        }


        static void ValidateTotal(Int64 total) {
            if(total < 0 || total > 1_000_000_000) {
                throw new InvalidDataException(nameof(total));
            }
        }

        static void ValidateAttributes(FileAttributes attributes) {
            var mask = FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory
                | FileAttributes.Archive | FileAttributes.Device | FileAttributes.Normal | FileAttributes.Temporary
                | FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed | FileAttributes.Offline
                | FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.IntegrityStream | FileAttributes.NoScrubData;


            if((attributes & ~mask) != 0) {
                throw new InvalidDataException(nameof(attributes));
            }
        }

        static void ValidateNameLength(Int32 nameLength) {
            if(nameLength < 0 || nameLength > 65536) {
                throw new InvalidDataException(nameof(nameLength));
            }
        }

        static void ValidateName(string name) {
            if(string.IsNullOrEmpty(name) || name.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
                throw new InvalidDataException(nameof(name));
            }

        }

        static void ValidateDirectoryDataLength(Int64 directoryDataLength) {
            if(directoryDataLength != 0) {
                throw new InvalidDataException(nameof(directoryDataLength));
            }
        }

        static void ValidateFileDataLength(Int64 fileDataLength) {
            if(fileDataLength < 0 || fileDataLength > 34_359_738_368) {
                throw new InvalidDataException(nameof(fileDataLength));
            }
        }

        static void ValidateFile(FileStream fileStream, Int64 expectedSize) {
            if(fileStream.Length != expectedSize) {
                throw new InvalidDataException(nameof(fileStream));
            }
        }
    }
}
