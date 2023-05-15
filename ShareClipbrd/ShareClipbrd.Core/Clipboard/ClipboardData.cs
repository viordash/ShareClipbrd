namespace ShareClipbrd.Core.Clipboard {
    public record ClipboardItem {
        public string Format { get; set; }
        public Stream Data { get; set; }
        public ClipboardItem(string format, Stream data) {
            Format = format;
            Data = data;
        }
    }
    public class ClipboardData {
        public List<ClipboardItem> Formats { get; } = new();

        public void Add(string format, Stream data) {
            Formats.Add(new ClipboardItem(format, data));
        }
    }
}
