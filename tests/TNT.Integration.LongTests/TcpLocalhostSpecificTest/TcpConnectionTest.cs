using System.Net;
using CommonTestTools;
using CommonTestTools.Contracts;
using NUnit.Framework;
using TNT;
using TNT.Api;
using TNT.Exceptions.Remote;
using TNT.Presentation.ReceiveDispatching;
using TNT.Tcp;

namespace Tnt.LongTests.TcpLocalhostSpecificTest
{
    [TestFixture]
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
        [Test]
        public void ClientDisconnected_ServerRaisesErrorMessage()
        {
            var failedOrigin = IntegrationTestsHelper
                .GetOriginBuilder()
                .UseSerializer(IntegrationTestsHelper.GetThrowsSerializationRuleFor<string>());

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (failedOrigin, IntegrationTestsHelper.GetProxyBuilder()))
            {
                var awaiter = new EventAwaiter<ClientDisconnectEventArgs<ITestContract, TcpChannel>>();
                tcpPair.Server.Disconnected += awaiter.EventRaised;

                //provokes exception at origin-side serialization
                try
                {
                    tcpPair.ProxyContract.Ask("some string");
                }
                catch (RemoteSerializationException){}

                var receivedArgs = awaiter.WaitOneOrDefault(500);
                //Client disconnected event is expected
                Assert.IsNotNull(receivedArgs);
                Assert.IsNotNull(receivedArgs.ErrorMessageOrNull);
                Assert.AreEqual(ErrorType.SerializationError, receivedArgs.ErrorMessageOrNull.ErrorType);
            }
        }
    }
}
