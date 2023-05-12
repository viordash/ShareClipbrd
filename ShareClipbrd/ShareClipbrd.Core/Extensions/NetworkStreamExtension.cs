using System.Net.Sockets;
using System.Text;

namespace ShareClipbrd.Core.Extensions {
    public static class NetworkStreamExtension {
        public static async ValueTask WriteAsync(this NetworkStream stream, UInt16 value, CancellationToken cancellationToken) {
            var bytes = BitConverter.GetBytes(value);
            await stream.WriteAsync(bytes, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        public static async ValueTask<UInt16> ReadUInt16Async(this NetworkStream stream, CancellationToken cancellationToken) {
            var receiveBuffer = new byte[sizeof(UInt16)];
            await stream.ReadExactlyAsync(receiveBuffer, cancellationToken);
            return BitConverter.ToUInt16(receiveBuffer, 0);
        }

        public static async ValueTask WriteAsync(this NetworkStream stream, Int32 value, CancellationToken cancellationToken) {
            var bytes = BitConverter.GetBytes(value);
            await stream.WriteAsync(bytes, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        public static async ValueTask<Int32> ReadInt32Async(this NetworkStream stream, CancellationToken cancellationToken) {
            var receiveBuffer = new byte[sizeof(Int32)];
            await stream.ReadExactlyAsync(receiveBuffer, cancellationToken);
            return BitConverter.ToInt32(receiveBuffer, 0);
        }

        public static async ValueTask WriteAsync(this NetworkStream stream, string value, CancellationToken cancellationToken) {
            await stream.WriteAsync(Encoding.ASCII.GetBytes(value), cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        public static async ValueTask<string> ReadASCIIStringAsync(this NetworkStream stream, CancellationToken cancellationToken) {
            var receiveBuffer = new byte[8192];
            var receivedBytes = await stream.ReadAsync(receiveBuffer, cancellationToken);
            return Encoding.ASCII.GetString(receiveBuffer, 0, receivedBytes);
        }

    }
}
