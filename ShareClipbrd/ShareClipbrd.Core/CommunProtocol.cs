namespace ShareClipbrd.Core {
    public class CommunProtocol {
        public const int ChunkSize = 81920;

        public const UInt16 Version = 0x4201;
        public const UInt16 SuccessVersion = 0x1955;
        public const UInt16 SuccessFormat = 0x1956;
        public const UInt16 SuccessSize = 0x1957;
        public const UInt16 SuccessData = 0x1958;
        public const UInt16 Error = 0x1982;
        public const UInt16 MoreData = 0x4315;
        public const UInt16 Finish = 0x4316;

        public const UInt16 SuccessFilename = 0x1975;

    }
}
