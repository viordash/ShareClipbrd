namespace ShareClipbrd.Core.Helpers {
    public class DirectoryHelper {
        static void GetFiles(string targetDirectory, List<string> files) {
            var fileEntries = Directory.GetFiles(targetDirectory);
            files.AddRange(fileEntries);

            var subdirectories = Directory.GetDirectories(targetDirectory);
            foreach(string subdirectory in subdirectories) {
                GetFiles(subdirectory, files);
            }
        }

        public static List<string> RecursiveGetFiles(string targetDirectory) {
            var files = new List<string>();
            GetFiles(targetDirectory, files);
            return files;
        }

        static void GeFoldersCore(string targetDirectory, IList<string> folders) {
            var files = Directory.GetFiles(targetDirectory);

            var subdirectories = Directory.GetDirectories(targetDirectory);
            foreach(string subdirectory in subdirectories) {
                GeFoldersCore(subdirectory, folders);
            }
            if(!files.Any() && !subdirectories.Any()) {
                folders.Add(targetDirectory);
            }
        }

        static void GeFolders(string targetDirectory, IList<string> folders) {
            var subdirectories = Directory.GetDirectories(targetDirectory);
            foreach(string subdirectory in subdirectories) {
                GeFoldersCore(subdirectory, folders);
            }
        }

        public static List<string> RecursiveGetEmptyFolders(string targetDirectory) {
            var folders = new List<string>();
            GeFolders(targetDirectory, folders);
            return folders;
        }

        public static List<string> GetDirectoriesAndFiles(string targetDirectory) {
            var entries = new List<string>();

            var files = Directory.GetFiles(targetDirectory);
            entries.AddRange(files);

            var directories = Directory.GetDirectories(targetDirectory);
            entries.AddRange(directories);
            return entries;
        }
    }
}
