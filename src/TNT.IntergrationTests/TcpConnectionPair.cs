using System;
using System.Linq;
using System.Net;
using NUnit.Framework;
using TNT.Api;
using TNT.Presentation.ReceiveDispatching;
using TNT.Tcp;
using TNT.Tests;
using TNT.Tests.Contracts;

namespace TNT.IntegrationTests
{
    public class TcpConnectionPair<TProxyContractInterface, TOriginContractInterface, TOriginContractType> : IDisposable
        where TProxyContractInterface: class
        where TOriginContractInterface: class 
        where TOriginContractType: class, TOriginContractInterface,  new() 
    {
        public IConnection<TProxyContractInterface, TcpChannel> ProxyConnection { get; }
        public TcpChannel ClientChannel { get; }
        private TNT.Tests.EventAwaiter<IConnection<TOriginContractInterface, TcpChannel>> _eventAwaiter;

        public IConnection<TOriginContractInterface, TcpChannel> OriginConnection { get; private set; } = null;
        public TOriginContractType OriginContract => OriginConnection.Contract as TOriginContractType;
        public TProxyContractInterface ProxyContract => ProxyConnection.Contract;
        public TcpChannelServer<TOriginContractInterface> Server { get; }

        public TcpConnectionPair(PresentationBuilder<TOriginContractInterface> originBuilder,
            PresentationBuilder<TProxyContractInterface> proxyBuider, bool connect = true)
        {
            Server = originBuilder.CreateTcpServer(IPAddress.Loopback, 12345);
            ClientChannel = new TcpChannel();
            ProxyConnection = proxyBuider.UseChannel(ClientChannel).Build();
            _eventAwaiter = new TNT.Tests.EventAwaiter<IConnection<TOriginContractInterface, TcpChannel>>();
            Server.AfterConnect += _eventAwaiter.EventRaised;
            if (connect)
                Connect();
        }
        public TcpConnectionPair(bool connect = true)
        {
            Server = TntBuilder
                .UseContract<TOriginContractInterface, TOriginContractType>()
              //  .UseReceiveDispatcher<NotThreadDispatcher>()
                .CreateTcpServer(IPAddress.Loopback, 12345);
            ClientChannel = new TcpChannel();
            ProxyConnection = TntBuilder
                .UseContract<TProxyContractInterface>()
               // .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(ClientChannel)
                .Build();
           
            if (connect)
                Connect();
        }
        public void Connect()
        {
            _eventAwaiter = new TNT.Tests.EventAwaiter<IConnection<TOriginContractInterface, TcpChannel>>();
            Server.AfterConnect += _eventAwaiter.EventRaised;
            Server.IsListening = true;
            ClientChannel.Connect(new IPEndPoint(IPAddress.Loopback, 12345));
            OriginConnection = _eventAwaiter.WaitOneOrDefault(500);
            Assert.IsNotNull(OriginConnection);
        }

        public void Disconnect()
        {
            OriginConnection.Channel.Disconnect();
            ProxyConnection.Channel.Disconnect();
        }

        public void DisconnectAndClose()
        {
            Disconnect();
            Server.Close();
        }
        public void Dispose()
        {
            DisconnectAndClose();
        }
        public void AssertPairIsConnected()
        {
            Assert.IsTrue( ProxyConnection.Channel.IsConnected, "Client connection has to be connected");
            Assert.IsTrue(OriginConnection.Channel.IsConnected, "Server connection has to be connected");
            Assert.AreEqual(1, Server.GetAllConnections().Count(), "Server connections list has to have 1 connection");
            Assert.IsTrue(Server.IsListening, "Server has to continue listening");
        }
        public void AssertPairIsDisconnected()
        {
            //Some disconnection subroutine could be asynchronous.
            TestTools.AssertTrue(() => !ProxyConnection.Channel.IsConnected, 1000, "Client connection has to be disconnected");
            TestTools.AssertTrue(() => !OriginConnection.Channel.IsConnected, 1000, "Server connection has to be disconnected");
            TestTools.AssertTrue(() => !Server.GetAllConnections().Any(), 1000, "Server connections list has to be empty");
            TestTools.AssertTrue(() =>  Server.IsListening, 1000, "Server has to continue listening");
            /*
            Assert.IsFalse(ProxyConnection.Channel.IsConnected, "Client connection has to be disconnected");
            Assert.IsFalse(OriginConnection.Channel.IsConnected, "Server connection has to be disconnected");
            Assert.AreEqual(0, Server.GetAllConnections().Count(), "Server connections list has to be empty");
            Assert.IsTrue(Server.IsListening, "Server has to continue listening");*/
        }

      
    }
    public class TcpConnectionPair: TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
    {
    }
}