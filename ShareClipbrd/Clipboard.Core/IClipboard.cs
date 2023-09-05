namespace Clipboard.Core {
    internal interface IClipboard {
        Task<string[]> GetFormats();
        Task<bool> ContainsFileDropList();

        Task<object?> GetData(string format);

        Task Clear();
        Task SetDataObject(ClipboardData data);
        Task SetFileDropList(IList<string> files);
    }
}
