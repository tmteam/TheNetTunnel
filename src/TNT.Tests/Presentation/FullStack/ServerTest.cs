using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using NUnit.Framework;
using TNT.Cord.Deserializers;
using TNT.Light;
using TNT.Presentation;
using TNT.Testing;

namespace TNT.Tests.Presentation.FullStack
{
    [TestFixture]
    public class ServerTest
    {
        [Test]
        public void ServerAcceptConnection_BeforeConnectRaised()
        {
            var server = new TestChannelServer<ITestContract>(ConnectionBuilder.UseContract<ITestContract, TestContractImplementation>());
            server.IsListening = true;
            BeforeConnectEventArgs<ITestContract, TestChannel> connectionArgs = null;
            server.BeforeConnect  += (sender, args) => connectionArgs = args;

            var clientChannel = new TestChannel();
            var proxyConnection = ConnectionBuilder.UseContract<ITestContract>().UseChannel(clientChannel).Build();

            server.TestListener.ImmitateAccept(clientChannel);

            Assert.IsNotNull(connectionArgs, "AfterConnect not raised");
        }

        [Test]
        public void ServerAcceptConnection_AfterConnectRaised()
        {
            var server = new TestChannelServer<ITestContract>(ConnectionBuilder.UseContract<ITestContract,TestContractImplementation>());
            server.IsListening = true; 
            Connection<ITestContract, TestChannel> incomeContractConnection = null;
            server.AfterConnect += (sender, income) => incomeContractConnection = income;
            var clientChannel = new TestChannel();
            var proxyConnection = ConnectionBuilder.UseContract<ITestContract>().UseChannel(clientChannel).Build();
            server.TestListener.ImmitateAccept(clientChannel);
            Assert.IsNotNull(incomeContractConnection, "AfterConnect not raised");
        }


        [Test]
        public void ServerAcceptConnection_AllowReceiveEqualTrue()
        {
            var server = new TestChannelServer<ITestContract>(ConnectionBuilder.UseContract<ITestContract, TestContractImplementation>());
            server.IsListening = true;
            var clientChannel = new TestChannel();
            ConnectionBuilder.UseContract<ITestContract>().UseChannel(clientChannel).Build();
            server.TestListener.ImmitateAccept(clientChannel);
            Assert.IsTrue(server.GetAllConnections().First().Channel.AllowReceive);
        }

        [Test]
        public void ClientDisconnected_DisconnectedRaised()
        {
            var server = new TestChannelServer<ITestContract>(ConnectionBuilder.UseContract<ITestContract, TestContractImplementation>());
            server.IsListening = true;
            Connection<ITestContract, TestChannel> disconnectedConnection = null;
            server.Disconnected += (sender, income) => disconnectedConnection = income;
            var clientChannel = new TestChannel();
            var proxyConnection = ConnectionBuilder.UseContract<ITestContract>().UseChannel(clientChannel).Build();
            var pair = server.TestListener.ImmitateAccept(clientChannel);

            pair.Disconnect();

            Assert.IsNotNull(disconnectedConnection, "Disconnect not raised");
        }


    }
}
