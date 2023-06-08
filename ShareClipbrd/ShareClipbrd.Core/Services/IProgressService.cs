namespace ShareClipbrd.Core.Services {
    public enum ProgressMode {
        None = 0,
        Send,
        Receive,
        Error
    };

    public interface IProgressService {
        IAsyncDisposable Begin(ProgressMode mode);
        void SetMaxTick(Int64 max);
        void Tick(Int64 steps);
        void SetMaxMinorTick(Int64 max);
        void MinorTick(Int64 steps);
    }
}
