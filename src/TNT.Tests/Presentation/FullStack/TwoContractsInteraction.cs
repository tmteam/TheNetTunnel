using System.Linq;
using NUnit.Framework;
using TNT.Channel;
using TNT.Channel.Test;

namespace TNT.Tests.Presentation.FullStack
{
    [TestFixture]
    public class TwoContractsInteraction
    {
        [Test]
        public void SayCall_FromProxyToOrigin_OriginSayCalled()
        {
            var channelPair = TntTestHelper.CreateChannelPair();
            var proxyConnection = new ConnectionBuilder<ITestContract>().UseChannel(channelPair.CahnnelA).Buid();
            var originConnection = new ConnectionBuilder<TestContractImplementation>().UseChannel(channelPair.ChannelB).Buid();
            channelPair.Connect();

            const string sentMessage = "Hey you";
            proxyConnection.Contract.Say(sentMessage);
            var received =  originConnection.Contract.SaySCalled.SingleOrDefault();
            Assert.AreEqual(received, sentMessage);
        }
    }
    
}
