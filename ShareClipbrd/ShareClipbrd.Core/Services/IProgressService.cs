namespace ShareClipbrd.Core.Services {
    public interface IProgressService {
        IAsyncDisposable Begin(Int64 max);
        void Tick(Int64 steps);
    }
}
