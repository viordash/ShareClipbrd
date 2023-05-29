namespace ShareClipbrd.Core.Services {
    public enum ProgressMode {
        Send,
        Receive
    };

    public interface IProgressService {
        IAsyncDisposable Begin(ProgressMode mode);
        void SetMaxTick(Int64 max);
        void Tick(Int64 steps);
    }
}
