using System.Net;

namespace ShareClipbrd.Core.Services {
    class IPAddressEqualityComparer : IEqualityComparer<IPAddress> {
        public bool Equals(IPAddress? iPAddress1, IPAddress? iPAddress2) {

            if(iPAddress1 is null || iPAddress2 is null) {
                return false;
            }

            if(iPAddress1.IsIPv4MappedToIPv6) {
                return iPAddress1.MapToIPv4().Equals(iPAddress2);
            }

            if(iPAddress2.IsIPv4MappedToIPv6) {
                return iPAddress2.MapToIPv4().Equals(iPAddress1);
            }

            return iPAddress1.Equals(iPAddress2);
        }

        public int GetHashCode(IPAddress iPAddress) {
            if(iPAddress.IsIPv4MappedToIPv6) {
                return iPAddress.MapToIPv4().GetHashCode();
            }
            return iPAddress.GetHashCode();
        }
    }
}
