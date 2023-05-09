using System.Net;
using ShareClipbrdApp.Win.Configuration;
using ShareClipbrdApp.Win.Properties;

namespace ShareClipbrdApp.Win.Tests.Services {
    public class SystemConfigurationTests {

        SystemConfiguration testable;

        [SetUp]
        public void Setup() {
            testable = new();
        }

        [Test]
        public void HostAddress_Test() {
            Settings.Default.HostAddress = "127.1.2.3:8081";
            Assert.That(testable.HostAddress.Address, Is.EqualTo(IPAddress.Parse("127.1.2.3")));
            Assert.That(testable.HostAddress.Port, Is.EqualTo(8081));
        }

        [Test]
        public void HostAddress_Default_Port_Test() {
            Settings.Default.HostAddress = "127.1.2.3";
            Assert.That(testable.HostAddress.Address, Is.EqualTo(IPAddress.Parse("127.1.2.3")));
            Assert.That(testable.HostAddress.Port, Is.EqualTo(0));
        }

        [Test]
        public void HostAddress_Incorrect_Throws() {
            Settings.Default.HostAddress = "127.1.2.";
            Assert.Throws<FormatException>(() => Assert.NotNull(testable.HostAddress));
            Settings.Default.HostAddress = "error8081";
            Assert.Throws<FormatException>(() => Assert.NotNull(testable.HostAddress));
        }


    }
}