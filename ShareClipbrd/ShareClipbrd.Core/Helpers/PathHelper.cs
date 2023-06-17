namespace ShareClipbrd.Core.Helpers {
    public class PathHelper {
        public static bool IsAbsolute(string path) {
            try {
                return Path.IsPathRooted(path) && !string.IsNullOrEmpty(Path.GetDirectoryName(path));
            } catch {
                return false;
            }
        }
    }
}
