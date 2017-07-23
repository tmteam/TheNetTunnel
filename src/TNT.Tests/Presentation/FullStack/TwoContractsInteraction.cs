using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TNT.Api;
using TNT.Exceptions;
using TNT.Presentation;
using TNT.Presentation.ReceiveDispatching;
using TNT.Testing;
using TNT.Tests.Presentation.Contracts;

namespace TNT.Tests.Presentation.FullStack
{
    [TestFixture]
    public class TwoContractsInteraction
    {
        [TestCase("Hey you")]
        [TestCase("")]
        [TestCase(null)]
        public void ProxySayCall_NoThreadDispatcher_OriginSayCalled(string sentMessage)
        {
            var channelPair = TntTestHelper.CreateThreadlessChannelPair();
            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<ITestContract,TestContractMock>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();

            proxyConnection.Contract.Say(sentMessage);
            var received =  ((TestContractMock) originConnection.Contract).SaySCalled.Single();
            Assert.AreEqual(sentMessage, received);
        }
       
        
        [TestCase("Hey you",12,24)]
        [TestCase("",234,0)]
        [TestCase(null,0,long.MaxValue)]
        public void ProxyAskCall_ReturnsCorrectValue(string s, int i, long l)
        {
            var func = new Func<string,int,long,string>((s1, i2, l3) =>
            {
               return s1 + i2.ToString() + l3.ToString();
            });


            var channelPair = TntTestHelper.CreateChannelPair();

            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<ConveyorDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<ITestContract, TestContractMock>()
                .UseContractInitalization((c,_)=> ((TestContractMock)c).WhenAskSILCalledCall(func))
                .UseReceiveDispatcher<ConveyorDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();

            var proxyResult = proxyConnection.Contract.Ask(s,i,l);
            var originResult = func(s, i, l);
            Assert.AreEqual(originResult, proxyResult);
        }

        public void ProxyAskCall_ReturnsSettedValue(string returnedValue)
        {
            var func = new Func<string, int, long, string>((s1, i2, l3) => s1 + i2.ToString() + l3.ToString());

            var channelPair = TntTestHelper.CreateChannelPair();

            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<ConveyorDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<ITestContract, TestContractMock>()
                .UseContractInitalization((c, _) => c.OnAskS += (arg)=>arg)
                .UseReceiveDispatcher<ConveyorDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();

            var proxyResult = proxyConnection.Contract.Ask(returnedValue);
            Assert.AreEqual(returnedValue, proxyResult);
        }
        [Test]
        public void ConveyourDispatcher_NetworkDeadlockNotHappens()
        {
            var channelPair = TntTestHelper.CreateChannelPair();
            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                //On income ask request, calling rpc ask. 
                //It can provoke an networkDeadlock 
                .UseContractInitalization((c,_)=> c.OnAsk+=c.Ask)
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<ITestContract, TestContractMock>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();

            var task = TestTools.AssertNotBlocks(originConnection.Contract.OnAsk);
            Assert.AreEqual(TestContractMock.AskReturns, task.Result);
        }
    }
}
