using ShareClipbrd.Core.Services;

namespace ShareClipbrd.Core.Tests.Services {
    public class AddressResolverTests {
        [Test]
        public void UseAddressDiscoveryService_When_Invalid_Address_Return_False() {
            string id;
            int? mandatoryPort;
           Assert.That(AddressResolver.UseAddressDiscoveryService(":55542", out id, out mandatoryPort), Is.False);
            Assert.That(id, Is.Empty);
            Assert.That(mandatoryPort, Is.Null);

           Assert.That(AddressResolver.UseAddressDiscoveryService(string.Empty, out id, out mandatoryPort), Is.False);
            Assert.That(id, Is.Empty);
            Assert.That(mandatoryPort, Is.Null);

           Assert.That(AddressResolver.UseAddressDiscoveryService("mdns 1234", out id, out mandatoryPort), Is.False);
            Assert.That(id, Is.Empty);
            Assert.That(mandatoryPort, Is.Null);

           Assert.That(AddressResolver.UseAddressDiscoveryService("mdns", out id, out mandatoryPort), Is.False);
            Assert.That(id, Is.Empty);
            Assert.That(mandatoryPort, Is.Null);

           Assert.That(AddressResolver.UseAddressDiscoveryService("mdns :", out id, out mandatoryPort), Is.False);
            Assert.That(id, Is.Empty);
            Assert.That(mandatoryPort, Is.Null);

           Assert.That(AddressResolver.UseAddressDiscoveryService("mdns :   ", out id, out mandatoryPort), Is.False);
            Assert.That(id, Is.Empty);
            Assert.That(mandatoryPort, Is.Null);
        }

        [Test]
        public void UseAddressDiscoveryService_Valid_Address_Return_True_And_Id() {
            string id;
            int? mandatoryPort;
            Assert.That(AddressResolver.UseAddressDiscoveryService("mdns:a", out id, out mandatoryPort), Is.True);
            Assert.That(id, Is.EqualTo("a"));
            Assert.That(mandatoryPort, Is.Null);

            Assert.That(AddressResolver.UseAddressDiscoveryService("mdns:55542", out id, out mandatoryPort), Is.True);
            Assert.That(id, Is.EqualTo("55542"));
            Assert.That(mandatoryPort, Is.Null);

            Assert.That(AddressResolver.UseAddressDiscoveryService("mdns:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef", out id, out mandatoryPort), Is.True);
            Assert.That(id, Is.EqualTo("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"));
            Assert.That(mandatoryPort, Is.Null);
        }

        [Test]
        public void UseAddressDiscoveryService_When_Invalid_Ports_Throws_ArgumentException() {
            string id;
            int? mandatoryPort;
            Assert.Throws<ArgumentException>(() => AddressResolver.UseAddressDiscoveryService("mdns:abcde: 1", out id, out mandatoryPort));
            Assert.Throws<ArgumentException>(() => AddressResolver.UseAddressDiscoveryService("mdns:abcde: 65535", out id, out mandatoryPort));

            Assert.Throws<ArgumentException>(() => AddressResolver.UseAddressDiscoveryService("mdns:abcde:-1", out id, out mandatoryPort));
            Assert.Throws<ArgumentException>(() => AddressResolver.UseAddressDiscoveryService("mdns:abcde:65536", out id, out mandatoryPort));

            Assert.That(AddressResolver.UseAddressDiscoveryService("mdns::", out id, out mandatoryPort));
            Assert.That(id, Is.Empty);
            Assert.That(mandatoryPort, Is.Null);
        }

        [Test]
        public void UseAddressDiscoveryService_Returns_True_And_Empty_Id_If_Address_Is_Not_Fully_Filled() {
            string id;
            int? mandatoryPort;

            Assert.That(AddressResolver.UseAddressDiscoveryService("mdns:abcde:", out id, out mandatoryPort), Is.True);
            Assert.That(id, Is.EqualTo("abcde"));
            Assert.That(mandatoryPort, Is.Null);

            Assert.That(AddressResolver.UseAddressDiscoveryService("mdns:", out id, out mandatoryPort), Is.True);
            Assert.That(id, Is.Empty);
            Assert.That(mandatoryPort, Is.Null);

            Assert.That(AddressResolver.UseAddressDiscoveryService("mdns::1", out id, out mandatoryPort), Is.True);
            Assert.That(id, Is.Empty);
            Assert.That(mandatoryPort, Is.EqualTo(1));
        }

        [Test]
        public void UseAddressDiscoveryService_Valid_Ports_And_Entire_Id_Extracting_Tests() {
            string id;
            int? mandatoryPort;
            Assert.That(AddressResolver.UseAddressDiscoveryService("mdns:abcde:0", out id, out mandatoryPort), Is.True);
            Assert.That(id, Is.EqualTo("abcde"));
            Assert.That(mandatoryPort, Is.EqualTo(0));
            Assert.That(AddressResolver.UseAddressDiscoveryService("mdns:abcde 12345:1", out id, out mandatoryPort), Is.True);
            Assert.That(id, Is.EqualTo("abcde 12345"));
            Assert.That(mandatoryPort, Is.EqualTo(1));
            Assert.That(AddressResolver.UseAddressDiscoveryService("mdns:abcde:65535", out id, out mandatoryPort), Is.True);
            Assert.That(id, Is.EqualTo("abcde"));
            Assert.That(mandatoryPort, Is.EqualTo(65535));
        }
    }
}
