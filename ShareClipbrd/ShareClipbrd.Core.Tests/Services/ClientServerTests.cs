using System.Net;
using Moq;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Services;

namespace ShareClipbrd.Core.Tests.Services {
    public class Tests {
        Mock<ISystemConfiguration> systemConfigurationMock;
        Mock<IDialogService> dialogServiceMock;
        Mock<IClipboardService> clipboardServiceMock;
        DataServer server;
        DataClient client;

        [SetUp]
        public void Setup() {
            systemConfigurationMock = new();
            dialogServiceMock = new();
            clipboardServiceMock = new();

            systemConfigurationMock.SetupGet(x => x.HostAddress).Returns(IPEndPoint.Parse("127.0.0.1:55542"));

            server = new DataServer(systemConfigurationMock.Object, dialogServiceMock.Object, clipboardServiceMock.Object);
            client = new DataClient(systemConfigurationMock.Object, dialogServiceMock.Object);

            server.Start();
        }

        [TearDown]
        public void Teardown() {
            server.Stop();
        }

        [Test]
        public async Task Send_CommonData_Test() {
            clipboardServiceMock
                .Setup(x => x.SupportedFormat(It.Is<string>(f => f == "UnicodeText")))
                .Returns(true);

            var clipboardData = new ClipboardData();
            clipboardData.Add("UnicodeText", System.Text.Encoding.Unicode.GetBytes("UnicodeText Кирилица"));
            
            await client.Send(clipboardData);

            clipboardServiceMock.Verify();
        }
    }
}