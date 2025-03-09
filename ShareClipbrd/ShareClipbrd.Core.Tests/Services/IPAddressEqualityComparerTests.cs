using ShareClipbrd.Core.Services;
using System.Net;

namespace ShareClipbrd.Core.Tests.Services {
    public class IPAddressEqualityComparerTests {
        [Test]
        public void Equals_IPV4_to_IPV4() {
            var testable = new IPAddressEqualityComparer();
            Assert.That(testable.Equals(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.1")), Is.True);
            Assert.That(testable.Equals(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.2")), Is.False);
        }

        [Test]
        public void Equals_IPV4_to_IPv4MappedToIPv6() {
            var testable = new IPAddressEqualityComparer();
            Assert.That(testable.Equals(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.1").MapToIPv6()), Is.True);
            Assert.That(testable.Equals(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.2").MapToIPv6()), Is.False);
        }

        [Test]
        public void GetHashCode_IPV4() {
            var testable = new IPAddressEqualityComparer();
            Assert.That(testable.GetHashCode(IPAddress.Parse("127.0.0.1")), Is.EqualTo(IPAddress.Parse("127.0.0.1").GetHashCode()));
        }

        [Test]
        public void GetHashCode_IPv4MappedToIPv6() {
            var testable = new IPAddressEqualityComparer();
            Assert.That(testable.GetHashCode(IPAddress.Parse("127.0.0.1").MapToIPv6()), Is.EqualTo(IPAddress.Parse("127.0.0.1").GetHashCode()));
        }
    }
}
