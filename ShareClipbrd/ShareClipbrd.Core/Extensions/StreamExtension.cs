﻿using System.Text;

namespace ShareClipbrd.Core.Extensions {
    public static class StreamExtension {
        public static async ValueTask WriteAsync(this Stream stream, UInt16 value, CancellationToken cancellationToken) {
            var bytes = BitConverter.GetBytes(value);
            await stream.WriteAsync(bytes, cancellationToken);
        }
        public static async ValueTask<UInt16> ReadUInt16Async(this Stream stream, CancellationToken cancellationToken) {
            var receiveBuffer = new byte[sizeof(UInt16)];
            await stream.ReadExactlyAsync(receiveBuffer, cancellationToken);
            return BitConverter.ToUInt16(receiveBuffer, 0);
        }

        public static async ValueTask WriteAsync(this Stream stream, Int32 value, CancellationToken cancellationToken) {
            var bytes = BitConverter.GetBytes(value);
            await stream.WriteAsync(bytes, cancellationToken);
        }
        public static async ValueTask<Int32> ReadInt32Async(this Stream stream, CancellationToken cancellationToken) {
            var receiveBuffer = new byte[sizeof(Int32)];
            await stream.ReadExactlyAsync(receiveBuffer, cancellationToken);
            return BitConverter.ToInt32(receiveBuffer, 0);
        }

        public static async ValueTask WriteAsync(this Stream stream, Int64 value, CancellationToken cancellationToken) {
            var bytes = BitConverter.GetBytes(value);
            await stream.WriteAsync(bytes, cancellationToken);
        }
        public static async ValueTask<Int64> ReadInt64Async(this Stream stream, CancellationToken cancellationToken) {
            var receiveBuffer = new byte[sizeof(Int64)];
            await stream.ReadExactlyAsync(receiveBuffer, cancellationToken);
            return BitConverter.ToInt64(receiveBuffer, 0);
        }

        public static async ValueTask WriteAsync(this Stream stream, string value, CancellationToken cancellationToken) {
            await stream.WriteAsync(Encoding.UTF8.GetBytes(value), cancellationToken);
        }
        public static async ValueTask<string> ReadUTF8StringAsync(this Stream stream, CancellationToken cancellationToken) {
            var receiveBuffer = new byte[65536];
            var receivedBytes = await stream.ReadNotEmptyAsync(receiveBuffer, cancellationToken);
            return Encoding.UTF8.GetString(receiveBuffer, 0, receivedBytes);
        }

        public static async Task<int> ReadNotEmptyAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            var receivedBytes = await stream.ReadAsync(buffer, offset, count, cancellationToken);
            if(receivedBytes == 0) {
                throw new OperationCanceledException("empty read");
            }
            return receivedBytes;
        }

        public static async ValueTask<int> ReadNotEmptyAsync(this Stream stream, Memory<byte> buffer, CancellationToken cancellationToken) {
            var receivedBytes = await stream.ReadAsync(buffer, cancellationToken);
            if(receivedBytes == 0) {
                throw new OperationCanceledException("empty read");
            }
            return receivedBytes;
        }
    }
}
