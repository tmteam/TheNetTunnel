using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TNT.Exceptions;
using TNT.Light;
using TNT.Presentation;
using TNT.Testing;

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
                .Build();

            var originConnection = ConnectionBuilder
                .UseContract<ITestContract,TestContractImplementation>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();

            const string sentMessage = "Hey you";
            proxyConnection.Contract.Say(sentMessage);
            var received =  ((TestContractImplementation) originConnection.Contract).SaySCalled.SingleOrDefault();
            Assert.AreEqual(received, sentMessage);
        }


        [Test]
        public void FIFODispatcher_NetworkDeadlockNotHappens()
        {
            var channelPair = TntTestHelper.CreateChannelPair();
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                //On income ask request, calling rpc ask. 
                //It can provoke an networkDeadlock 
                .UseContractInitalization((c,_)=> c.OnAsk+=c.Ask)
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();

            var task = TestTools.AssertNotBlocks(originConnection.Contract.OnAsk);
            Assert.AreEqual(TestContractImplementation.AskReturns, task.Result);
        }

        //[Test]
        //public void NoThreadDispatcher_NetworkDeadlockThrows()
        //{
        //    var channelPair = TntTestHelper.CreateChannelPair();
        //    var proxyConnection = ConnectionBuilder
        //        .UseContract<ITestContract>()

        //        //On income ask request, calling rpc ask. 
        //        //It can provoke an networkDeadlock 
        //        .UseContractInitalization((c, _) => c.OnAsk += c.Ask)
        //        .UseChannel(channelPair.CahnnelA)
        //        .Build();

        //    var originConnection = ConnectionBuilder
        //        .UseContract<ITestContract, TestContractImplementation>()
        //        .UseChannel(channelPair.ChannelB)
        //        .Build();

        //    channelPair.ConnectAndStartReceiving();

        //    var result = TestTools.AssertNotBlocks(originConnection.Contract.OnAsk);
        //    Assert.AreEqual(TestContractImplementation.AskReturns, result);
        //}
    }
    
}
