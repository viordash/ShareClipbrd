using System.Collections.Specialized;
using System.Diagnostics;
using Moq;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Configuration;
using ShareClipbrd.Core.Helpers;
using ShareClipbrd.Core.Services;

namespace ShareClipbrd.Core.Tests.Services {
    public class Tests {
        Mock<ISystemConfiguration> systemConfigurationMock;
        Mock<IDialogService> dialogServiceMock;
        Mock<IDispatchService> dispatchServiceMock;
        Mock<IProgressService> progressServiceMock;
        Mock<IConnectStatusService> connectStatusServiceMock;
        Mock<ITimeService> timeServiceMock;
        DataServer server;
        DataClient client;

        bool clientConnected;

        [SetUp]
        public void Setup() {
            systemConfigurationMock = new();
            dialogServiceMock = new();
            dispatchServiceMock = new();
            progressServiceMock = new();
            connectStatusServiceMock = new();
            timeServiceMock = new();

            systemConfigurationMock.SetupGet(x => x.HostAddress).Returns("127.0.0.1:55542");
            systemConfigurationMock.SetupGet(x => x.PartnerAddress).Returns("127.0.0.1:55542");

            server = new DataServer(systemConfigurationMock.Object, dialogServiceMock.Object, dispatchServiceMock.Object,
                progressServiceMock.Object, connectStatusServiceMock.Object);
            client = new DataClient(systemConfigurationMock.Object, progressServiceMock.Object,
                connectStatusServiceMock.Object, dialogServiceMock.Object, timeServiceMock.Object);

            clientConnected = false;
            connectStatusServiceMock
                .Setup(x => x.ClientOnline())
                .Callback(() => {
                    clientConnected = true;
                });

            connectStatusServiceMock
                .Setup(x => x.ClientOffline())
                .Callback(() => {
                    clientConnected = false;
                });

            timeServiceMock.SetupGet(x => x.DataClientPollTime).Returns(TimeSpan.FromMilliseconds(100));
        }

        [TearDown]
        public void Teardown() {
        }

        async Task AwaitClientConnectStatus(bool isConnected) {
            var cts = new CancellationTokenSource(5000);
            while(clientConnected != isConnected && !cts.IsCancellationRequested) {
                await Task.Delay(10);
            }
            Assert.That(clientConnected, Is.EqualTo(isConnected));
        }

        [Test]
        public async Task Send_CommonData_Test() {
            ClipboardData? receivedClipboard = null;

            dispatchServiceMock
                .Setup(x => x.ReceiveData(It.IsAny<ClipboardData>()))
                .Callback<ClipboardData>(x => receivedClipboard = x);

            server.Start();
            client.Start();
            await AwaitClientConnectStatus(true);

            var clipboardData = new ClipboardData();
            clipboardData.Add("UnicodeText", new MemoryStream(System.Text.Encoding.Unicode.GetBytes("UnicodeText юникод Œ")));
            await client.SendData(clipboardData);

            clipboardData = new ClipboardData();
            clipboardData.Add("Text", new MemoryStream(System.Text.Encoding.Unicode.GetBytes("Text 0123456789")));

            await client.SendData(clipboardData);
            client.Stop();
            await AwaitClientConnectStatus(false);
            await server.Stop();

            dispatchServiceMock.Verify(x => x.ReceiveData(It.IsAny<ClipboardData>()), Times.Exactly(2));
            Assert.IsNotNull(receivedClipboard);
            Assert.That(receivedClipboard.Formats.Select(x => x.Format), Is.EquivalentTo(new[] { "UnicodeText", "Text" }));
            Assert.That(receivedClipboard.Formats.Select(x => x.Stream), Is.EquivalentTo(new[] {
                new MemoryStream(System.Text.Encoding.Unicode.GetBytes("UnicodeText юникод Œ")),
                new MemoryStream(System.Text.Encoding.Unicode.GetBytes("Text 0123456789"))
            }));
        }

        [Test]
        public async Task Send_Common_Big_Data_Test() {
            ClipboardData? receivedClipboard = null;

            dispatchServiceMock
                .Setup(x => x.ReceiveData(It.IsAny<ClipboardData>()))
                .Callback<ClipboardData>(x => receivedClipboard = x);

            server.Start();
            client.Start();
            await AwaitClientConnectStatus(true);

            var clipboardData = new ClipboardData();

            var rnd = new Random();
            var bytes = new byte[1_000_000_003];
            rnd.NextBytes(bytes);

            clipboardData.Add("Text", new MemoryStream(bytes));

            await client.SendData(clipboardData);
            client.Stop();
            await AwaitClientConnectStatus(false);
            await server.Stop();

            dispatchServiceMock.VerifyAll();
            Assert.IsNotNull(receivedClipboard);
            Assert.That(receivedClipboard.Formats.Select(x => x.Format), Is.EquivalentTo(new[] { "Text" }));
            Assert.That(receivedClipboard.Formats.First(x => x.Format == "Text").Stream, Has.Length.EqualTo(1_000_000_003));
            Assert.That(receivedClipboard.Formats.First(x => x.Format == "Text").Stream.ToArray().Take(1_000_000), Is.EquivalentTo(bytes.Take(1_000_000)));

        }

