using System;
using System.Buffers;
using System.IO;
using System.IO.Pipes;
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
            Assert.That(receivedClipboardData.Formats.Select(x => x.Format), Is.EquivalentTo(new[] { "UnicodeText" }));
            Assert.That(receivedClipboardData.Formats.Select(x => x.Data), Is.EquivalentTo(new[] { new MemoryStream(System.Text.Encoding.Unicode.GetBytes("UnicodeText Кирилица")) }));
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
            Assert.That(receivedClipboardData.Formats.Select(x => x.Format), Is.EquivalentTo(new[] { "Text" }));
            Assert.That(receivedClipboardData.Formats.First(x => x.Format == "Text").Data, Has.Length.EqualTo(1_000_000_003));
            Assert.That(((MemoryStream)receivedClipboardData.Formats.First(x => x.Format == "Text").Data).ToArray().Take(1_000_000), Is.EquivalentTo(bytes.Take(1_000_000)));
        }

        [Test]
        public async Task Send_Files_Test() {
            ClipboardData? receivedClipboardData = null;

            clipboardSerializerMock
                .Setup(x => x.FormatForFiles(It.Is<string>(f => f == "FileDrop")))
                .Returns(() => true);

            server.Start((c) => receivedClipboardData = c);

            var clipboardData = new ClipboardData();

            var testsPath = Path.Combine(Path.GetTempPath(), "tests");
            Directory.CreateDirectory(testsPath);

            var rnd = new Random();
            var bytes = new byte[3_333_333];
            rnd.NextBytes(bytes);
            var testdata = new MemoryStream(bytes);
            var files = new List<FileStream>();

            var buffer = new byte[3_333_333 / 100];
            testdata.Position = 0;
            for(int i = 0; i < 100; i++) {
                var filename = Path.Combine(testsPath, Path.GetFileName(Path.GetTempFileName()));
                testdata.Read(buffer, 0, buffer.Length);

                File.WriteAllBytes(filename, buffer);
                files.Add(new FileStream(filename, FileMode.Open));
            }

            try {
                foreach(var fileStream in files) {
                    clipboardData.Add("FileDrop", fileStream);
                }
                await client.Send(clipboardData);

            } finally {
                foreach(var fileStream in files) {
                    fileStream.Close();
                    File.Delete(fileStream.Name);
                }
            }
            await Task.Delay(1000);
            Assert.IsNotNull(receivedClipboardData);
            Assert.That(receivedClipboardData.Formats, Has.Count.EqualTo(100));


            testdata.Position = 0;
            for(int i = 0; i < 100; i++) {
                Assert.That(receivedClipboardData.Formats[i].Format, Is.EqualTo("FileDrop"));
                var otherFilename = System.Text.Encoding.UTF8.GetString(((MemoryStream)receivedClipboardData.Formats[i].Data).ToArray());

                Assert.That(otherFilename, Does.Exist);

                var otherBytes = File.ReadAllBytes(otherFilename);
                File.Delete(otherFilename);

                Assert.That(otherBytes, Has.Length.EqualTo(3_333_333 / 100));
                testdata.Read(buffer, 0, buffer.Length);
                Assert.That(otherBytes, Is.EquivalentTo(buffer));
            }
        }

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
            Assert.That(receivedClipboardData.Formats.Select(x => x.Format), Is.EquivalentTo(new[] { "FileDrop" }));
            var otherFilename = System.Text.Encoding.UTF8.GetString(((MemoryStream)receivedClipboardData.Formats.First(x => x.Format == "FileDrop").Data).ToArray());

            Assert.That(otherFilename, Does.Exist);

            var otherBytes = File.ReadAllBytes(otherFilename);
            File.Delete(otherFilename);

            Assert.That(otherBytes, Has.Length.EqualTo(1_000_000_003));
            Assert.That(otherBytes.Take(1_000_000), Is.EquivalentTo(bytes.Take(1_000_000)));

        }

        [Test]
        public async Task Send_Files_And_Folders_Test() {
            ClipboardData? receivedClipboardData = null;

            clipboardSerializerMock
                .Setup(x => x.FormatForFiles(It.Is<string>(f => f == ClipboardData.Format.FileDrop || f == ClipboardData.Format.DirectoryDrop)))
                .Returns(() => true);

            server.Start((c) => receivedClipboardData = c);

            var clipboardData = new ClipboardData();

            var testsPath = Path.Combine(Path.GetTempPath(), "tests");
            Directory.CreateDirectory(testsPath);

            var rnd = new Random();
            var bytes0 = new byte[555_000];
            rnd.NextBytes(bytes0);
            var bytes1 = new byte[777_000];
            rnd.NextBytes(bytes1);

            var filename0 = Path.Combine(testsPath, "filename0");
            File.WriteAllBytes(filename0, bytes0);
            clipboardData.Add(ClipboardData.Format.FileDrop, new FileStream(filename0, FileMode.Open, FileAccess.Read, FileShare.Read));

            var directory0 = Path.Combine(testsPath, "directory0");
            Directory.CreateDirectory(directory0);
            clipboardData.Add(ClipboardData.Format.DirectoryDrop, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(directory0)));

            var filename1 = Path.Combine(directory0, "filename1");
            File.WriteAllBytes(filename1, bytes1);
            clipboardData.Add(ClipboardData.Format.FileDrop, new FileStream(filename1, FileMode.Open, FileAccess.Read, FileShare.Read));

            var directory0_Child0 = Path.Combine(directory0, "directory0_Child0");
            Directory.CreateDirectory(directory0_Child0);
            //clipboardData.Add(ClipboardData.Format.DirectoryDrop, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(directory0_Child0)));

            //var directory0_Child1 = Path.Combine(directory0, "directory0_Child1");
            //Directory.CreateDirectory(directory0_Child1);
            ////clipboardData.Add(ClipboardData.Format.DirectoryDrop, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(directory0_Child1)));

            //var directory0_Child1_Child0 = Path.Combine(directory0_Child1, "directory0_Child1_Child0");
            //Directory.CreateDirectory(directory0_Child1_Child0);
            ////clipboardData.Add(ClipboardData.Format.DirectoryDrop, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(directory0_Child1_Child0)));

            //var filename1 = Path.Combine(directory0_Child1_Child0, "filename1");
            //File.WriteAllBytes(filename1, bytes1);
            //clipboardData.Add(ClipboardData.Format.FileDrop, new FileStream(filename1, FileMode.Open, FileAccess.Read, FileShare.Read));

            //var directory1 = Path.Combine(testsPath, "directory1");
            //Directory.CreateDirectory(directory1);
            //clipboardData.Add(ClipboardData.Format.DirectoryDrop, new MemoryStream(System.Text.Encoding.UTF8.GetBytes(directory1)));

            try {
                await client.Send(clipboardData);
            } finally {
                foreach(var fileStream in clipboardData.Formats
                    .Where(x => x.Format == ClipboardData.Format.FileDrop)
                    .Select(x=>x.Data)
                    .Cast<FileStream>()) {
                    fileStream.Close();
                }
                Directory.Delete(testsPath, true);

            }
            await Task.Delay(1000);
            Assert.IsNotNull(receivedClipboardData);
            Assert.That(receivedClipboardData.Formats, Has.Count.EqualTo(7));

            Assert.That(receivedClipboardData.Formats[0].Format, Is.EqualTo(ClipboardData.Format.FileDrop));
            var otherFilename = System.Text.Encoding.UTF8.GetString(((MemoryStream)receivedClipboardData.Formats[0].Data).ToArray());
            Assert.That(otherFilename, Does.Exist);
            var otherBytes = File.ReadAllBytes(otherFilename);
            File.Delete(otherFilename);
            Assert.That(otherBytes, Is.EquivalentTo(bytes0));

            Assert.That(receivedClipboardData.Formats[1].Format, Is.EqualTo(ClipboardData.Format.DirectoryDrop));
            var otherDirectory0 = System.Text.Encoding.UTF8.GetString(((MemoryStream)receivedClipboardData.Formats[1].Data).ToArray());
            Assert.That(otherDirectory0, Does.Exist);
        }
    }
}