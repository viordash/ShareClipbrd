using GuardNet;

namespace ShareClipbrd.Core.Services {
    public interface IDataTransferService {
        Task Send(ClipboardData data);
    }

    public class DataTransferService : IDataTransferService {
        readonly IDataClient dataClient;


        public DataTransferService(
            IDataServer dataServer,
            IDataClient dataClient
            ) {
            Guard.NotNull(dataServer, nameof(dataServer));
            Guard.NotNull(dataClient, nameof(dataClient));
            this.dataClient = dataClient;
        }

        public async Task Send(ClipboardData data) {
            await dataClient.Send(data);
        }
    }
}
