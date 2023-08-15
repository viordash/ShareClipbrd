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
            var service = new ServiceProfile(HashKey(key), serviceName, (ushort)port);
            Debug.WriteLine($"------- Advertise 0 {service.FullyQualifiedName}");
            var sd = new ServiceDiscovery();
            sd.Advertise(service);
        }

        public Task<IPEndPoint> Discover(string key) {
            var tcs = new TaskCompletionSource<IPEndPoint>();
            Debug.WriteLine($"------- dddd 0");
            var sd = new ServiceDiscovery();
            sd.ServiceDiscovered += (s, serviceName) => {
                Debug.WriteLine($"ServiceDiscovered {s}  {serviceName}");
            };

            sd.ServiceInstanceDiscovered += (s, e) => {
                Debug.WriteLine($"ServiceInstanceDiscovered {s}  {e}");
                tcs.TrySetResult(new IPEndPoint(IPAddress.Any, 0));
            };
            sd.QueryUnicastServiceInstances(serviceName);


            Debug.WriteLine($"------- dddd 1");
            return tcs.Task;
        }
    }
}
