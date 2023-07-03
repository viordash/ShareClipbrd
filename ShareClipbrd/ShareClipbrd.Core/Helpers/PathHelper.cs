namespace ShareClipbrd.Core.Helpers {
    public class PathHelper {
        public static bool IsAbsolute(string path) {
            try {
                return Path.IsPathRooted(path) && (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(path)) || !string.IsNullOrWhiteSpace(Path.GetFileName(path)));
            } catch {
                return false;
            }
        }
    }
}
