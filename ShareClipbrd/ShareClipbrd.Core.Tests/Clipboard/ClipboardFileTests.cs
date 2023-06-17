using ShareClipbrd.Core.Clipboard;

namespace ShareClipbrd.Core.Tests.Clipboard {
    public class ClipboardFileTests {

        static Func<string, Task<object>> getDataFunc(string format, object obj) {
            return (f) => Task.FromResult(f == format
                ? obj
                : new()
            );
        }

        List<string> files = new() {
                "/ShareClipbrd/ShareClipbrd.Core/Clipboard/ClipboardFile.cs",
                "/Users/user/Documents/New Bitmap Image.bmp",
                "/tsclient/Documents.zip",
                "   /file with spaces around   ",
            };

        List<string> incorrectItems = new() {
                "   no file path   ",
                "     ",
            };


        async Task GetFileDropList_Data_As_List_Test(string format) {
            var fileDropList = await ClipboardFile.GetFileDropList(new[] { format }, getDataFunc(format, files.Concat(incorrectItems).ToList()));
            Assert.That(fileDropList, Is.EquivalentTo(files.Select(x => x.Trim())));
        }

        async Task GetFileDropList_Data_As_StringLines_Test(string format) {
            var lines = string.Join("\r\n", files.Concat(incorrectItems));
            var fileDropList = await ClipboardFile.GetFileDropList(new[] { format }, getDataFunc(format, lines));
            Assert.That(fileDropList, Is.EquivalentTo(files.Select(x => x.Trim())));

            lines = string.Join("\n", files.Concat(incorrectItems));
            fileDropList = await ClipboardFile.GetFileDropList(new[] { format }, getDataFunc(format, lines));
            Assert.That(fileDropList, Is.EquivalentTo(files.Select(x => x.Trim())));
        }

        async Task GetFileDropList_Data_As_ByteArray_Test(string format) {
            var lines = string.Join("\r\n", files.Concat(incorrectItems));
            var bytes = System.Text.Encoding.UTF8.GetBytes(lines);
            var fileDropList = await ClipboardFile.GetFileDropList(new[] { format }, getDataFunc(format, bytes));
            Assert.That(fileDropList, Is.EquivalentTo(files.Select(x => x.Trim())));

            lines = string.Join("\n", files.Concat(incorrectItems));
            bytes = System.Text.Encoding.UTF8.GetBytes(lines);
            fileDropList = await ClipboardFile.GetFileDropList(new[] { format }, getDataFunc(format, bytes));
            Assert.That(fileDropList, Is.EquivalentTo(files.Select(x => x.Trim())));
        }

        [Test]
        public async Task GetFileDropList_FileNames_As_List_Test() {
            await GetFileDropList_Data_As_List_Test(ClipboardFile.Format.FileNames);
        }

        [Test]
        public async Task GetFileDropList_FileNames_As_StringLines_Test() {
            await GetFileDropList_Data_As_StringLines_Test(ClipboardFile.Format.FileNames);
        }

        [Test]
        public async Task GetFileDropList_FileNames_As_ByteArray_Test() {
            await GetFileDropList_Data_As_ByteArray_Test(ClipboardFile.Format.FileNames);
        }

        [Test]
        public async Task GetFileDropList_XMateFileNames_As_List_Test() {
            await GetFileDropList_Data_As_List_Test(ClipboardFile.Format.XMateFileNames);
        }

        [Test]
        public async Task GetFileDropList_XMateFileNames_As_StringLines_Test() {
            await GetFileDropList_Data_As_StringLines_Test(ClipboardFile.Format.XMateFileNames);
        }

        [Test]
        public async Task GetFileDropList_XMateFileNames_As_ByteArray_Test() {
            await GetFileDropList_Data_As_ByteArray_Test(ClipboardFile.Format.XMateFileNames);
        }

        [Test]
        public async Task GetFileDropList_XKdeFileNames_As_List_Test() {
            await GetFileDropList_Data_As_List_Test(ClipboardFile.Format.XKdeFileNames);
        }

        [Test]
        public async Task GetFileDropList_XKdeFileNames_As_StringLines_Test() {
            await GetFileDropList_Data_As_StringLines_Test(ClipboardFile.Format.XKdeFileNames);
        }

        [Test]
        public async Task GetFileDropList_XKdeFileNames_As_ByteArray_Test() {
            await GetFileDropList_Data_As_ByteArray_Test(ClipboardFile.Format.XKdeFileNames);
        }

        [Test]
        public async Task GetFileDropList_XGnomeFileNames_As_List_Test() {
            await GetFileDropList_Data_As_List_Test(ClipboardFile.Format.XGnomeFileNames);
        }

        [Test]
        public async Task GetFileDropList_XGnomeFileNames_As_StringLines_Test() {
            await GetFileDropList_Data_As_StringLines_Test(ClipboardFile.Format.XGnomeFileNames);
        }

        [Test]
        public async Task GetFileDropList_XGnomeFileNames_As_ByteArray_Test() {
            await GetFileDropList_Data_As_ByteArray_Test(ClipboardFile.Format.XGnomeFileNames);
        }

        [Test]
        public async Task GetFileDropList_UnknownFormat_Test() {
            var fileDropList = await ClipboardFile.GetFileDropList(new[] { "UnknownFormat0", "UnknownFormat1" }, getDataFunc("UnknownFormat0", files.Concat(incorrectItems)));
            Assert.That(fileDropList, Is.Empty);
        }


    }
}
