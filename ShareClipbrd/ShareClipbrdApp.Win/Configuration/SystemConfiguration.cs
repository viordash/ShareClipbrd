using System.Net;
using ShareClipbrd.Core.Configuration;
using ShareClipbrdApp.Win.Properties;

namespace ShareClipbrdApp.Win.Configuration {

    public class SystemConfiguration : ISystemConfiguration {
        public IPEndPoint HostAddress {
            get {
                return IPEndPoint.Parse(Settings.Default.HostAddress);
            }
        }
    }
}
