using System;
using NUnit.Framework;
using TNT.Api;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;
using TNT.Presentation.ReceiveDispatching;
using TNT.Testing;
using TNT.Tests.Contracts;

namespace TNT.Tests.FullStack
{
    [TestFixture]
    public class ConnectionFailedExceptionsTest
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
