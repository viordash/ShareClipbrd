using System.Collections.Specialized;
using Moq;
using ShareClipbrd.Core.Clipboard;
using ShareClipbrd.Core.Services;

namespace ShareClipbrd.Core.Tests.Clipboard {
    public class FileTransmitterTests {

        Mock<IProgressService> progressServiceMock;
        CancellationTokenSource cts;

        [SetUp]
        public void Setup() {
            progressServiceMock = new();
            cts = new();
        }

        [TearDown]
        public void Teardown() {
        }

        [Test]
        public async Task Send_Files_And_Folders_Test() {

            var transmitStream = new MemoryStream();

            var testable = new FileTransmitter(progressServiceMock.Object, transmitStream);

            var testsPath = Path.Combine(Path.GetTempPath(), "tests");
            Directory.CreateDirectory(testsPath);

            var rnd = new Random();
            var bytes0 = new byte[100];
            rnd.NextBytes(bytes0);
            var bytes1 = new byte[200];
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
                await testable.Send(files, cts.Token);
            } finally {
                Directory.Delete(testsPath, true);

            }

            Assert.That(transmitStream.Length, Is.EqualTo(100));


            progressServiceMock.Verify(x => x.Begin(It.Is<ProgressMode>(p => p == ProgressMode.Send)), Times.Once);
            progressServiceMock.Verify(x => x.SetMaxTick(It.IsAny<Int64>()), Times.Exactly(1));
            progressServiceMock.Verify(x => x.Tick(It.IsAny<Int64>()), Times.Exactly(9));
        }

    }
}