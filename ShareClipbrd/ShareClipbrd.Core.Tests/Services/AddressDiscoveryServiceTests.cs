using ShareClipbrd.Core.Services;
using System.Windows;

namespace ShareClipbrd.Core.Tests.Services {
    public class AddressDiscoveryServiceTests {

        class TestableAddressDiscoveryService : AddressDiscoveryService {
            public string PublicMorozov_Get_selfIdProperty() {
                return selfIdProperty;
            }
            public bool PublicMorozov_HasExternalSign(IEnumerable<Makaretu.Dns.TXTRecord> txtRecords) {
                return HasExternalSign(txtRecords);
            }
        }

        TestableAddressDiscoveryService testable;

        [SetUp]
        public void Setup() {
            testable = new();
        }

        [TearDown]
        public void Teardown() {
        }


        [Test]
        public void HasExternalSign_Returns_False_When_Records_Empty() {
            var records = new List<Makaretu.Dns.TXTRecord>();
            Assert.That(testable.PublicMorozov_HasExternalSign(records), Is.False);
        }

        [Test]
        public void HasExternalSign_Returns_False_When_Key_Is_Incorrect_Or_Id_Is_External() {
            var records = new List<Makaretu.Dns.TXTRecord>() { new Makaretu.Dns.TXTRecord() };
            records[0].Strings.Add(null);
            records[0].Strings.Add("");
            records[0].Strings.Add("selfId");
            records[0].Strings.Add("selfIdEEEE=");
            records[0].Strings.Add("EEEselfId=" + testable.PublicMorozov_Get_selfIdProperty());
            records[0].Strings.Add("SELFId=" + testable.PublicMorozov_Get_selfIdProperty());

            Assert.That(testable.PublicMorozov_HasExternalSign(records), Is.False);
        }

        [Test]
        public void HasExternalSign_Returns_False_When_Is_SelfId() {
            var records = new List<Makaretu.Dns.TXTRecord>() { new Makaretu.Dns.TXTRecord() };
            records[0].Strings.Add("selfId=" + testable.PublicMorozov_Get_selfIdProperty());
            Assert.That(testable.PublicMorozov_HasExternalSign(records), Is.False);
        }

        [Test]
        public void HasExternalSign_Returns_False_When_SelfId_Is_Not_Guid() {
            var records = new List<Makaretu.Dns.TXTRecord>() { new Makaretu.Dns.TXTRecord() };
            var externalId = Guid.NewGuid().ToString();
            var externalId_With_NoGuid_Format = externalId.Replace('-', '_');
            records[0].Strings.Add("selfId=" + externalId_With_NoGuid_Format);
            Assert.That(testable.PublicMorozov_HasExternalSign(records), Is.False);

            records[0].Strings.Add("selfId=" + externalId);
            Assert.That(testable.PublicMorozov_HasExternalSign(records), Is.True);
        }

        [Test]
        public void HasExternalSign_Returns_True_When_Records_Contains_External_Id() {
            var records = new List<Makaretu.Dns.TXTRecord>() { new Makaretu.Dns.TXTRecord() };
            records[0].Strings.Add("selfId=" + Guid.NewGuid().ToString());

            Assert.That(testable.PublicMorozov_HasExternalSign(records), Is.True);
        }

    }
}
