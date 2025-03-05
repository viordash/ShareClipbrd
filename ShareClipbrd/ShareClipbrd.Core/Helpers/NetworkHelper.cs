using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace ShareClipbrd.Core.Helpers {
    public class NetworkHelper {
        public static int ExtractPort(string url, out int startPos) {
            startPos = url.LastIndexOf(':');
            int port;
            if(startPos < 0) {
                startPos = url.Length;
                return 0;
            }

            try {
                port = int.Parse(url[(startPos + 1)..], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                _ = new IPEndPoint(IPAddress.Any, port);
                return port;
            } catch(ArgumentOutOfRangeException) {
                throw new ArgumentException($"Port not valid");
            } catch(FormatException) {
                throw new ArgumentException($"Port not valid");
            }
        }

        public static IPEndPoint ResolveHostName(string hostname) {
            var isIpEndPoint = IPEndPoint.TryParse(hostname, out var ipEndPoint);
            if(isIpEndPoint && ipEndPoint != null) {
                return ipEndPoint;
            }

            var isIpAddress = IPAddress.TryParse(hostname, out var ipAdress);
            var port = ExtractPort(hostname, out int portStart);
            if(isIpAddress && ipAdress != null) {
                return new IPEndPoint(ipAdress, port); ;
            }

            var ipString = hostname[..portStart].Trim();   
            var addresses = Dns.GetHostAddresses(ipString);
            var adr = addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork
                    && !IPAddress.IsLoopback(x));
            if(adr != null) {
                return new IPEndPoint(adr, port);
            }

            adr = addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            if(adr != null) {
                return new IPEndPoint(adr, port);
            }
            adr = addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetworkV6);
            if(adr != null) {
                return new IPEndPoint(adr, port);
            }
            throw new ArgumentException($"Host name ({hostname}) resolve error");
        }
    }
}
