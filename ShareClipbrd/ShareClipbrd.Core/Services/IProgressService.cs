namespace ShareClipbrd.Core.Services {
    public enum ProgressMode {
        Send,
        Receive
    };

    public interface IProgressService {
        IAsyncDisposable Begin(Int64 max, ProgressMode mode);
        void Tick(Int64 steps);
    }
}
