using System;
using System.Runtime.InteropServices.ComTypes;
using ShareClipbrd.Core.Services;

namespace ShareClipbrdApp.Win.Services {
    internal class DataTransferService : IDataTransferService {
        public IDataObject Receive() {
            throw new NotImplementedException();
        }

        public void Send(IDataObject dataObject) {
            throw new NotImplementedException();
        }
    }
}
