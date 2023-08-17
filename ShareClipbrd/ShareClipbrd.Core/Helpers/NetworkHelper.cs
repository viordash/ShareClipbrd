using System.Net;
using System.Net.Sockets;

namespace ShareClipbrd.Core.Helpers {
    public class NetworkHelper {
        public const string PublicIP = ":0";

        public static IPEndPoint ResolveHostName(string hostname) {
            int portStart = hostname.LastIndexOf(':');
            if(!int.TryParse(hostname[(portStart + 1)..], out int port)) {
                throw new ArgumentException($"Port not valid, hostname: {hostname}");
            }
            var ipString = hostname[..portStart];
            if(ipString == IPAddress.Any.ToString()) {
                return new IPEndPoint(IPAddress.Any, port);
            }
            if(ipString == IPAddress.IPv6Any.ToString()) {
                return new IPEndPoint(IPAddress.IPv6Any, port);
            }

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
