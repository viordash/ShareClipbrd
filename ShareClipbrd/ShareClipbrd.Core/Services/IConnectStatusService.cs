﻿namespace ShareClipbrd.Core.Services {
    public interface IConnectStatusService {
        void Online();
        void Offline();
        void ClientOnline();
        void ClientOffline();
    }
}
