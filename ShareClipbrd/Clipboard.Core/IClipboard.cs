namespace Clipboard.Core {
    internal interface IClipboard {
        Task<string[]> GetFormats();
        Task<bool> ContainsFileDropList();
        Task<bool> ContainsImage();

        Task<object?> GetData(string format);
    }
}
