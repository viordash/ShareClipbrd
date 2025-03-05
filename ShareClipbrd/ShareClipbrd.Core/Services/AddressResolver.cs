using ShareClipbrd.Core.Helpers;

namespace ShareClipbrd.Core.Services {
    public class AddressResolver {
        public const string DefaultId = "ShareClipbrd_60D54950";
        public const string TagDiscoveryService = "mdns:";

        public static bool UseAddressDiscoveryService(string address, out string id, out int port) {
            if(!address.StartsWith(TagDiscoveryService)) {
                id = string.Empty;
                port = 0;
                return false;
            }

            var s = address.Replace(AddressResolver.TagDiscoveryService, string.Empty);

            port = NetworkHelper.ExtractPort(s, out int portStart);
            id = s[..portStart].Trim();
            if(string.IsNullOrEmpty(id)) {
                id = AddressResolver.DefaultId;
            }
            return true;
        }
    }
}
