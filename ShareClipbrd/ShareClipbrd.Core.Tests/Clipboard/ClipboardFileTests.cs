using ShareClipbrd.Core.Clipboard;

namespace ShareClipbrd.Core.Tests.Clipboard {
    public class ClipboardFileTests {

        static Func<string, Task<object>> getDataFunc(string format, object obj) {
            return (f) => Task.FromResult(f == format
                ? obj
                : new()
            );
        }

        static List<string> filesWindows = new() {
                "c:/Share папка Clipbrd/Clipboard файл File.cs",
                "C:\\Share папка Clipbrd\\Share файл Clipbrd.Core",
                "    c:/Share папка Clipbrd/Clipboard файл File.cs        ",
                "       C:\\Share папка Clipbrd\\Share файл Clipbrd.Core           ",
                "file://c:/Share%20Clipbrd/Clipboard%20File.cs",
                "file://C:\\Share%20Clipbrd\\Share%20Clipbrd.Core",
                "/tsclient/Documents.zip"
            };

        static List<string> outFilesWindows = new() {
                "c:\\Share папка Clipbrd\\Clipboard файл File.cs",
                "C:\\Share папка Clipbrd\\Share файл Clipbrd.Core",
                "c:\\Share папка Clipbrd\\Clipboard файл File.cs",
                "C:\\Share папка Clipbrd\\Share файл Clipbrd.Core",
                "c:\\Share Clipbrd\\Clipboard File.cs",
                "C:\\Share Clipbrd\\Share Clipbrd.Core",
            };

        static List<string> filesLinux = new() {
                "/home/пользователь/Downloads/main",
                "/home/пользователь/Downloads/code 1 amd64.deb",
                "    /home/пользователь/Downloads/main   ",
                "           /home/пользователь/Downloads/code 1 amd64.deb       ",
                "file:///home/user/Downloads/main",
                "file:///home/user/Downloads/code%201%20amd64.deb",
                "C:\\Share папка Clipbrd\\Share файл Clipbrd.Core",
            };

        static List<string> outFilesLinux = new() {
                "/home/пользователь/Downloads/main",
                "/home/пользователь/Downloads/code 1 amd64.deb",
                "/home/пользователь/Downloads/main",
                "/home/пользователь/Downloads/code 1 amd64.deb",
                "/home/user/Downloads/main",
                "/home/user/Downloads/code 1 amd64.deb",
                "C:\\Share папка Clipbrd\\Share файл Clipbrd.Core"
            };

        static List<string> incorrectItems = new() {
                "   no file path   ",
                "     ",
            };


        async Task GetFileDropList_Files_As_List_Test(string format) {
            if(OperatingSystem.IsWindows()) {
                var fileDropList = await ClipboardFile.GetList(new[] { format }, getDataFunc(format, filesWindows.Concat(incorrectItems).ToList()));
                Assert.That(fileDropList, Is.EquivalentTo(outFilesWindows));
                return;
            }
            if(OperatingSystem.IsLinux()) {
                var fileDropList = await ClipboardFile.GetList(new[] { format }, getDataFunc(format, filesLinux.Concat(incorrectItems).ToList()));
                Assert.That(fileDropList, Is.EquivalentTo(outFilesLinux));
                return;
            }
            throw new NotSupportedException($"OS: {Environment.OSVersion}");
        }

        async Task GetFileDropList_FilesAndUrls_As_StringLines_Test(string format) {
            List<string> files;
            List<string> outFiles;

            if(OperatingSystem.IsWindows()) {
                files = filesWindows;
                outFiles = outFilesWindows;
            } else if(OperatingSystem.IsLinux()) {
                files = filesLinux;
                outFiles = outFilesLinux;
            } else {
                throw new NotSupportedException($"OS: {Environment.OSVersion}");
            }

            var lines = string.Join("\r\n", files.Concat(incorrectItems));
            var fileDropList = await ClipboardFile.GetList(new[] { format }, getDataFunc(format, lines));
            Assert.That(fileDropList, Is.EquivalentTo(outFiles));

            lines = string.Join("\n", files.Concat(incorrectItems));
            fileDropList = await ClipboardFile.GetList(new[] { format }, getDataFunc(format, lines));
            Assert.That(fileDropList, Is.EquivalentTo(outFiles));
        }

        async Task GetFileDropList_FilesAndUrls_As_ByteArray_Test(string format) {
            List<string> files;
            List<string> outFiles;

            if(OperatingSystem.IsWindows()) {
                files = filesWindows;
                outFiles = outFilesWindows;
            } else if(OperatingSystem.IsLinux()) {
                files = filesLinux;
                outFiles = outFilesLinux;
            } else {
                throw new NotSupportedException($"OS: {Environment.OSVersion}");
            }

            var lines = string.Join("\r\n", files.Concat(incorrectItems));
            var bytes = System.Text.Encoding.UTF8.GetBytes(lines);
            var fileDropList = await ClipboardFile.GetList(new[] { format }, getDataFunc(format, bytes));
            Assert.That(fileDropList, Is.EquivalentTo(outFiles));

            lines = string.Join("\n", files.Concat(incorrectItems));
            bytes = System.Text.Encoding.UTF8.GetBytes(lines);
            fileDropList = await ClipboardFile.GetList(new[] { format }, getDataFunc(format, bytes));
            Assert.That(fileDropList, Is.EquivalentTo(outFiles));
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
            if(OperatingSystem.IsWindows()) {
                var fileDropList = await ClipboardFile.GetList(new[] { "UnknownFormat0", "UnknownFormat1" }, getDataFunc("UnknownFormat0", filesWindows.Concat(incorrectItems)));
                Assert.That(fileDropList, Is.Empty);
                return;
            }
            if(OperatingSystem.IsLinux()) {
                var fileDropList = await ClipboardFile.GetList(new[] { "UnknownFormat0", "UnknownFormat1" }, getDataFunc("UnknownFormat0", filesLinux.Concat(incorrectItems)));
                Assert.That(fileDropList, Is.Empty);
                return;
            }
            throw new NotSupportedException($"OS: {Environment.OSVersion}");

        }

        [Test]
        public void SetFileDropList_Test() {
            string? outFormat = null;
            object? outObject = null;

            if(OperatingSystem.IsWindows()) {
                ClipboardFile.SetFileDropList((f, o) => { outFormat = f; outObject = o; }, outFilesWindows);
                Assert.That(outFormat, Is.EqualTo(ClipboardFile.Format.FileNames));
                Assert.That(outObject, Is.InstanceOf<IList<string>>());
                Assert.That((IList<string>)outObject!, Is.EquivalentTo(outFilesWindows));
                return;
            }

            if(OperatingSystem.IsLinux()) {
                ClipboardFile.SetFileDropList((f, o) => { outFormat = f; outObject = o; }, outFilesLinux);
                Assert.That(outFormat, Is.EqualTo(ClipboardFile.Format.XMateFileNames));
                Assert.That(outObject, Is.InstanceOf<byte[]>());

                var urlsBytes = System.Text.Encoding.UTF8.GetBytes(string.Join("\n", outFilesLinux));
                Assert.That((byte[])outObject!, Is.EquivalentTo(urlsBytes));
                return;
            }

            throw new NotSupportedException($"OS: {Environment.OSVersion}");
        }
    }
}
