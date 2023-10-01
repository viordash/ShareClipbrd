using System.Net;

namespace ShareClipbrd.Core.Services {
    public class AddressResolver {
        public const string TagDiscoveryService = "mdns:";

        static void MandatoryPort(ref string id, out int? mandatoryPort) {
            int portStart = id.LastIndexOf(':');
            int port;
            if(portStart < 0) {
                mandatoryPort = null;
                return;
            }

            if(int.TryParse(id[(portStart + 1)..], out port)) {
                try {
                    _ = new IPEndPoint(IPAddress.Any, port);
                    id = id[..portStart];
                    mandatoryPort = port;
                    return;
                } catch(ArgumentOutOfRangeException) {
                }
            }
            throw new ArgumentException("mdns port for the partner address is not needed");
        }

        public static bool UseAddressDiscoveryService(string address, out string id, out int? mandatoryPort) {
            if(!address.StartsWith(TagDiscoveryService)) {
                id = string.Empty;
                mandatoryPort = null;
                return false;
            }

            var s = address.Replace(TagDiscoveryService, string.Empty);

            MandatoryPort(ref s, out mandatoryPort);
            s = s.Trim();
            if(string.IsNullOrEmpty(s)) {
                id = string.Empty;
                mandatoryPort = null;
            }
            id = s;
            return true;
        }

    }
}
