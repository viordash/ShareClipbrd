using ShareClipbrdApp.Configuration;
using ShareClipbrdApp.Properties;

namespace ShareClipbrdApp.Tests.Configuration {
    public class SystemConfigurationTests {

        SystemConfiguration testable;

        [SetUp]
        public void Setup() {
            testable = new();
        }

        [Test]
        public void HostAddress_Test() {
            Settings.Default.HostAddress = "127.1.2.3:8081";
            Assert.That(testable.HostAddress, Is.EqualTo("127.1.2.3:8081"));
        }

        [Test]
        public void PartnerAddress_Test() {
            Settings.Default.PartnerAddress = "127.1.2.3:8082";
            Assert.That(testable.PartnerAddress, Is.EqualTo("127.1.2.3:8082"));
        }
    }
}
