using System.Diagnostics;
using System.IO.Hashing;
using System.Net;
using System.Text;
using Makaretu.Dns;

namespace ShareClipbrd.Core.Services {
    public interface IAddressDiscoveryService {
        Task<IPEndPoint> Discover(string key);
        void Advertise(string key, int port);
    }

    public class AddressDiscoveryService : IAddressDiscoveryService {
        const string serviceName = "_shareclipbrd28CBA1._tcp";

        public AddressDiscoveryService() { }

        static string HashKey(string key) {
            var bytes = Encoding.ASCII.GetBytes(key);
            var hash = Crc32.Hash(bytes);
            var b64 = Convert.ToBase64String(hash);
            return b64;
        }

        public void Advertise(string key, int port) {
            var hashKey = HashKey(key);
            var service = new ServiceProfile(hashKey, serviceName, (ushort)port);
            Debug.WriteLine($"------- Advertise 0 {service.FullyQualifiedName}");
            var sd = new ServiceDiscovery();
            sd.Advertise(service);
        }

        public Task<IPEndPoint> Discover(string key) {
            var hashKey = HashKey(key);
            var tcs = new TaskCompletionSource<IPEndPoint>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            cts.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

            Debug.WriteLine($"------- dddd 0");
            var sd = new ServiceDiscovery();
            sd.ServiceDiscovered += (s, serviceName) => {
                Debug.WriteLine($"ServiceDiscovered {s}  {serviceName}");
            };

            sd.ServiceInstanceDiscovered += (s, e) => {
                Debug.WriteLine($"ServiceInstanceDiscovered {s} {e.ServiceInstanceName.Labels.FirstOrDefault()}");

                if(e.ServiceInstanceName.Labels.FirstOrDefault() == hashKey) {
                    var srvRecord = e.Message.AdditionalRecords.OfType<Makaretu.Dns.SRVRecord>().FirstOrDefault();
                    var aRecord = e.Message.AdditionalRecords.OfType<Makaretu.Dns.ARecord>().FirstOrDefault();
                    if(srvRecord != null && aRecord != null) {
                        var ipEndPoint = new IPEndPoint(aRecord.Address, srvRecord.Port);
                        Debug.WriteLine($"Discover client: {ipEndPoint}");
                        tcs.TrySetResult(ipEndPoint);
                    }
                }
            };
            sd.QueryUnicastServiceInstances(serviceName);

            Debug.WriteLine($"------- dddd 1");
            return tcs.Task;
        }
    }
}
