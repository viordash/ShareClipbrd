using Makaretu.Dns;
using System.Diagnostics;
using System.IO.Hashing;
using System.Net;
using System.Text;

namespace ShareClipbrd.Core.Services {
    public interface IAddressDiscoveryService {
        Task<IPEndPoint> Discover(string id, List<IPAddress> badIpAdresses);
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
            Debug.WriteLine($"Advertise id:{id}, service:{service.FullyQualifiedName}");
            var sd = new ServiceDiscovery();
            sd.Advertise(service);
        }

        public async Task<IPEndPoint> Discover(string id, List<IPAddress> badIpAdresses) {
            Debug.WriteLine($"{DateTime.Now.TimeOfDay.TotalSeconds}: Discover id:{id}");
            var hashId = HashId(id);
            var tcs = new TaskCompletionSource<IPEndPoint>();

            using var timed_cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5000));
            using(timed_cts.Token.Register(() => {
                Debug.WriteLine($"{DateTime.Now.TimeOfDay.TotalSeconds}: Discover timeout");
                tcs.TrySetCanceled();
            })) {
                using var sd = new ServiceDiscovery();
                sd.ServiceInstanceDiscovered += (s, e) => {
                    Debug.WriteLine($"ServiceInstanceDiscovered {s} {e.ServiceInstanceName.Labels.FirstOrDefault()}, badIpAdresses:[{string.Join(", ", badIpAdresses)}]");

                    if(e.ServiceInstanceName.Labels.FirstOrDefault() == hashId) {
                        var srvRecord = e.Message.AdditionalRecords.OfType<Makaretu.Dns.SRVRecord>()
                            .FirstOrDefault();
                        var aRecord = e.Message.AdditionalRecords.OfType<Makaretu.Dns.ARecord>()
                            .Select(x => x.Address.MapToIPv6())
                            .Except(badIpAdresses)
                            .FirstOrDefault();
                        if(srvRecord != null && aRecord != null) {
                            var ipEndPoint = new IPEndPoint(aRecord, srvRecord.Port);
                            Debug.WriteLine($"{DateTime.Now.TimeOfDay.TotalSeconds}: Discover client: {ipEndPoint}");
                            tcs.TrySetResult(ipEndPoint);
                        }
                    }
                };
                sd.QueryUnicastServiceInstances(serviceName);

                var res = await tcs.Task;
                Debug.WriteLine($"{DateTime.Now.TimeOfDay.TotalSeconds}: Discover return res: {res}");
                return res;
            }
        }
    }
}
