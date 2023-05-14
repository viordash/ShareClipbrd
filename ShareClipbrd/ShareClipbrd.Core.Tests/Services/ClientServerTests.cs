using Moq;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Services;

namespace ShareClipbrd.Core.Tests.Services {
    public class Tests {
        Mock<ISystemConfiguration> systemConfigurationMock;
        Mock<IDialogService> dialogServiceMock;
        DataServer server;
        DataClient client;

        [SetUp]
        public void Setup() {
            systemConfigurationMock = new();
            dialogServiceMock = new();

            systemConfigurationMock.SetupGet(x => x.HostAddress).Returns("127.0.0.1:55542");
            systemConfigurationMock.SetupGet(x => x.PartnerAddress).Returns("127.0.0.1:55542");

            server = new DataServer(systemConfigurationMock.Object, dialogServiceMock.Object);
            client = new DataClient(systemConfigurationMock.Object);

        }

        [TearDown]
        public void Teardown() {
            server.Stop();
        }

        [Test]
        public async Task Send_CommonData_Test() {
            ClipboardData? receivedClipboardData = null;

            server.Start((c) => receivedClipboardData = c);

            var clipboardData = new ClipboardData();
            clipboardData.Add("UnicodeText", new MemoryStream(System.Text.Encoding.Unicode.GetBytes("UnicodeText Кирилица")));

            await Task.Delay(100);
            await client.Send(clipboardData);
            await Task.Delay(100);


            Assert.IsNotNull(receivedClipboardData);
            Assert.That(receivedClipboardData.Formats.Keys, Is.EquivalentTo(new[] { "UnicodeText" }));
            Assert.That(receivedClipboardData.Formats.Values, Is.EquivalentTo(new[] { new MemoryStream(System.Text.Encoding.Unicode.GetBytes("UnicodeText Кирилица")) }));
        }

        [Test]
        public async Task Send_Common_Big_Data_Test() {
            ClipboardData? receivedClipboardData = null;

            server.Start((c) => receivedClipboardData = c);

            var clipboardData = new ClipboardData();
            clipboardData.Add("Text", new MemoryStream(Enumerable.Repeat<byte>(0x20, 1000_000_003).ToArray()));

            await Task.Delay(100);
            await client.Send(clipboardData);
            await Task.Delay(1000);

            Assert.IsNotNull(receivedClipboardData);
            Assert.That(receivedClipboardData.Formats.Keys, Is.EquivalentTo(new[] { "Text" }));
            Assert.That(receivedClipboardData.Formats["Text"], Has.Length.EqualTo(1000_000_003));
            Assert.False(((MemoryStream)receivedClipboardData.Formats["Text"]).ToArray().Take(1000_000).Any(x => x != 0x20));
        }
    }
}