namespace ShareClipbrd.Core.Services {
    public interface IDataTransferService {
        Task Send(ClipboardData data);
        void StartServer();
    }

    public class DataTransferService : IDataTransferService {

        public async Task Send(ClipboardData data) {
            //throw new NotImplementedException();

            await Task.Delay(5);
        }

        public void StartServer() {
            throw new NotImplementedException();
        }
    }
}
