using System.Net;

namespace ShareClipbrd.Core.Configuration {
    public interface ISystemConfiguration {
        IPEndPoint HostAddress { get; }
    }
}
