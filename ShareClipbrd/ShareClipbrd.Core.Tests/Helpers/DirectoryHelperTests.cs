using ShareClipbrd.Core.Helpers;

namespace ShareClipbrd.Core.Tests.Helpers {
    public class DirectoryHelperTests {
        string testsPath;

        [SetUp]
        public void Setup() {
            testsPath = Path.Combine(Path.GetTempPath(), "tests_DirectoryHelperTests");
            if(Directory.Exists(testsPath)) {
                Directory.Delete(testsPath, true);
            }
            Directory.CreateDirectory(testsPath);
        }

        [TearDown]
        public void Teardown() {
            Directory.Delete(testsPath, true);
        }

        [Test]
        public void RecursiveGetFiles_Test() {
            var directory0 = Path.Combine(testsPath, "directory0");
            Directory.CreateDirectory(directory0);

            var directory0_Child0 = Path.Combine(directory0, "directory0_Child0");
            Directory.CreateDirectory(directory0_Child0);
            var filename = Path.Combine(directory0_Child0, "filename0");
            File.WriteAllText(filename, "0");

            var directory0_Child1 = Path.Combine(directory0, "directory0_Child1");
            Directory.CreateDirectory(directory0_Child1);

            var directory0_Child1_Child0 = Path.Combine(directory0_Child1, "directory0_Child1_Child0");
            Directory.CreateDirectory(directory0_Child1_Child0);
            filename = Path.Combine(directory0_Child1_Child0, "filename1");
            File.WriteAllText(filename, "1");

            var directory1 = Path.Combine(testsPath, "directory1");
            Directory.CreateDirectory(directory1);

            var directory2 = Path.Combine(testsPath, "directory2");
            Directory.CreateDirectory(directory2);
            for(int i = 0; i < 10; i++) {
                filename = Path.Combine(directory2, i.ToString());
                File.WriteAllText(filename, $"3 {i}");
            }

            var files = DirectoryHelper.RecursiveGetFiles(testsPath);
            Assert.That(files, Has.Count.EqualTo(12));
            var relativeFiles = files
                .Select(x => Path.GetRelativePath(testsPath, x).Replace('\\', Path.AltDirectorySeparatorChar))
                .Order();
            Assert.That(relativeFiles, Is.EquivalentTo(new[] { "directory0/directory0_Child0/filename0", "directory0/directory0_Child1/directory0_Child1_Child0/filename1",
                        "directory2/0", "directory2/1", "directory2/2", "directory2/3", "directory2/4", "directory2/5", "directory2/6", "directory2/7", "directory2/8",
                        "directory2/9"}));


        }

        [Test]
        public void RecursiveGetEmptyFolders_Test() {
            var directory0 = Path.Combine(testsPath, "directory0");
            Directory.CreateDirectory(directory0);

            var directory0_Child0 = Path.Combine(directory0, "directory0_Child0");
            Directory.CreateDirectory(directory0_Child0);

            var directory0_Child1 = Path.Combine(directory0, "directory0_Child1");
            Directory.CreateDirectory(directory0_Child1);

            var directory0_Child1_Child0 = Path.Combine(directory0_Child1, "directory0_Child1_Child0");
            Directory.CreateDirectory(directory0_Child1_Child0);

            var directory1 = Path.Combine(testsPath, "directory1");
            Directory.CreateDirectory(directory1);

            var directory2 = Path.Combine(testsPath, "directory2");
            Directory.CreateDirectory(directory2);
            for(int i = 0; i < 10; i++) {
                var dir = Path.Combine(directory2, $"directory2_Child{i}");
                Directory.CreateDirectory(dir);
            }

            var filename1 = Path.Combine(directory0_Child1_Child0, Path.GetFileName(Path.GetTempFileName()));
            File.WriteAllText(filename1, "directory0_Child1_Child0 filename1");

            var filename2 = Path.Combine(Path.Combine(directory2, $"directory2_Child3"), Path.GetFileName(Path.GetTempFileName()));
            File.WriteAllText(filename2, "directory2_Child3 filename2");

            var emptyFolders = DirectoryHelper.RecursiveGetEmptyFolders(testsPath);
            Assert.That(emptyFolders, Has.Count.EqualTo(11));
            var relativeEmptyFolders = emptyFolders
                .Select(x => Path.GetRelativePath(testsPath, x).Replace('\\', Path.AltDirectorySeparatorChar))
                .Order();

            Assert.That(relativeEmptyFolders, Is.EquivalentTo(new[] { "directory0/directory0_Child0", "directory1", "directory2/directory2_Child0", "directory2/directory2_Child1",
                    "directory2/directory2_Child2", "directory2/directory2_Child4", "directory2/directory2_Child5", "directory2/directory2_Child6", "directory2/directory2_Child7",
                    "directory2/directory2_Child8", "directory2/directory2_Child9" }));


            emptyFolders = DirectoryHelper.RecursiveGetEmptyFolders(directory0_Child1);
            Assert.That(emptyFolders, Is.Empty);

            emptyFolders = DirectoryHelper.RecursiveGetEmptyFolders(directory1);
            Assert.That(emptyFolders, Is.Empty);
        }


    }
}
