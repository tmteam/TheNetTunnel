using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TNT.Channel;
using TNT.Channel.Test;
using TNT.Exceptions;
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

            var tsk = Task.Run(originConnection.Contract.OnAsk);
            tsk.Wait(1000);
            Assert.IsTrue(tsk.IsCompleted);
            Assert.AreEqual(TestContractImplementation.AskReturns,   tsk.Result);
        }

        [Test]
        public void NoThreadDispatcher_NetworkDeadlockThrows()
        {
            Exception wereRaised = null;
            var channelPair = TntTestHelper.CreateChannelPair();
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseContractInitalization((c, _) => c.OnAsk += 
                ()=> {
                    try
                    {
                        var result = c.Ask();
                        return result;
                    }
                    catch (NetworkDeadLockException e)
                    {
                        wereRaised = e;
                        return 0;
                    }
                })
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();

            TestTools.AssertNotBlocks(originConnection.Contract.OnAsk);
            Assert.IsInstanceOf<NetworkDeadLockException>(wereRaised,"networkDeadLock was not raised");
        }
    }
    
}
