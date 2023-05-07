using System.Runtime.InteropServices.ComTypes;

namespace ShareClipbrd.Core.Services {
    public interface IDataTransferService {
        void Send(IDataObject dataObject);
        IDataObject Receive();
    }
}
