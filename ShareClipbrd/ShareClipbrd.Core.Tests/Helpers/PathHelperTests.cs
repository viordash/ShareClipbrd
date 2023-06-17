using ShareClipbrd.Core.Helpers;

namespace ShareClipbrd.Core.Tests.Helpers {
    public class PathHelperTests {
        [Test]
        public void IsAbsolute_Test() {
            Assert.True(PathHelper.IsAbsolute("C:\\ShareClipbrd\\ShareClipbrd.Core\\Clipboard\\ClipboardFile.cs"));
            Assert.True(PathHelper.IsAbsolute("C:\\ShareClipbrd//ClipboardFile.cs"));
            Assert.True(PathHelper.IsAbsolute("\\tsclient\\Documents.zip"));
            Assert.True(PathHelper.IsAbsolute("/home/Documents.zip"));

            Assert.False(PathHelper.IsAbsolute("Projects\\ShareClipbrd\\ShareClipbrd\\ShareClipbrd.Core\\Clipboard\\ClipboardFile.cs"));
            Assert.False(PathHelper.IsAbsolute("Documents"));
            Assert.False(PathHelper.IsAbsolute("   "));
            Assert.False(PathHelper.IsAbsolute("/"));
            Assert.False(PathHelper.IsAbsolute("//"));
            Assert.False(PathHelper.IsAbsolute("\\"));
            Assert.False(PathHelper.IsAbsolute("\\\\"));
        }
    }
}
