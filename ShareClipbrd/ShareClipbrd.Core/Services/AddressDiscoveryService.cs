﻿using Makaretu.Dns;
using ShareClipbrd.Core.Helpers;
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
        protected const string selfIdPropertyName = "selfId";
        protected readonly string selfIdProperty = Guid.NewGuid().ToString();
        readonly Lazy<MulticastService> mdns;

        public AddressDiscoveryService() {
            var sessionDir =
            mdns = new Lazy<MulticastService>(() => {
                var m = new MulticastService();
                m.Start();
                return m;
            });
        }

        static string HashId(string id) {
            var bytes = Encoding.ASCII.GetBytes(id);
            var hash = Crc32.Hash(bytes);
            var b64 = Convert.ToBase64String(hash);
            return b64;
        }

        public void Advertise(string id, int port) {
            var hashId = HashId(id);
            var service = new ServiceProfile(hashId, serviceName, (ushort)port);
            Debug.WriteLine($"Advertise id:{id}, port:{port}, service:{service.FullyQualifiedName}");
            var sd = new ServiceDiscovery(mdns.Value);
            service.AddProperty(selfIdPropertyName, selfIdProperty);

            sd.Advertise(service);
        }

        protected bool HasExternalSign(IEnumerable<Makaretu.Dns.TXTRecord> txtRecords) {
            return txtRecords
                    .SelectMany(x => x.Strings)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => x.Split('='))
                    .Where(x => x.Length == 2)
                    .Where(x => x.First().Equals(selfIdPropertyName) && Guid.TryParse(x.Last(), out _) && !x.Last().Equals(selfIdProperty))
                    .Any();
        }

        public async Task<IPEndPoint> Discover(string id, List<IPAddress> badIpAdresses) {
            var hashId = HashId(id);
            Debug.WriteLine($"[{Environment.CurrentManagedThreadId}] Discover id:{id} ({hashId})");
            var tcs = new TaskCompletionSource<IPEndPoint>();

            using var timed_cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5000));
            using(timed_cts.Token.Register(() => {
                Debug.WriteLine($"Discover timeout");
                tcs.TrySetCanceled();
            })) {
                using var sd = new ServiceDiscovery(mdns.Value);
                sd.ServiceInstanceDiscovered += (s, e) => {
                    Debug.WriteLine($"[{Environment.CurrentManagedThreadId}] ServiceInstanceDiscovered {s} {e.ServiceInstanceName.Labels.FirstOrDefault()}, badIpAdresses:[{string.Join(", ", badIpAdresses)}]");

                    if(e.ServiceInstanceName.Labels.FirstOrDefault() == hashId) {
                        var srvRecord = e.Message.AdditionalRecords.OfType<Makaretu.Dns.SRVRecord>()
                            .FirstOrDefault();
                        var aRecord = e.Message.AdditionalRecords.OfType<Makaretu.Dns.ARecord>()
                            .Select(x => x.Address)
                            .Except(badIpAdresses, new IPAddressEqualityComparer())
                            .FirstOrDefault();

                        var externalRecord = HasExternalSign(e.Message.AdditionalRecords.OfType<Makaretu.Dns.TXTRecord>());

                        if(srvRecord != null && aRecord != null && externalRecord) {
                            var ipEndPoint = new IPEndPoint(aRecord, srvRecord.Port);
                            Debug.WriteLine($"[{Environment.CurrentManagedThreadId}] Discover client: {ipEndPoint}");
                            tcs.TrySetResult(ipEndPoint);
                        } else {
                            Debug.WriteLine($"[{Environment.CurrentManagedThreadId}] Discover wrong client, ext:{externalRecord}, srv:'{srvRecord}', a:'{aRecord}'");
                        }
                    }
                };
                sd.QueryUnicastServiceInstances(serviceName);

                var res = await tcs.Task;
                Debug.WriteLine($"[{Environment.CurrentManagedThreadId}] Discover return res: {res}");
                return res;
            }
        }
    }
}
