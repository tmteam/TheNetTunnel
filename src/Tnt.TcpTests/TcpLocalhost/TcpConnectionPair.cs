using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using TNT.Api;
using TNT.Presentation.Deserializers;
using TNT.Presentation.ReceiveDispatching;
using TNT.Presentation.Serializers;
using TNT.Tcp;
using TNT.Tests.Presentation.Contracts;

namespace TNT.IntegrationTests.TcpLocalhost
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
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .CreateTcpServer(IPAddress.Loopback, 12345);
            ClientChannel = new TcpChannel();
            ProxyConnection = TntBuilder
                .UseContract<TProxyContractInterface>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
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
            Server.Close();
        }
        public void Dispose()
        {
            Disconnect();
        }
    }
    public class TcpConnectionPair: TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
    {
    }
}