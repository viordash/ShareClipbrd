using System.IO;
using Moq;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Services;

namespace ShareClipbrd.Core.Tests.Services {
    public class Tests {
        Mock<ISystemConfiguration> systemConfigurationMock;
        Mock<IDialogService> dialogServiceMock;
        Mock<IClipboardSerializer> clipboardSerializerMock;
        DataServer server;
        DataClient client;

        [SetUp]
        public void Setup() {
            systemConfigurationMock = new();
            dialogServiceMock = new();
            clipboardSerializerMock = new();

            systemConfigurationMock.SetupGet(x => x.HostAddress).Returns("127.0.0.1:55542");
            systemConfigurationMock.SetupGet(x => x.PartnerAddress).Returns("127.0.0.1:55542");

            server = new DataServer(systemConfigurationMock.Object, dialogServiceMock.Object, clipboardSerializerMock.Object);
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

            var rnd = new Random();
            var bytes = new byte[1_000_000_003];
            rnd.NextBytes(bytes);

            clipboardData.Add("Text", new MemoryStream(bytes));

            await Task.Delay(100);
            await client.Send(clipboardData);
            await Task.Delay(1000);

            Assert.IsNotNull(receivedClipboardData);
            Assert.That(receivedClipboardData.Formats.Keys, Is.EquivalentTo(new[] { "Text" }));
            Assert.That(receivedClipboardData.Formats["Text"], Has.Length.EqualTo(1_000_000_003));
            Assert.That(((MemoryStream)receivedClipboardData.Formats["Text"]).ToArray().Take(1_000_000), Is.EquivalentTo(bytes.Take(1_000_000)));
        }

        //[Test]
        //public async Task Send_Files_Test() {
        //    ClipboardData? receivedClipboardData = null;

        //    clipboardSerializerMock
        //        .Setup(x => x.FormatForFiles(It.Is<string>(f => f == "FileDrop")))
        //        .Returns(() => true);

        //    server.Start((c) => receivedClipboardData = c);

        //    var clipboardData = new ClipboardData();

        //    var testsPath = Path.Combine(Path.GetTempPath(), "tests");
        //    Directory.CreateDirectory(testsPath);

        //    var rnd = new Random();
        //    var bytes = new byte[33_000_000];
        //    rnd.NextBytes(bytes);
        //    var files = new List<FileStream>();
        //    for(int i = 0; i < 100; i++) {
        //        var filename = Path.Combine(testsPath, Path.GetFileName(Path.GetTempFileName()));
        //        File.WriteAllBytes(filename, bytes.Skip(i * 33_000_000 / 100).Take(33_000_000 / 100).ToArray());
        //        files.Add(new FileStream(filename, FileMode.Open));
        //    }

        //    try {
        //        foreach(var fileStream in files) {
        //            clipboardData.Add("FileDrop", fileStream);
        //        }
        //        await client.Send(clipboardData);
                
        //    } finally {
        //        foreach(var fileStream in files) {
        //            fileStream.Close();
        //            File.Delete(fileStream.Name);
        //        }
        //    }

        //    await Task.Delay(1000);

        //    Assert.IsNotNull(receivedClipboardData);
        //    Assert.That(receivedClipboardData.Formats.Keys, Is.EquivalentTo(new[] { "FileDrop" }));
        //    var otherFilename = System.Text.Encoding.UTF8.GetString(((MemoryStream)receivedClipboardData.Formats["FileDrop"]).ToArray());

        //    Assert.That(otherFilename, Does.Exist);

        //    var otherBytes = File.ReadAllBytes(otherFilename);
        //    File.Delete(otherFilename);

        //    Assert.That(otherBytes, Has.Length.EqualTo(1_000_000_003));
        //    Assert.That(otherBytes.Take(1_000_000), Is.EquivalentTo(bytes.Take(1_000_000)));
        //}

        [Test]
        public async Task Send_Big_File_Test() {
            ClipboardData? receivedClipboardData = null;

            clipboardSerializerMock
                .Setup(x => x.FormatForFiles(It.Is<string>(f => f == "FileDrop")))
                .Returns(() => true);

            server.Start((c) => receivedClipboardData = c);

            var rnd = new Random();
            var bytes = new byte[1_000_000_003];
            rnd.NextBytes(bytes);

            var testsPath = Path.Combine(Path.GetTempPath(), "tests");
            Directory.CreateDirectory(testsPath);
            var filename = Path.Combine(testsPath, Path.GetFileName(Path.GetTempFileName()));
            File.WriteAllBytes(filename, bytes);
            try {
                using(var fileStream = new FileStream(filename, FileMode.Open)) {
                    var clipboardData = new ClipboardData();
                    clipboardData.Add("FileDrop", fileStream);

                    await client.Send(clipboardData);
                }
            } finally {
                File.Delete(filename);
            }
            await Task.Delay(1000);

            Assert.IsNotNull(receivedClipboardData);
            Assert.That(receivedClipboardData.Formats.Keys, Is.EquivalentTo(new[] { "FileDrop" }));
            var otherFilename = System.Text.Encoding.UTF8.GetString(((MemoryStream)receivedClipboardData.Formats["FileDrop"]).ToArray());

            Assert.That(otherFilename, Does.Exist);

            var otherBytes = File.ReadAllBytes(otherFilename);
            File.Delete(otherFilename);

            Assert.That(otherBytes, Has.Length.EqualTo(1_000_000_003));
            Assert.That(otherBytes.Take(1_000_000), Is.EquivalentTo(bytes.Take(1_000_000)));

        }
    }
}