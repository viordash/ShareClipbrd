namespace ShareClipbrd.Core.Services {
    public interface ITimeService {
        public TimeSpan DataClientPollTime { get; }
    }

    public class TimeService : ITimeService {
        public TimeSpan DataClientPollTime {
            get {
                return TimeSpan.FromSeconds(5);
            }
        }
    }
}
