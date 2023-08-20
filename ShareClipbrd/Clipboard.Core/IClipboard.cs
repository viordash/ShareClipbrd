namespace Clipboard.Core {
    internal interface IClipboard {
        Task<string[]> GetFormatsAsync();
        Task<bool> ContainsFileDropList();
        Task<bool> ContainsImage();

        Task<object> GetData(string format);
    }
}
