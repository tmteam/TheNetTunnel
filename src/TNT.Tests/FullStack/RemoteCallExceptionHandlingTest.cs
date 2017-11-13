using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TNT.Exceptions.Remote;
using TNT.Presentation.ReceiveDispatching;
using TNT.Testing;
using TNT.Tests.Contracts;

namespace TNT.Tests.FullStack
{
    [TestFixture]
    public class RemoteCallExceptionHandlingTest
    {
        [Test]
        public void Origin_AskExceptioanlCallback_Throws()
        {
            var channelPair = TntTestHelper.CreateChannelPair();

            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseContractInitalization((c, _) =>
                {
                    c.OnAsk += () => { throw new InvalidOperationException(); };
                })
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<ITestContract, TestContractMock>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();
            TestTools.AssertThrows_AndNotBlocks_AndContainsInfo<RemoteUnhandledException>(() => originConnection.Contract.OnAsk());
        }

        [Test]
        public void Proxy_AskWithException_Throws()
        {
            var channelPair = TntTestHelper.CreateChannelPair();
            var proxyConnection = TntBuilder
                .UseContract<IExceptionalContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<IExceptionalContract, ExceptionalContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();
            TestTools.AssertThrows_AndNotBlocks_AndContainsInfo<RemoteUnhandledException>(() => proxyConnection.Contract.Ask());

        }


        [Test]
        public void Proxy_AskMissingCord_Throws()
        {
            var channelPair = TntTestHelper.CreateChannelPair();
            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<IEmptyContract, EmptyContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();

            TestTools.AssertThrows_AndNotBlocks_AndContainsInfo<RemoteContractImplementationException>(() => proxyConnection.Contract.Ask());
        }


        [Test]
        public void Proxy_SayMissingCord_NotThrows()
        {
            var channelPair = TntTestHelper.CreateChannelPair();
            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<IEmptyContract, EmptyContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();
            TestTools.AssertNotBlocks(proxyConnection.Contract.Say);
        }
        [Test]
        public void Origin_SayMissingCord_NotThrows()
        {
            var channelPair = TntTestHelper.CreateChannelPair();

            var proxyConnection = TntBuilder
                .UseContract<IEmptyContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<ITestContract, TestContractMock>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();
            TestTools.AssertNotBlocks(originConnection.Contract.OnSay);
        }
        [Test]
        public void Proxy_SayWithException_CallNotThrows()
        {
            var channelPair = TntTestHelper.CreateChannelPair();
            var proxyConnection = TntBuilder
                .UseContract<IExceptionalContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<IExceptionalContract, ExceptionalContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();
            TestTools.AssertNotBlocks(proxyConnection.Contract.Say);
        }

        [Test]
        public void Origin_SayExceptioanlCallback_NotThrows()
        {
            var channelPair = TntTestHelper.CreateChannelPair();

            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseContractInitalization((c, _) =>
                {
                    c.OnSay += () => { throw new InvalidOperationException(); };
                    c.OnAsk += () => { throw new InvalidOperationException(); };
                })
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<ITestContract, TestContractMock>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();
            TestTools.AssertNotBlocks(originConnection.Contract.OnSay);
        }



        [Test]
        public void Origin_AsksNotImplemented_returnsDefault()
        {
            var channelPair = TntTestHelper.CreateChannelPair();

            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = TntBuilder
                .UseContract<ITestContract, TestContractMock>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();

            var answer = TestTools.AssertNotBlocks(originConnection.Contract.OnAsk).Result;
            Assert.AreEqual(default(int), answer);
        }
    }
}
