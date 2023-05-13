namespace ShareClipbrd.Core.Services {
    public interface IClipboardService {
        ClipboardData GetCurrentData();

        bool SupportedFormat(string format);
        bool SupportedDataSize(Int32 size);

        void SetClipboardData(ClipboardData data);
    }
}
