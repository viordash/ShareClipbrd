using System.Net;
using System.Net.Sockets;

namespace ShareClipbrd.Core.Services {
    public class AddressResolver {
        public const string TagDiscoveryService = "mdns:";

        public static bool UseAddressDiscoveryService(string address, out string id) {
            if(!address.StartsWith(TagDiscoveryService)) {
                id = string.Empty;
                return false;
            }

            var s = address.Replace(TagDiscoveryService, string.Empty);
            if(string.IsNullOrEmpty(s)) {
                id = string.Empty;
                return false;
            }
            id = s;
            return true;
        }
    }
}
