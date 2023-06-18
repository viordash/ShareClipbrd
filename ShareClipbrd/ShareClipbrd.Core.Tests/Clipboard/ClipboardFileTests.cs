using ShareClipbrd.Core.Clipboard;

namespace ShareClipbrd.Core.Tests.Clipboard {
    public class ClipboardFileTests {

        static Func<string, Task<object>> getDataFunc(string format, object obj) {
            return (f) => Task.FromResult(f == format
                ? obj
                : new()
            );
        }

        static List<string> files = new() {
                "/ShareClipbrd/ShareClipbrd.Core/Clipboard/ClipboardFile.cs",
                "/Users/user/Documents/New Bitmap Image.bmp",
                "/tsclient/Documents.zip",
                "   /file with spaces around   ",
                "copy"
            };

        static List<string> urls = new() {
                "file:///home/user/Downloads/main",
                "file:///home/user/Downloads/code_1_amd64.deb"
            };

        static List<string> outFiles = new() {
                "/ShareClipbrd/ShareClipbrd.Core/Clipboard/ClipboardFile.cs",
                "/Users/user/Documents/New Bitmap Image.bmp",
                "/tsclient/Documents.zip",
                "/file with spaces around"
            };

        static List<string> outUrls = new() {
                "/home/user/Downloads/main",
                "/home/user/Downloads/code_1_amd64.deb"
            };

        static List<string> incorrectItems = new() {
                "   no file path   ",
                "     ",
            };


        async Task GetFileDropList_Files_As_List_Test(string format) {
            var fileDropList = await ClipboardFile.GetList(new[] { format }, getDataFunc(format, files.Concat(incorrectItems).Concat(urls).ToList()));
            Assert.That(fileDropList, Is.EquivalentTo(outFiles));
        }

        async Task GetFileDropList_FilesAndUrls_As_StringLines_Test(string format) {
            var lines = string.Join("\r\n", files.Concat(incorrectItems).Concat(urls));
            var fileDropList = await ClipboardFile.GetList(new[] { format }, getDataFunc(format, lines));
            Assert.That(fileDropList, Is.EquivalentTo(outFiles.Concat(outUrls)));

            lines = string.Join("\n", files.Concat(incorrectItems).Concat(urls));
            fileDropList = await ClipboardFile.GetList(new[] { format }, getDataFunc(format, lines));
            Assert.That(fileDropList, Is.EquivalentTo(outFiles.Concat(outUrls)));
        }

        async Task GetFileDropList_FilesAndUrls_As_ByteArray_Test(string format) {
            var lines = string.Join("\r\n", files.Concat(incorrectItems).Concat(urls));
            var bytes = System.Text.Encoding.UTF8.GetBytes(lines);
            var fileDropList = await ClipboardFile.GetList(new[] { format }, getDataFunc(format, bytes));
            Assert.That(fileDropList, Is.EquivalentTo(outFiles.Concat(outUrls)));

            lines = string.Join("\n", files.Concat(incorrectItems).Concat(urls));
            bytes = System.Text.Encoding.UTF8.GetBytes(lines);
            fileDropList = await ClipboardFile.GetList(new[] { format }, getDataFunc(format, bytes));
            Assert.That(fileDropList, Is.EquivalentTo(outFiles.Concat(outUrls)));
        }

        [Test]
        public async Task GetFileDropList_FileNames_As_List_Test() {
            await GetFileDropList_Files_As_List_Test(ClipboardFile.Format.FileNames);
        }

        [Test]
        public void GetFileDropList_FileNames_As_StringLines_Test() {
            Assert.ThrowsAsync<InvalidDataException>(() => GetFileDropList_FilesAndUrls_As_StringLines_Test(ClipboardFile.Format.FileNames));
        }

        [Test]
        public void GetFileDropList_FileNames_As_ByteArray_Test() {
            Assert.ThrowsAsync<InvalidDataException>(() => GetFileDropList_FilesAndUrls_As_ByteArray_Test(ClipboardFile.Format.FileNames));
        }

        [Test]
        public void GetFileDropList_XMateFileNames_As_List_Test() {
            Assert.ThrowsAsync<InvalidDataException>(() => GetFileDropList_Files_As_List_Test(ClipboardFile.Format.XMateFileNames));
        }

        [Test]
        public async Task GetFileDropList_XMateFileNames_As_StringLines_Test() {
            await GetFileDropList_FilesAndUrls_As_StringLines_Test(ClipboardFile.Format.XMateFileNames);
        }

        [Test]
        public async Task GetFileDropList_XMateFileNames_As_ByteArray_Test() {
            await GetFileDropList_FilesAndUrls_As_ByteArray_Test(ClipboardFile.Format.XMateFileNames);
        }

        [Test]
        public void GetFileDropList_XKdeFileNames_As_List_Test() {
            Assert.ThrowsAsync<InvalidDataException>(() => GetFileDropList_Files_As_List_Test(ClipboardFile.Format.XKdeFileNames));
        }

        [Test]
        public async Task GetFileDropList_XKdeFileNames_As_StringLines_Test() {
            await GetFileDropList_FilesAndUrls_As_StringLines_Test(ClipboardFile.Format.XKdeFileNames);
        }

        [Test]
        public async Task GetFileDropList_XKdeFileNames_As_ByteArray_Test() {
            await GetFileDropList_FilesAndUrls_As_ByteArray_Test(ClipboardFile.Format.XKdeFileNames);
        }

        [Test]
        public void GetFileDropList_XGnomeFileNames_As_List_Test() {
            Assert.ThrowsAsync<InvalidDataException>(() => GetFileDropList_Files_As_List_Test(ClipboardFile.Format.XGnomeFileNames));
        }

        [Test]
        public async Task GetFileDropList_XGnomeFileNames_As_StringLines_Test() {
            await GetFileDropList_FilesAndUrls_As_StringLines_Test(ClipboardFile.Format.XGnomeFileNames);
        }

        [Test]
        public async Task GetFileDropList_XGnomeFileNames_As_ByteArray_Test() {
            await GetFileDropList_FilesAndUrls_As_ByteArray_Test(ClipboardFile.Format.XGnomeFileNames);
        }

        [Test]
        public async Task GetFileDropList_UnknownFormat_Test() {
            var fileDropList = await ClipboardFile.GetList(new[] { "UnknownFormat0", "UnknownFormat1" }, getDataFunc("UnknownFormat0", files.Concat(incorrectItems)));
            Assert.That(fileDropList, Is.Empty);
        }


    }
}
