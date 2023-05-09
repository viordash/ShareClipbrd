namespace ShareClipbrd.Core {
    public class ClipboardData {
        public Dictionary<string, byte[]> Formats { get; } = new();

        public void Add(string format, byte[] data) {
            Formats.Add(format, data);
        }
    }
}
