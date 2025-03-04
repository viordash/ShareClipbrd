using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Services;

namespace ShareClipbrd.Core.Extensions {
    public static class SystemConfigurationExtension {
        public static string HostAddressOrDefault(this ISystemConfiguration systemConfiguration) {
            if(!string.IsNullOrEmpty(systemConfiguration.HostAddress)) {
                return systemConfiguration.HostAddress;
            }
            if(string.IsNullOrEmpty(systemConfiguration.PartnerAddress)) {
                return AddressResolver.TagDiscoveryService + DataServer.DefaultId;
            }
            return string.Empty;
        }

    }
}
