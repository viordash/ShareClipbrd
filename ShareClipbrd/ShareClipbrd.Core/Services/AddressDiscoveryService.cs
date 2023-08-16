using System.Diagnostics;
using System.IO.Hashing;
using System.Net;
using System.Text;
using Makaretu.Dns;

namespace ShareClipbrd.Core.Services {
    public interface IAddressDiscoveryService {
        Task<IPEndPoint> Discover(string id);
        void Advertise(string id, int port);
    }

    public class AddressDiscoveryService : IAddressDiscoveryService {
        const string serviceName = "_shareclipbrd28CBA1._tcp";

        public AddressDiscoveryService() { }

        static string HashId(string id) {
            var bytes = Encoding.ASCII.GetBytes(id);
            var hash = Crc32.Hash(bytes);
            var b64 = Convert.ToBase64String(hash);
            return b64;
        }

        public void Advertise(string id, int port) {
            var hashId = HashId(id);
            var service = new ServiceProfile(hashId, serviceName, (ushort)port);
            Debug.WriteLine($"------- Advertise 0 {service.FullyQualifiedName}");
            var sd = new ServiceDiscovery();
            sd.Advertise(service);
        }

        public async Task<IPEndPoint> Discover(string id) {
            var hashId = HashId(id);
            var tcs = new TaskCompletionSource<IPEndPoint>();
            using var sd = new ServiceDiscovery();

            sd.ServiceInstanceDiscovered += (s, e) => {
                Debug.WriteLine($"ServiceInstanceDiscovered {s} {e.ServiceInstanceName.Labels.FirstOrDefault()}");

                if(e.ServiceInstanceName.Labels.FirstOrDefault() == hashId) {
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

            var delayTask = Task.Run(async () => {
                await Task.Delay(2000);
                return await Task.FromException<IPEndPoint>(new OperationCanceledException());
            });
            var res = await Task.WhenAny(delayTask, tcs.Task).Unwrap();
            return res;
        }
    }
}
