using ShareClipbrd.Core.Services;

namespace ShareClipbrd.Core.Tests.Services {
    public class AddressResolverTests {
        [Test]
        public void UseAddressDiscoveryService_When_Invalid_Address_Return_False() {
            string id;
            Assert.IsFalse(AddressResolver.UseAddressDiscoveryService(":55542", out id));
            Assert.That(id, Is.Empty);

            Assert.IsFalse(AddressResolver.UseAddressDiscoveryService(string.Empty, out id));
            Assert.That(id, Is.Empty);

            Assert.IsFalse(AddressResolver.UseAddressDiscoveryService("mdns 1234", out id));
            Assert.That(id, Is.Empty);

            Assert.IsFalse(AddressResolver.UseAddressDiscoveryService("mdns", out id));
            Assert.That(id, Is.Empty);

            Assert.IsFalse(AddressResolver.UseAddressDiscoveryService("mdns:", out id));
            Assert.That(id, Is.Empty);
        }

        [Test]
        public void UseAddressDiscoveryService_Valid_Address_Return_True_And_Id() {
            string id;
            Assert.IsTrue(AddressResolver.UseAddressDiscoveryService("mdns:a", out id));
            Assert.That(id, Is.EqualTo("a"));

            Assert.IsTrue(AddressResolver.UseAddressDiscoveryService("mdns:55542", out id));
            Assert.That(id, Is.EqualTo("55542"));

            Assert.IsTrue(AddressResolver.UseAddressDiscoveryService("mdns:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef", out id));
            Assert.That(id, Is.EqualTo("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"));
        }

    }
}
