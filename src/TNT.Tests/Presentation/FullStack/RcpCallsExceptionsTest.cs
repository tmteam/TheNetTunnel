using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TNT.Channel.Test;
using TNT.Exceptions;
using TNT.Light;
using TNT.Presentation;
using TNT.Tests.Presentation.Contracts;
using TNT.Tests.Presentation.Proxy.ContractInterfaces;

namespace TNT.Tests.Presentation.FullStack
{
    [TestFixture]
    public class RcpCallsExceptionsTest
    {
        [Test]
        public void ProxyConnectionIsNotEstablishedYet_SayCallThrows()
        {
            var channel = new TestChannel();
            var proxyConnection = ConnectionBuilder
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
            var proxyConnection = ConnectionBuilder
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
            var proxyConnection = ConnectionBuilder
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
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channel)
                .Build();

            channel.ImmitateConnect();
            channel.ImmitateDisconnect();

            TestTools.AssertThrowsAndNotBlocks<ConnectionIsNotEstablishedYet>(() => proxyConnection.Contract.Ask());
        }



        [Test]
        public void Proxy_AskMissingCord_Throws()
        {
            var channelPair = TntTestHelper.CreateChannelPair();
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = ConnectionBuilder
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
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = ConnectionBuilder
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

            var proxyConnection = ConnectionBuilder
                .UseContract<IEmptyContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
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
            var proxyConnection = ConnectionBuilder
                .UseContract<IExceptionalContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = ConnectionBuilder
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
            var proxyConnection = ConnectionBuilder
                .UseContract<IExceptionalContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = ConnectionBuilder
                .UseContract<IExceptionalContract, ExceptionalContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();
            TestTools.AssertThrowsAndNotBlocks<RemoteSideUnhandledException>(() => proxyConnection.Contract.Ask());
        }

        [Test]
        public void Origin_SayExceptioanlCallback_NotThrows()
        {
            var channelPair = TntTestHelper.CreateChannelPair();

            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseContractInitalization((c, _) =>
                {
                    c.OnSay += () => { throw new InvalidOperationException(); };
                    c.OnAsk += () => { throw new InvalidOperationException(); };
                })
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
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

            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseContractInitalization((c, _) =>
                {
                    c.OnSay += () => { throw new InvalidOperationException(); };
                    c.OnAsk += () => { throw new InvalidOperationException(); };
                })
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();
            TestTools.AssertThrowsAndNotBlocks<RemoteSideUnhandledException>(() => originConnection.Contract.Ask());
        }
        [Test]
        public void Origin_AsksNotImplemented_returnsDefault()
        {
            var channelPair = TntTestHelper.CreateChannelPair();

            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();

            var answer = TestTools.AssertNotBlocks(originConnection.Contract.Ask).Result;
            Assert.AreEqual(default(int), answer);
        }

        [Test]
        public void Disconnected_duringOriginAsk_throws()
        {
            var channelPair = TntTestHelper.CreateChannelPair();
            ManualResetEvent AskCalled = new ManualResetEvent(false);
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseContractInitalization((c, _) =>
                {
                    c.OnAsk += () =>
                    {
                        AskCalled.Set();
                        //block the thread
                        new ManualResetEvent(false).WaitOne();
                        return 0;
                    };
                })
                .UseChannel(channelPair.CahnnelA)
                .Build();

            var originConnection = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(channelPair.ChannelB)
                .Build();

            channelPair.ConnectAndStartReceiving();
            Task.Run(() =>
                {
                    //Wait for ask method called
                    AskCalled.WaitOne();
                    //While origin ask call is busy - immitate disconnect
                    channelPair.Disconnect();
                    try
                    {
                        //call say method to notify channel about disconnection (tcp channel immitating)
                        originConnection.Contract.Say();
                    }
                    catch (ConnectionIsLostException)
                    {
                    }
                }
            );
            Assert.Throws<ConnectionIsLostException>(() => originConnection.Contract.Ask());
        }
      
    }
}
