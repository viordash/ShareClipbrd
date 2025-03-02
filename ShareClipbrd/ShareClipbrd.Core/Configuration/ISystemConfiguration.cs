
namespace ShareClipbrd.Core.Configuration {
    public interface ISystemConfiguration {
        string HostAddress { get; }
        string PartnerAddress { get; }
        int SettingsProfile { get; }
        TimeSpan ClientTimeout { get; }
    }
}