        [Test]
        public async Task Send_Files_Test() {
            IList<string>? fileDropList = null;

            dispatchServiceMock
                .Setup(x => x.ReceiveFiles(It.IsAny<IList<string>>()))
                .Callback<IList<string>>(x => fileDropList = x);

            var clipboardData = new ClipboardData();

            var testsPath = Path.Combine(Path.GetTempPath(), "tests");
            Directory.CreateDirectory(testsPath);

            var rnd = new Random();
            var bytes = new byte[3_333_333];
            rnd.NextBytes(bytes);
            var testdata = new MemoryStream(bytes);

            var files = new StringCollection();

            var buffer = new byte[3_333_333 / 100];
            testdata.Position = 0;
            for(int i = 0; i < 100; i++) {
                var filename = Path.Combine(testsPath, $"Unicode юникод ® _{i}");
                testdata.Read(buffer, 0, buffer.Length);

                File.WriteAllBytes(filename, buffer);
                files.Add(filename);
            }

            server.Start();
            client.Start();
            await AwaitClientConnectStatus(true);

            try {
                await client.SendFileDropList(files);
                await client.SendFileDropList(files);
                await client.SendFileDropList(files);

            } finally {
                Directory.Delete(testsPath, true);
                client.Stop();
                await AwaitClientConnectStatus(false);
                await server.Stop();
            }

            dispatchServiceMock.Verify(x => x.ReceiveFiles(It.IsAny<IList<string>>()), Times.Exactly(3));
            Assert.IsNotNull(fileDropList);
            Assert.That(fileDropList.Count, Is.EqualTo(100));


            testdata.Position = 0;
            for(int i = 0; i < 100; i++) {
                var otherFilename = fileDropList.First(x => x.EndsWith($"Unicode юникод ® _{i}"));

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
            IList<string>? fileDropList = null;

            dispatchServiceMock
                .Setup(x => x.ReceiveFiles(It.IsAny<IList<string>>()))
                .Callback<IList<string>>(x => fileDropList = x);

            var rnd = new Random();
            var bytes = new byte[1_000_003];
            rnd.NextBytes(bytes);

            var testsPath = Path.Combine(Path.GetTempPath(), "tests");
            Directory.CreateDirectory(testsPath);

            var files = new StringCollection();
            var filename = Path.Combine(testsPath, Path.GetFileName(Path.GetTempFileName()));

            using(var fs = new FileStream(filename, FileMode.CreateNew)) {
                fs.Write(bytes);
                fs.Seek(4096L * 1024 * 1024, SeekOrigin.Begin);
                fs.WriteByte(0);
            }

            files.Add(filename);

            server.Start();
            client.Start();
            await AwaitClientConnectStatus(true);

            try {
                await client.SendFileDropList(files);
            } finally {
                Directory.Delete(testsPath, true);
                client.Stop();
                await AwaitClientConnectStatus(false);
                await server.Stop();
            }

            dispatchServiceMock.VerifyAll();
            Assert.IsNotNull(fileDropList);
            var otherFilename = fileDropList[0];

            Assert.That(otherFilename, Does.Exist);


            using(var fs = new FileStream(otherFilename, FileMode.Open, FileAccess.Read)) {
                Assert.That(fs.Length, Is.EqualTo(4096L * 1024 * 1024 + 1));

                var otherBytes = new byte[1_000_003];
                fs.Read(otherBytes);
                Assert.That(otherBytes, Is.EquivalentTo(bytes));
            }
            File.Delete(otherFilename);

        }

        [Test]
        public async Task Send_Files_And_Folders_Test() {
            IList<string>? fileDropList = null;

            dispatchServiceMock
                .Setup(x => x.ReceiveFiles(It.IsAny<IList<string>>()))
                .Callback<IList<string>>(x => fileDropList = x);


            var testsPath = Path.Combine(Path.GetTempPath(), "tests");
            Directory.CreateDirectory(testsPath);

            var rnd = new Random();
            var bytes0 = new byte[555_000];
            rnd.NextBytes(bytes0);
            var bytes1 = new byte[777_000];
            rnd.NextBytes(bytes1);

            var filename0 = Path.Combine(testsPath, "filename0.bin");
            File.WriteAllBytes(filename0, bytes0);

            var directory0 = Path.Combine(testsPath, "directory0");
            Directory.CreateDirectory(directory0);

            var directory0_filename1 = Path.Combine(directory0, "filename1.bin");
            File.WriteAllBytes(directory0_filename1, bytes1);

            Directory.CreateDirectory(Path.Combine(directory0, "Child0"));

            var directory0_child1 = Path.Combine(directory0, "Дочерний1™");
            Directory.CreateDirectory(directory0_child1);

            var directory0_child1_empty0 = Path.Combine(directory0_child1, "Empty0");
            Directory.CreateDirectory(directory0_child1_empty0);

            var filename2 = Path.Combine(directory0_child1, "Файл2.dat");
            File.WriteAllBytes(filename2, bytes1);

            var files = new StringCollection();
            files.Add(filename0);
            files.Add(directory0);
            files.Add(directory0_filename1);
            files.Add(directory0_child1);
            files.Add(directory0_child1_empty0);

            server.Start();
            client.Start(); 
            await AwaitClientConnectStatus(true);
            try {
                await client.SendFileDropList(files);
            } finally {
                Directory.Delete(testsPath, true);
                client.Stop();
                await AwaitClientConnectStatus(false);
                await server.Stop();
            }

            dispatchServiceMock.VerifyAll();
            Assert.IsNotNull(fileDropList);

            Assert.That(fileDropList.Count, Is.EqualTo(5));

            Assert.That(fileDropList.First(x => Path.GetFileName(x) == "filename0.bin"), Does.Exist);
            Assert.That(fileDropList.First(x => x.EndsWith("directory0")), Does.Exist);
            Assert.That(fileDropList.First(x => Path.GetFileName(x) == "filename1.bin"), Does.Exist);
            Assert.That(fileDropList.First(x => x.EndsWith("Дочерний1™")), Does.Exist);
            Assert.That(fileDropList.First(x => x.EndsWith("Empty0")), Does.Exist);

            const string pathShareClipbrd = "ShareClipbrd_60D54950";
            var tempDir = Path.Combine(Path.GetTempPath(), pathShareClipbrd);
            var storedFiles = DirectoryHelper.RecursiveGetFiles(tempDir)
                .Concat(DirectoryHelper.RecursiveGetEmptyFolders(tempDir))
                .Select(x => x.Replace('\\', Path.AltDirectorySeparatorChar));
            Assert.That(storedFiles.Count, Is.EqualTo(9));

            var otherFilename = storedFiles.First(x => x.EndsWith(pathShareClipbrd + "/filename0.bin"));
            Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes0));

