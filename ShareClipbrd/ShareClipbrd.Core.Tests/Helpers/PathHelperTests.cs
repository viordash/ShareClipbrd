using ShareClipbrd.Core.Helpers;

namespace ShareClipbrd.Core.Tests.Helpers {
    public class PathHelperTests {
        [Test]
        public void IsAbsolute_Test() {
            if(OperatingSystem.IsWindows()) {
                Assert.That(PathHelper.IsAbsolute("C:\\ShareClipbrd\\ShareClipbrd.Core\\Clipboard\\ClipboardFile.cs"), Is.True);
                Assert.That(PathHelper.IsAbsolute("C:\\ShareClipbrd//ClipboardFile.cs"), Is.True);
                Assert.That(PathHelper.IsAbsolute("\\tsclient\\Documents.zip"), Is.True);
                Assert.That(PathHelper.IsAbsolute("//"), Is.False);
                Assert.That(PathHelper.IsAbsolute("//tsclient/Documents.zip"), Is.False);
                Assert.That(PathHelper.IsAbsolute("//file/1"), Is.False);
                Assert.That(PathHelper.IsAbsolute("//file"), Is.False);
            }

            if(OperatingSystem.IsLinux()) {
                Assert.That(PathHelper.IsAbsolute("/ShareClipbrd/ShareClipbrd.Core/Clipboard/ClipboardFile.cs"), Is.True);
                Assert.That(PathHelper.IsAbsolute("/ShareClipbrd//ClipboardFile.cs"), Is.True);
                Assert.That(PathHelper.IsAbsolute("/tsclient/Documents.zip"), Is.True);
                Assert.That(PathHelper.IsAbsolute("//"), Is.True);
                Assert.That(PathHelper.IsAbsolute("//tsclient/Documents.zip"), Is.True);
                Assert.That(PathHelper.IsAbsolute("//file/1"), Is.True);
                Assert.That(PathHelper.IsAbsolute("//file"), Is.True);
            }
            Assert.That(PathHelper.IsAbsolute("/home/Documents.zip"), Is.True);

            Assert.That(PathHelper.IsAbsolute("ShareClipbrd\\ShareClipbrd.Core\\Clipboard\\ClipboardFile.cs"), Is.False);
            Assert.That(PathHelper.IsAbsolute("ShareClipbrd/ShareClipbrd.Core/Clipboard/ClipboardFile.cs"), Is.False);
            Assert.That(PathHelper.IsAbsolute("Documents"), Is.False);
            Assert.That(PathHelper.IsAbsolute("   "), Is.False);
            Assert.That(PathHelper.IsAbsolute("/"), Is.False);
            Assert.That(PathHelper.IsAbsolute("\\"), Is.False);
            Assert.That(PathHelper.IsAbsolute("\\\\"), Is.False);
        }
    }
}
