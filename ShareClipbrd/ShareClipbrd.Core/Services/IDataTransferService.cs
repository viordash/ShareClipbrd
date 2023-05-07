namespace ShareClipbrd.Core.Services {
    public interface IDataTransferService {
        Task Send(ClipboardData data);
        Task<ClipboardData> Receive();
    }
}
