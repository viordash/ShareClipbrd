using ShareClipbrd.Core.Configuration;
using ShareClipbrdApp.Properties;

namespace ShareClipbrdApp.Configuration {
    public class SystemConfiguration : ISystemConfiguration {
        public string HostAddress {
            get {
                return Settings.Default.HostAddress;
            }
        }
        public string PartnerAddress {
            get {
                return Settings.Default.PartnerAddress;
            }
        }
    }
}
