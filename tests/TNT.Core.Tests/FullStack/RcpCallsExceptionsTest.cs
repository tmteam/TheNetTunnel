using System;
using CommonTestTools;
using CommonTestTools.Contracts;
using NUnit.Framework;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;
using TNT.Presentation.ReceiveDispatching;
using TNT.Testing;

namespace TNT.Core.Tests.FullStack
{
    [TestFixture]
    public class RcpCallsExceptionsTest
    {
        [Test]
        public void ProxyConnectionIsNotEstablishedYet_SayCallThrows()
        {
            var channel = new TestChannel();
            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channel)
                .Build();

            TestTools.AssertThrowsAndNotBlocks<ConnectionIsNotEstablishedYet>(() => proxyConnection.Contract.Say());
        }


        [Test]
        public void ProxyConnectionIsNotEstablishedYet_AskCallThrows()
        {
            var channel = new TestChannel();
            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channel)
                .Build();

            TestTools.AssertThrowsAndNotBlocks<ConnectionIsNotEstablishedYet>(() => proxyConnection.Contract.Ask());
        }

        [Test]
        public void ProxyConnectionIsLost_SayCallThrows()
        {
            var channel = new TestChannel();
            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channel)
                .Build();
            channel.ImmitateConnect();
            channel.ImmitateDisconnect();
            TestTools.AssertThrowsAndNotBlocks<ConnectionIsLostException>(() => proxyConnection.Contract.Say());
        }

        [Test]
        public void ProxyConnectionIsLost_AskCallThrows()
        {
            var channel = new TestChannel();
            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channel)
                .Build();

            channel.ImmitateConnect();
            channel.ImmitateDisconnect();

            TestTools.AssertThrowsAndNotBlocks<ConnectionIsLostException>(() => proxyConnection.Contract.Ask());
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

            TestTools.AssertThrowsAndNotBlocks<RemoteContractImplementationException>(()=> proxyConnection.Contract.Ask());
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
            TestTools.AssertThrowsAndNotBlocks<RemoteUnhandledException>(() => proxyConnection.Contract.Ask());
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
            TestTools.AssertThrowsAndNotBlocks<RemoteUnhandledException>(() => originConnection.Contract.OnAsk());
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

        [Test]
        public void Disconnected_duringOriginAsk_throws()
        {
            var channelPair = TntTestHelper.CreateChannelPair();

            var originConnection = TntBuilder
                .UseContract<ITestContract, TestContractMock>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseContractInitalization((c, _) =>
                {
                    c.OnAsk += () =>
                    {
                        channelPair.Disconnect();
                        try {
                            //call say method to notify channel about disconnection (tcp channel immitation)
                            originConnection.Contract.Say();
                        } catch (ConnectionIsLostException) { /*suppressTheException*/ }
                        return 0;
                    };
                })
                .UseChannel(channelPair.CahnnelA)
                .Build();
            channelPair.ConnectAndStartReceiving();
            TestTools.AssertThrowsAndNotBlocks<ConnectionIsLostException>(() => originConnection.Contract.OnAsk());
        }
      
    }
}