            otherFilename = storedFiles.First(x => x.EndsWith(pathShareClipbrd + "/filename1.bin"));
            Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes1));

            var otherDirectory = storedFiles.First(x => x.EndsWith(pathShareClipbrd + "/Empty0"));
            Assert.That(otherDirectory, Does.Exist);

            otherDirectory = storedFiles.First(x => x.EndsWith("directory0/Child0"));
            Assert.That(otherDirectory, Does.Exist);

            otherDirectory = storedFiles.First(x => x.EndsWith("directory0/Дочерний1™/Empty0"));
            Assert.That(otherDirectory, Does.Exist);

            otherFilename = storedFiles.First(x => x.EndsWith("directory0/Дочерний1™/Файл2.dat"));
            Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes1));

            otherFilename = storedFiles.First(x => x.EndsWith("directory0/filename1.bin"));
            Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes1));

            otherDirectory = storedFiles.First(x => x.EndsWith(pathShareClipbrd + "/Дочерний1™/Empty0"));
            Assert.That(otherDirectory, Does.Exist);

            otherFilename = storedFiles.First(x => x.EndsWith(pathShareClipbrd + "/Дочерний1™/Файл2.dat"));
            Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes1));

            progressServiceMock.Verify(x => x.Begin(It.Is<ProgressMode>(p => p == ProgressMode.Send)), Times.Once);
            progressServiceMock.Verify(x => x.Begin(It.Is<ProgressMode>(p => p == ProgressMode.Receive)), Times.Once);
            progressServiceMock.Verify(x => x.SetMaxTick(It.IsAny<Int64>()), Times.Exactly(2));
            progressServiceMock.Verify(x => x.Tick(It.IsAny<Int64>()), Times.Exactly(9 * 2));
        }

        [Test]
        public async Task Send_identical_Files__Test() {
            IList<string>? fileDropList = null;

            dispatchServiceMock
                .Setup(x => x.ReceiveFiles(It.IsAny<IList<string>>()))
                .Callback<IList<string>>(x => fileDropList = x);


            var testsPath = Path.Combine(Path.GetTempPath(), "tests");
            Directory.CreateDirectory(testsPath);

            var rnd = new Random();
            var bytes0 = new byte[1000];
            rnd.NextBytes(bytes0);

            var files = new StringCollection();

            var filename0 = Path.Combine(testsPath, "filename0.bin");
            File.WriteAllBytes(filename0, bytes0);
            files.Add(filename0);
            files.Add(filename0);
            files.Add(filename0);

            server.Start();
            client.Start();
            await AwaitClientConnectStatus(true);
            try {
                await client.SendFileDropList(files);
            } finally {
                Directory.Delete(testsPath, true);
                client.Stop();
                await AwaitClientConnectStatus(false);
                await server.Stop();

            }

            dispatchServiceMock.VerifyAll();
            Assert.IsNotNull(fileDropList);
            Assert.That(fileDropList.Count, Is.EqualTo(1));

            var otherFilename = fileDropList[0];
            Assert.That(otherFilename, Does.Exist);
            Assert.That(Path.GetFileName(otherFilename), Is.EqualTo("filename0.bin"));
            Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes0));

            progressServiceMock.Verify(x => x.Begin(It.Is<ProgressMode>(p => p == ProgressMode.Send)), Times.Once);
            progressServiceMock.Verify(x => x.Begin(It.Is<ProgressMode>(p => p == ProgressMode.Receive)), Times.Once);
            progressServiceMock.Verify(x => x.SetMaxTick(It.IsAny<Int64>()), Times.Exactly(2));
            progressServiceMock.Verify(x => x.Tick(It.IsAny<Int64>()), Times.Exactly(2));
        }

        [Test]
        public async Task Client_Connect_Pooling_Test() {
            systemConfigurationMock.SetupGet(x => x.HostAddress).Returns("127.0.0.1:55542");
            systemConfigurationMock.SetupGet(x => x.PartnerAddress).Returns("127.0.0.1:0");

            server.Start();
            client.Start();
            await Task.Delay(1000);

            connectStatusServiceMock.Verify(x => x.ClientOffline(), Times.Once());
            connectStatusServiceMock.Verify(x => x.ClientOnline(), Times.Never());

            systemConfigurationMock.SetupGet(x => x.PartnerAddress).Returns("127.0.0.1:55542");

            await AwaitClientConnectStatus(true);
            connectStatusServiceMock.Verify(x => x.ClientOnline(), Times.Once());

            client.Stop();
            await AwaitClientConnectStatus(false);
            await server.Stop();
        }

        [Test]
        public async Task Client_Reconnecting_Test() {
            server.Start();
            client.Start();

            await AwaitClientConnectStatus(true);

            connectStatusServiceMock.Verify(x => x.ClientOnline(), Times.Once());
            connectStatusServiceMock.Verify(x => x.ClientOffline(), Times.Once());

            await server.Stop();
            await AwaitClientConnectStatus(false);

            server.Start();
            await AwaitClientConnectStatus(true);

            connectStatusServiceMock.Verify(x => x.ClientOffline(), Times.Exactly(2));
            connectStatusServiceMock.Verify(x => x.ClientOnline(), Times.Exactly(2));

            var clipboardData = new ClipboardData();
            clipboardData.Add("UnicodeText", new MemoryStream(System.Text.Encoding.Unicode.GetBytes("UnicodeText юникод Œ")));
            await client.SendData(clipboardData);

            dispatchServiceMock.Verify(x => x.ReceiveData(It.IsAny<ClipboardData>()), Times.Once);

            client.Stop();
            await AwaitClientConnectStatus(false);
            await server.Stop();
        }

        [Test]
        public async Task Client_Stop_Test() {
            server.Start();
            client.Start();
            await AwaitClientConnectStatus(true);

            connectStatusServiceMock.Verify(x => x.ClientOffline(), Times.Once());
            connectStatusServiceMock.Verify(x => x.ClientOnline(), Times.Once());


            client.Stop();
            await AwaitClientConnectStatus(false);
            connectStatusServiceMock.Verify(x => x.ClientOffline(), Times.Exactly(2));
            connectStatusServiceMock.Verify(x => x.ClientOnline(), Times.Once());

            await server.Stop();
        }

        [Test]
        public async Task Sequential_Calls_Stop_Server_Test() {
            await server.Stop();
            connectStatusServiceMock.Verify(x => x.Offline(), Times.Once());
            await server.Stop();
            connectStatusServiceMock.Verify(x => x.Offline(), Times.Exactly(2));
        }
    }
}
