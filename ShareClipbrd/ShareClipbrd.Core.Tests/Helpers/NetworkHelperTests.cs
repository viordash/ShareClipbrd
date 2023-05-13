using System;
using System.Net;
using System.Net.Sockets;
using ShareClipbrd.Core.Helpers;

namespace ShareClipbrd.Core.Tests.Helpers {
    public class NetworkHelperTests {
        [Test]
        public void ResolveHostName_IPv4_Test() {
            var adr = NetworkHelper.ResolveHostName("127.0.0.1:55542");
            Assert.That(adr.AddressFamily, Is.EqualTo(AddressFamily.InterNetwork));
            Assert.That(adr.Address, Is.EqualTo(IPAddress.Loopback));
            Assert.That(adr.Port, Is.EqualTo(55542));
        }

        [Test]
        public void ResolveHostName_IPv6_Test() {
            var adr = NetworkHelper.ResolveHostName("[::1]:51234");
            Assert.That(adr.AddressFamily, Is.EqualTo(AddressFamily.InterNetworkV6));
            Assert.That(adr.Address, Is.EqualTo(IPAddress.IPv6Loopback));
            Assert.That(adr.Port, Is.EqualTo(51234));

            adr = NetworkHelper.ResolveHostName("[fe80::9656:d028:8652:66b6]:51234");
            Assert.That(adr.AddressFamily, Is.EqualTo(AddressFamily.InterNetworkV6));
            Assert.That(adr.Address, Is.EqualTo(IPAddress.Parse("[fe80::9656:d028:8652:66b6]")));
            Assert.That(adr.Port, Is.EqualTo(51234));
        }

        [Test]
        public void ResolveHostName_Hostname_Test() {
            var adr = NetworkHelper.ResolveHostName("localhost:4219");
            Assert.That(adr.AddressFamily, Is.EqualTo(AddressFamily.InterNetwork).Or.EqualTo(AddressFamily.InterNetworkV6));
            Assert.That(adr.Address, Is.EqualTo(IPAddress.Loopback).Or.EqualTo(IPAddress.IPv6Loopback));
            Assert.That(adr.Port, Is.EqualTo(4219));
        }

        [Test]
        public void ResolveHostName_No_Port_Throws_ArgumentException() {
            var exception = Assert.Throws<ArgumentException>(() => NetworkHelper.ResolveHostName("localhost"));
            Assert.That(exception.Message, Is.EqualTo("Port not valid, hostname: localhost"));
            exception = Assert.Throws<ArgumentException>(() => NetworkHelper.ResolveHostName("127.0.0.1"));
            Assert.That(exception.Message, Is.EqualTo("Port not valid, hostname: 127.0.0.1"));
            exception = Assert.Throws<ArgumentException>(() => NetworkHelper.ResolveHostName("127.0.0.1:"));
            Assert.That(exception.Message, Is.EqualTo("Port not valid, hostname: 127.0.0.1:"));
            exception = Assert.Throws<ArgumentException>(() => NetworkHelper.ResolveHostName("[::1]"));
            Assert.That(exception.Message, Is.EqualTo("Port not valid, hostname: [::1]"));
            exception = Assert.Throws<ArgumentException>(() => NetworkHelper.ResolveHostName("[fe80::9656:d028:8652:66b6]"));
            Assert.That(exception.Message, Is.EqualTo("Port not valid, hostname: [fe80::9656:d028:8652:66b6]"));
            exception = Assert.Throws<ArgumentException>(() => NetworkHelper.ResolveHostName(""));
            Assert.That(exception.Message, Is.EqualTo("Port not valid, hostname: "));
        }

        [Test]
        public void ResolveHostName_Invalid_Hostname_Throws_SocketException() {
            var exception = Assert.Throws<SocketException>(() => NetworkHelper.ResolveHostName("AD551E59-DA2A-47E9-BD6D-1FB8C09C0845:1"));
        }
    }
}
