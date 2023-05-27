
namespace ShareClipbrd.Core.Configuration {
    public interface ISystemConfiguration {
        string HostAddress { get; }
        string PartnerAddress { get; }
        CompressionLevel Compression { get; }
    }
}
