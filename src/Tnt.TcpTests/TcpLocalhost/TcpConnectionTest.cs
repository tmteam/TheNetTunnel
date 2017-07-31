using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using NUnit.Framework;
using TNT.Api;
using TNT.Presentation.ReceiveDispatching;
using TNT.Tcp;
using TNT.Tests.Presentation.Contracts;

namespace TNT.IntegrationTests.TcpLocalhost
{
    [TestFixture()]
    public class TcpConnectionTest
    {
        [Test]
        public void ServerCreatedServerIsNotListening()
        {
            var server = TntBuilder
              .UseContract<ITestContract, TestContractMock>()
              .UseReceiveDispatcher<NotThreadDispatcher>()
              .CreateTcpServer(IPAddress.Loopback, 12345);
            Assert.IsFalse(server.IsListening);
        }
        [Test]
        public void TcpClientConnectsToTcpServer()
        {
            using (var tcpPair = new TcpConnectionPair())
            {
                tcpPair.AssertPairIsConnected();
            }
        }

        [Test]
        public void TcpClientDisconnectFromTcpServer()
        {
            using (var tcpPair = new TcpConnectionPair())
            {
                tcpPair.Disconnect();
                tcpPair.AssertPairIsDisconnected();
            }
        }

        [Test]
        public void TcpClientDisconnectFromTcpServerTwice()
        {
            using (var tcpPair = new TcpConnectionPair())
            {
                tcpPair.Disconnect();
                Assert.DoesNotThrow(() => tcpPair.Disconnect());
            }

        }
    }
}
