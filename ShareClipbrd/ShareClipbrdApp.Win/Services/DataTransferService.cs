using System;
using System.Threading.Tasks;
using ShareClipbrd.Core;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Win.Services {
    internal class DataTransferService : IDataTransferService {
        public Task<ClipboardData> Receive() {
            throw new NotImplementedException();
        }

        public async Task Send(ClipboardData data) {
            //throw new NotImplementedException();

            await Task.Delay(5);
        }
    }
}
