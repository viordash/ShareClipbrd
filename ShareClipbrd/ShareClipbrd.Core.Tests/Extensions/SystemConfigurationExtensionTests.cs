using Moq;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Extensions;

namespace ShareClipbrd.Core.Tests.Extensions {
    public class SystemConfigurationExtensionTests {
        Mock<ISystemConfiguration> systemConfigurationMock;


        [SetUp]
        public void Setup() {
            systemConfigurationMock = new();

        }

        [TearDown]
        public void Teardown() {
        }

        [Test]
        public void HostAddressOrDefault_Returns_HostAddress_If_Configuration_HostAddress_Is_Not_Empty() {
            systemConfigurationMock.SetupGet(x => x.HostAddress).Returns("127.0.0.1:0");
            systemConfigurationMock.SetupGet(x => x.PartnerAddress).Returns("127.0.0.2:0");

            Assert.That(systemConfigurationMock.Object.HostAddressOrDefault(), Is.EqualTo("127.0.0.1:0"));
        }

        [Test]
        public void HostAddressOrDefault_Returns_DefaultId_If_Configuration_Both_Addresses_Is_Empty() {
            systemConfigurationMock.SetupGet(x => x.HostAddress).Returns("");
            systemConfigurationMock.SetupGet(x => x.PartnerAddress).Returns("");

            Assert.That(systemConfigurationMock.Object.HostAddressOrDefault(), Is.EqualTo("mdns:ShareClipbrd_60D54950"));
        }

        [Test]
        public void HostAddressOrDefault_Returns_StringEmpty_If_Configuration_PartnerAddress_Is_Not_Empty() {
            systemConfigurationMock.SetupGet(x => x.HostAddress).Returns("");
            systemConfigurationMock.SetupGet(x => x.PartnerAddress).Returns("127.0.0.2:0");

            Assert.That(systemConfigurationMock.Object.HostAddressOrDefault(), Is.EqualTo(string.Empty));
        }

    }
}
