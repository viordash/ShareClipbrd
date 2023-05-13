using System.Net;

namespace ShareClipbrd.Core.Configuration {
    public interface ISystemConfiguration {
        string HostAddress { get; }
        string PartnerAddress { get; }
    }
}
