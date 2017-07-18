using System.Linq;
using NUnit.Framework;
using TNT.Channel;
using TNT.Channel.Test;
using TNT.Light;
using TNT.Presentation;

namespace TNT.Tests.Presentation.FullStack
{
    [TestFixture]
    public class TwoContractsInteraction
    {
        [Test]
        public void SayCall_FromProxyToOrigin_OriginSayCalled()
        {
            var channelPair = TntTestHelper.CreateChannelPair();
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcherFactory<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Buid();

            var originConnection = ConnectionBuilder
                .UseContract<ITestContract,TestContractImplementation>()
                .UseReceiveDispatcherFactory<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Buid();

            channelPair.Connect();

            const string sentMessage = "Hey you";
            proxyConnection.Contract.Say(sentMessage);
            var received =  ((TestContractImplementation) originConnection.Contract).SaySCalled.SingleOrDefault();
            Assert.AreEqual(received, sentMessage);
        }
    }
    
}
