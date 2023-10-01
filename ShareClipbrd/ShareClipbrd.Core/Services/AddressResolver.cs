using System.Globalization;
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
            if(portStart == id.Length - 1) {
                id = id[..portStart];
                mandatoryPort = null;
                return;
            }

            try {
                port = int.Parse(id[(portStart + 1)..], NumberStyles.None, CultureInfo.InvariantCulture);
                _ = new IPEndPoint(IPAddress.Any, port);
                id = id[..portStart];
                mandatoryPort = port;
                return;
            } catch(ArgumentOutOfRangeException) {
                throw new ArgumentException($"Port not valid");
            } catch(FormatException) {
                throw new ArgumentException($"Port not valid");
            }
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
            id = s;
            return true;
        }

    }
}
