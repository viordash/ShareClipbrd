using ShareClipbrd.Core.Helpers;

namespace ShareClipbrd.Core.Tests.Helpers {
    public class PathHelperTests {
        [Test]
        public void IsAbsolute_Test() {
            if(OperatingSystem.IsWindows()) {
                Assert.True(PathHelper.IsAbsolute("C:\\ShareClipbrd\\ShareClipbrd.Core\\Clipboard\\ClipboardFile.cs"));
                Assert.True(PathHelper.IsAbsolute("C:\\ShareClipbrd//ClipboardFile.cs"));
                Assert.True(PathHelper.IsAbsolute("\\tsclient\\Documents.zip"));
                Assert.False(PathHelper.IsAbsolute("//"));
                Assert.False(PathHelper.IsAbsolute("//tsclient/Documents.zip"));
                Assert.False(PathHelper.IsAbsolute("//file/1"));
                Assert.False(PathHelper.IsAbsolute("//file"));
            }

            if(OperatingSystem.IsLinux()) {
                Assert.True(PathHelper.IsAbsolute("/ShareClipbrd/ShareClipbrd.Core/Clipboard/ClipboardFile.cs"));
                Assert.True(PathHelper.IsAbsolute("/ShareClipbrd//ClipboardFile.cs"));
                Assert.True(PathHelper.IsAbsolute("/tsclient/Documents.zip"));
                Assert.True(PathHelper.IsAbsolute("//"));
                Assert.True(PathHelper.IsAbsolute("//tsclient/Documents.zip"));
                Assert.True(PathHelper.IsAbsolute("//file/1"));
                Assert.True(PathHelper.IsAbsolute("//file"));
            }
            Assert.True(PathHelper.IsAbsolute("/home/Documents.zip"));

            Assert.False(PathHelper.IsAbsolute("ShareClipbrd\\ShareClipbrd.Core\\Clipboard\\ClipboardFile.cs"));
            Assert.False(PathHelper.IsAbsolute("ShareClipbrd/ShareClipbrd.Core/Clipboard/ClipboardFile.cs"));
            Assert.False(PathHelper.IsAbsolute("Documents"));
            Assert.False(PathHelper.IsAbsolute("   "));
            Assert.False(PathHelper.IsAbsolute("/"));
            Assert.False(PathHelper.IsAbsolute("\\"));
            Assert.False(PathHelper.IsAbsolute("\\\\"));
        }
    }
}
