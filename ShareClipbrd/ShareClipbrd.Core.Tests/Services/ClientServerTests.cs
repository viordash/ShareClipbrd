using System.Collections.Specialized;
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
            ClipboardData? receivedClipboard = null;

            server.Start((c) => receivedClipboard = c, _ => { });

            var clipboardData = new ClipboardData();
            clipboardData.Add("UnicodeText", new MemoryStream(System.Text.Encoding.Unicode.GetBytes("UnicodeText Кирилица")));

            await Task.Delay(100);
            await client.SendData(clipboardData);
            await Task.Delay(100);


            Assert.IsNotNull(receivedClipboard);
            Assert.That(receivedClipboard.Formats.Select(x => x.Format), Is.EquivalentTo(new[] { "UnicodeText" }));
            Assert.That(receivedClipboard.Formats.Select(x => x.Stream), Is.EquivalentTo(new[] { new MemoryStream(System.Text.Encoding.Unicode.GetBytes("UnicodeText Кирилица")) }));
        }

        [Test]
        public async Task Send_Common_Big_Data_Test() {
            ClipboardData? receivedClipboard = null;

            server.Start((c) => receivedClipboard = c, _ => { });

            var clipboardData = new ClipboardData();

            var rnd = new Random();
            var bytes = new byte[1_000_000_003];
            rnd.NextBytes(bytes);

            clipboardData.Add("Text", new MemoryStream(bytes));

            await Task.Delay(100);
            await client.SendData(clipboardData);
            await Task.Delay(1000);

            Assert.IsNotNull(receivedClipboard);
            Assert.That(receivedClipboard.Formats.Select(x => x.Format), Is.EquivalentTo(new[] { "Text" }));
            Assert.That(receivedClipboard.Formats.First(x => x.Format == "Text").Stream, Has.Length.EqualTo(1_000_000_003));
            Assert.That((receivedClipboard.Formats.First(x => x.Format == "Text").Stream).ToArray().Take(1_000_000), Is.EquivalentTo(bytes.Take(1_000_000)));
        }

        [Test]
        public async Task Send_Files_Test() {
            StringCollection? fileDropList = null;

            server.Start(_ => { }, (f) => fileDropList = f);

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
                var filename = Path.Combine(testsPath, "кирилица_" + Path.GetFileName(Path.GetTempFileName()));
                testdata.Read(buffer, 0, buffer.Length);

                File.WriteAllBytes(filename, buffer);
                files.Add(filename);
            }

            try {
                await client.SendFileDropList(files);
            } finally {
                Directory.Delete(testsPath, true);
            }
            await Task.Delay(1000);
            Assert.IsNotNull(fileDropList);
            Assert.That(fileDropList.Count, Is.EqualTo(100));


            testdata.Position = 0;
            for(int i = 0; i < 100; i++) {
                var otherFilename = fileDropList[i];

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
            StringCollection? fileDropList = null;

            server.Start(_ => { }, (f) => fileDropList = f);

            var rnd = new Random();
            var bytes = new byte[1_000_000_003];
            rnd.NextBytes(bytes);

            var testsPath = Path.Combine(Path.GetTempPath(), "tests");
            Directory.CreateDirectory(testsPath);

            var files = new StringCollection();
            var filename = Path.Combine(testsPath, Path.GetFileName(Path.GetTempFileName()));
            File.WriteAllBytes(filename, bytes);
            files.Add(filename);

            try {
                await client.SendFileDropList(files);
            } finally {
                Directory.Delete(testsPath, true);
            }
            await Task.Delay(1000);

            Assert.IsNotNull(fileDropList);
            var otherFilename = fileDropList[0];

            Assert.That(otherFilename, Does.Exist);

            var otherBytes = File.ReadAllBytes(otherFilename);
            File.Delete(otherFilename);

            Assert.That(otherBytes, Has.Length.EqualTo(1_000_000_003));
            Assert.That(otherBytes.Take(1_000_000), Is.EquivalentTo(bytes.Take(1_000_000)));

        }

        [Test]
        public async Task Send_Files_And_Folders_Test() {
            StringCollection? fileDropList = null;

            server.Start(_ => { }, (f) => fileDropList = f);


            var testsPath = Path.Combine(Path.GetTempPath(), "tests");
            Directory.CreateDirectory(testsPath);

            var rnd = new Random();
            var bytes0 = new byte[555_000];
            rnd.NextBytes(bytes0);
            var bytes1 = new byte[777_000];
            rnd.NextBytes(bytes1);

            var files = new StringCollection();

            var filename0 = Path.Combine(testsPath, "filename0");
            File.WriteAllBytes(filename0, bytes0);
            files.Add(filename0);

            var directory0 = Path.Combine(testsPath, "directory0");
            Directory.CreateDirectory(directory0);
            files.Add(directory0);

            var filename1 = Path.Combine(directory0, "filename1.bin");
            File.WriteAllBytes(filename1, bytes1);
            files.Add(filename1);

            var directory0_Child0 = Path.Combine(directory0, "directory0_Child0");
            Directory.CreateDirectory(directory0_Child0);

            var directory0_Child1 = Path.Combine(directory0, "директория0_Child1");
            Directory.CreateDirectory(directory0_Child1);
            files.Add(directory0_Child1);

            var directory0_Child1_Child0_Empty = Path.Combine(directory0_Child1, "directory0_Child1_Child0_Empty");
            Directory.CreateDirectory(directory0_Child1_Child0_Empty);
            files.Add(directory0_Child1_Child0_Empty);

            var filename2 = Path.Combine(directory0_Child1, "файл2.dat");
            File.WriteAllBytes(filename2, bytes1);

            try {
                await client.SendFileDropList(files);
            } finally {
                Directory.Delete(testsPath, true);

            }
            await Task.Delay(500);
            Assert.IsNotNull(fileDropList);
            Assert.That(fileDropList.Count, Is.EqualTo(9));

            var otherFilename = fileDropList[0];
            Assert.That(otherFilename, Does.Exist);
            Assert.That(Path.GetFileName(otherFilename), Is.EqualTo("filename0"));
            Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes0));

            otherFilename = fileDropList[1];
            Assert.That(otherFilename, Does.Exist);
            Assert.That(Path.GetFileName(otherFilename), Is.EqualTo("filename1.bin"));
            Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes1));

            otherFilename = fileDropList[2];
            Assert.That(otherFilename, Does.Exist);
            Assert.That(Path.GetFileName(otherFilename), Is.EqualTo("файл2.dat"));
            Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes1));

            var otherDirectory = fileDropList[3];
            Assert.That(otherDirectory, Does.Exist);
            Assert.That(otherDirectory, Does.EndWith("directory0\\directory0_Child0"));

            otherDirectory = fileDropList[4];
            Assert.That(otherDirectory, Does.Exist);
            Assert.That(otherDirectory, Does.EndWith("directory0\\директория0_Child1\\directory0_Child1_Child0_Empty"));

            otherFilename = fileDropList[5];
            Assert.That(otherFilename, Does.Exist);
            Assert.That(Path.GetFileName(otherFilename), Is.EqualTo("filename1.bin"));
            Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes1));

            otherFilename = fileDropList[6];
            Assert.That(otherFilename, Does.Exist);
            Assert.That(Path.GetFileName(otherFilename), Is.EqualTo("файл2.dat"));
            Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes1));

            otherDirectory = fileDropList[7];
            Assert.That(otherDirectory, Does.Exist);
            Assert.That(otherDirectory, Does.EndWith("директория0_Child1\\directory0_Child1_Child0_Empty"));

            otherDirectory = fileDropList[8];
            Assert.That(otherDirectory, Does.Exist);
            Assert.That(otherDirectory, Does.EndWith("directory0_Child1_Child0_Empty"));
        }

        //[Test]
        //public async Task Send_identical_Files__Test() {
        //    StringCollection? fileDropList = null;

        //    server.Start((c, f) => fileDropList = f);

        //    var clipboardData = new ClipboardData();

        //    var testsPath = Path.Combine(Path.GetTempPath(), "tests");
        //    Directory.CreateDirectory(testsPath);

        //    var rnd = new Random();
        //    var bytes0 = new byte[1_000];
        //    rnd.NextBytes(bytes0);

        //    var filename0 = Path.Combine(testsPath, "filename0");
        //    File.WriteAllBytes(filename0, bytes0);
        //    clipboardData.Add(ClipboardData.Format.FileDrop, new FileStream(filename0, FileMode.Open, FileAccess.Read, FileShare.Read));

        //    clipboardData.Add(ClipboardData.Format.FileDrop, new FileStream(filename0, FileMode.Open, FileAccess.Read, FileShare.Read));

        //    clipboardData.Add(ClipboardData.Format.FileDrop, new FileStream(filename0, FileMode.Open, FileAccess.Read, FileShare.Read));

        //    try {
        //        await client.Send(clipboardData);
        //    } finally {
        //        foreach(var fileStream in clipboardData.Formats
        //            .Where(x => x.Format == ClipboardData.Format.FileDrop)
        //            .Select(x => x.Data)
        //            .Cast<FileStream>()) {
        //            fileStream.Close();
        //        }
        //        Directory.Delete(testsPath, true);

        //    }
        //    await Task.Delay(500);
        //    Assert.IsNotNull(fileDropList);
        //    Assert.That(fileDropList.Count, Is.EqualTo(3));

        //    var otherFilename = fileDropList[0];
        //    Assert.That(otherFilename, Does.Exist);
        //    Assert.That(Path.GetFileName(otherFilename), Is.EqualTo("filename0"));
        //    Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes0));

        //    otherFilename = fileDropList[1];
        //    Assert.That(otherFilename, Does.Exist);
        //    Assert.That(Path.GetFileName(otherFilename), Is.EqualTo("filename0"));
        //    Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes0));

        //    otherFilename = fileDropList[2];
        //    Assert.That(otherFilename, Does.Exist);
        //    Assert.That(Path.GetFileName(otherFilename), Is.EqualTo("filename0"));
        //    Assert.That(File.ReadAllBytes(otherFilename), Is.EquivalentTo(bytes0));


        //}
    }
}