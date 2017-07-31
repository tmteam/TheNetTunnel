using System;
using NUnit.Framework;
using TNT.Tests;
using TNT.Tests.Presentation.Contracts;

namespace TNT.IntegrationTests.TcpLocalhost
{
    [TestFixture]
    public class TwoContractsInteraction
    {

        [Test]
        public void TcpClientConnectsToTcpServer()
        {
            using (var tcpPair = new TcpConnectionPair())
            {
                Assert.IsNotNull(tcpPair.OriginConnection);
            }
        }


        [TestCase("Hey you")]
        [TestCase("")]
        [TestCase(null)]
        public void ProxySayCall_NoThreadDispatcher_OriginSayCalled(string sentMessage)
        {
            using (var tcpPair = new TcpConnectionPair())
            {
                EventAwaiter<string> callAwaiter = new EventAwaiter<string>();
                tcpPair.OriginContract.SayMethodWasCalled += callAwaiter.EventRaised;

                tcpPair.ProxyConnection.Contract.Say(sentMessage);
                var received = callAwaiter.WaitOneOrDefault(500);

                Assert.AreEqual(sentMessage, received);
            }
        }

        [TestCase("Hey you", 12, 24)]
        [TestCase("", 234, 0)]
        [TestCase(null, 0, long.MaxValue)]
        public void ProxyAskCall_ReturnsCorrectValue(string s, int i, long l)
        {
            using (var tcpPair = new TcpConnectionPair())
            {
                var contract = tcpPair.OriginContract;
                var func = new Func<string, int, long, string>((s1, i2, l3) => s1 + i2.ToString() + l3.ToString());
                //set reaction on AskSilCall
                contract.WhenAskSILCalledCall(func);

                var proxyResult = tcpPair.ProxyConnection.Contract.Ask(s, i, l);
                // expected value is:
                var originResult = func(s, i, l);

                Assert.AreEqual(originResult, proxyResult);
            }
        }

        [TestCase("Hey you")]
        [TestCase("")]
        [TestCase(null)]
        public void ProxyAskCall_ReturnsSettedValue(string returnedValue)
        {
            using (var tcpPair = new TcpConnectionPair())
            {
                tcpPair.ProxyConnection.Contract.OnAskS += (arg) => arg;
                var proxyResult = tcpPair.OriginContract.OnAskS(returnedValue);
                Assert.AreEqual(returnedValue, proxyResult);
            }
        }

        [Test]
        public void ConveyourDispatcher_NetworkDeadlockNotHappens()
        {
            using (var tcpPair = new TcpConnectionPair())
            {
                tcpPair.OriginContract.OnAsk += tcpPair.OriginContract.Ask;
                var task = TestTools.AssertNotBlocks(tcpPair.OriginContract.OnAsk);
                Assert.AreEqual(TestContractMock.AskReturns, task.Result);
            }
        }
    }
}
