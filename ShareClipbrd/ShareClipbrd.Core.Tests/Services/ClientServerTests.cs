using System.Drawing;
using System.Net;
using System.Text;
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

            systemConfigurationMock.SetupGet(x => x.HostAddress).Returns("127.0.0.1:55542");
            systemConfigurationMock.SetupGet(x => x.PartnerAddress).Returns("127.0.0.1:55542");

            server = new DataServer(systemConfigurationMock.Object, dialogServiceMock.Object, clipboardServiceMock.Object);
            client = new DataClient(systemConfigurationMock.Object);

            server.Start();
        }

        [TearDown]
        public void Teardown() {
            server.Stop();
        }

        [Test]
        public async Task Send_CommonData_Test() {
            ClipboardData? receivedClipboardData = null;

            clipboardServiceMock
                .Setup(x => x.SupportedFormat(It.Is<string>(f => f == "UnicodeText")))
                .Returns(true);

            clipboardServiceMock
                .Setup(x => x.SupportedDataSize(It.Is<Int32>(s => s > 0 && s < 1000)))
                .Returns(true);

            clipboardServiceMock
                .Setup(x => x.SetClipboardData(It.IsAny<ClipboardData>()))
                .Callback<ClipboardData>(c => {
                    receivedClipboardData = c;
                });

            var clipboardData = new ClipboardData();
            clipboardData.Add("UnicodeText", System.Text.Encoding.Unicode.GetBytes("UnicodeText Кирилица"));

            await client.Send(clipboardData);
            await Task.Delay(100);


            Assert.IsNotNull(receivedClipboardData);
            Assert.That(receivedClipboardData.Formats.Keys, Is.EquivalentTo(new[] { "UnicodeText" }));
            Assert.That(receivedClipboardData.Formats.Values, Is.EquivalentTo(new[] { System.Text.Encoding.Unicode.GetBytes("UnicodeText Кирилица") }));

            clipboardServiceMock.Verify();
        }

        [Test]
        public async Task Send_Common_Big_Data_Test() {
            ClipboardData? receivedClipboardData = null;

            clipboardServiceMock
                .Setup(x => x.SupportedFormat(It.Is<string>(f => f == "Text")))
                .Returns(true);

            clipboardServiceMock
                .Setup(x => x.SupportedDataSize(It.Is<Int32>(s => s == 1000_000_003)))
                .Returns(true);

            clipboardServiceMock
                .Setup(x => x.SetClipboardData(It.IsAny<ClipboardData>()))
                .Callback<ClipboardData>(c => {
                    receivedClipboardData = c;
                });

            var clipboardData = new ClipboardData();
            clipboardData.Add("Text", Enumerable.Repeat<byte>(0x20, 1000_000_003).ToArray());

            await client.Send(clipboardData);
            await Task.Delay(1000);


            Assert.IsNotNull(receivedClipboardData);
            Assert.That(receivedClipboardData.Formats.Keys, Is.EquivalentTo(new[] { "Text" }));
            Assert.That(receivedClipboardData.Formats["Text"], Has.Length.EqualTo(1000_000_003));
            Assert.False(receivedClipboardData.Formats["Text"].Take(1000_000).Any(x => x != 0x20));

            clipboardServiceMock.Verify();
        }
    }
}