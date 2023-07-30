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
            Settings.Default.HostAddress0 = "127.1.2.3:8081";
            Settings.Default.HostAddress1 = "127.1.2.3:8181";
            Settings.Default.HostAddress2 = "127.1.2.3:8281";
            Settings.Default.SettingsProfile = 0;
            Assert.That(testable.HostAddress, Is.EqualTo("127.1.2.3:8081"));
            Settings.Default.SettingsProfile = 1;
            Assert.That(testable.HostAddress, Is.EqualTo("127.1.2.3:8181"));
            Settings.Default.SettingsProfile = 2;
            Assert.That(testable.HostAddress, Is.EqualTo("127.1.2.3:8281"));
        }

        [Test]
        public void PartnerAddress_Test() {
            Settings.Default.PartnerAddress0 = "127.1.2.3:8082";
            Settings.Default.PartnerAddress1 = "127.1.2.3:8182";
            Settings.Default.PartnerAddress2 = "127.1.2.3:8282";
            Settings.Default.SettingsProfile = 0;
            Assert.That(testable.PartnerAddress, Is.EqualTo("127.1.2.3:8082"));
            Settings.Default.SettingsProfile = 1;
            Assert.That(testable.PartnerAddress, Is.EqualTo("127.1.2.3:8182"));
            Settings.Default.SettingsProfile = 2;
            Assert.That(testable.PartnerAddress, Is.EqualTo("127.1.2.3:8282"));
        }

        [Test]
        public void SettingsProfile_Clamped_Range_Test() {
            Settings.Default.SettingsProfile = -1;
            Assert.That(testable.SettingsProfile, Is.EqualTo(0));
            Settings.Default.SettingsProfile = 0;
            Assert.That(testable.SettingsProfile, Is.EqualTo(0));
            Settings.Default.SettingsProfile = 1;
            Assert.That(testable.SettingsProfile, Is.EqualTo(1));
            Settings.Default.SettingsProfile = 2;
            Assert.That(testable.SettingsProfile, Is.EqualTo(2));
            Settings.Default.SettingsProfile = 3;
            Assert.That(testable.SettingsProfile, Is.EqualTo(2));
        }
    }
}
