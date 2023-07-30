using System;
using ShareClipbrd.Core.Configuration;
using ShareClipbrdApp.Properties;

namespace ShareClipbrdApp.Configuration {
    public class SystemConfiguration : ISystemConfiguration {
        public string HostAddress {
            get {
                switch(SettingsProfile) {
                    case 1:
                        return Settings.Default.HostAddress1;
                    case 2:
                        return Settings.Default.HostAddress2;
                    default:
                        return Settings.Default.HostAddress0;
                }
            }
        }
        public string PartnerAddress {
            get {
                switch(SettingsProfile) {
                    case 1:
                        return Settings.Default.PartnerAddress1;
                    case 2:
                        return Settings.Default.PartnerAddress2;
                    default:
                        return Settings.Default.PartnerAddress0;
                }
            }
        }
        public int SettingsProfile {
            get {
                return Math.Clamp(Settings.Default.SettingsProfile, 0, 2);
            }
        }
    }
}
