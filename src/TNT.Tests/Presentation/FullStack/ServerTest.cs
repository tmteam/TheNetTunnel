using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using NUnit.Framework;
using TNT.Channel;
using TNT.Channel.Test;
using TNT.Cord.Deserializers;
using TNT.Light;
using TNT.Presentation;

namespace TNT.Tests.Presentation.FullStack
{
    [TestFixture]
    public class ServerTest
    {
        [Test]
        public void ServerRecieveConnection_BeforeConnectRaised()
        {
            var server = new TestChannelServer<ITestContract>(ConnectionBuilder.UseContract<ITestContract, TestContractImplementation>());
            server.IsListening = true;
            BeforeConnectEventArgs<ITestContract, TestChannel> connectionArgs = null;
            server.BeforeConnect  += (sender, args) => connectionArgs = args;

            var clientChannel = new TestChannel();
            var proxyConnection = ConnectionBuilder.UseContract<ITestContract>().UseChannel(clientChannel).Buid();

            server.TestListener.ImmitateAccept(clientChannel);

            Assert.IsNotNull(connectionArgs, "AfterConnect not raised");
        }

        [Test]
        public void ServerRecieveConnection_AfterConnectRaised()
        {
            var server = new TestChannelServer<ITestContract>(ConnectionBuilder.UseContract<ITestContract,TestContractImplementation>());
            server.IsListening = true; 
            Connection<ITestContract, TestChannel> incomeContractConnection = null;
            server.AfterConnect += (sender, income) => incomeContractConnection = income;
            var clientChannel = new TestChannel();
            var proxyConnection = ConnectionBuilder.UseContract<ITestContract>().UseChannel(clientChannel).Buid();
            server.TestListener.ImmitateAccept(clientChannel);
            Assert.IsNotNull(incomeContractConnection, "AfterConnect not raised");
        }
    }
}
