namespace ShareClipbrd.Core.Clipboard {
    public class ClipboardData {
        public Dictionary<string, Stream> Formats { get; } = new();

        public void Add(string format, Stream data) {
            Formats.Add(format, data);
        }
    }
}
