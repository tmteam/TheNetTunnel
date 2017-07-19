using System.Linq;
using System.Threading.Tasks;
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
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Buid();

            var originConnection = ConnectionBuilder
                .UseContract<ITestContract,TestContractImplementation>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Buid();

            channelPair.ConnectAndStartReceiving();

            const string sentMessage = "Hey you";
            proxyConnection.Contract.Say(sentMessage);
            var received =  ((TestContractImplementation) originConnection.Contract).SaySCalled.SingleOrDefault();
            Assert.AreEqual(received, sentMessage);
        }


        [Test]
        public void NetworkDeadlockCheck()
        {
            var channelPair = TntTestHelper.CreateChannelPair();
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                //On income ask request, do rpc ask call. 
                //It can provoke an networkDeadlock
                .UseContractInitalization((c,_)=> c.OnAsk+=c.Ask)
                .UseChannel(channelPair.CahnnelA)
                .Buid();

            var originConnection = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
                .UseChannel(channelPair.ChannelB)
                .Buid();

            channelPair.ConnectAndStartReceiving();

            var tsk = Task.Run(originConnection.Contract.OnAsk);
            tsk.Wait(1000);
            Assert.IsTrue(tsk.IsCompleted);
            Assert.AreEqual(TestContractImplementation.AskReturns,   tsk.Result);
        }
    }
    
}
