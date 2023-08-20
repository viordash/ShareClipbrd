namespace Clipboard.Core {
    internal interface IClipboard {
        Task<string[]> GetFormats();
        Task<bool> ContainsFileDropList();
        Task<bool> ContainsImage();

        Task<object?> GetData(string format);

        Task Clear();
        Task SetDataObject(object data);
        Task SetFileDropList(IList<string> files);
        Task SetAudio(byte[] audioBytes);
    }
}
