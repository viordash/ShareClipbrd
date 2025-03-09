namespace ShareClipbrd.Core.Services {
    public interface ITimeService {
        public TimeSpan DataClientPingPeriod { get; }
        public TimeSpan DataClientTimeout { get; }
    }

    public class TimeService : ITimeService {
        public TimeSpan DataClientPingPeriod {
            get {
                return TimeSpan.FromSeconds(10);
            }
        }
        public TimeSpan DataClientTimeout {
            get {
                return TimeSpan.FromSeconds(10);
            }
        }
    }
}
