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

namespace TNT.Tests.Presentation.FullStack
{
    [TestFixture]
    public class ServerTest
    {
        [Test]
        public void ServerRecieveConnection_AfterConnectRaised()
        {
            var server = new TestChannelServer<TestContractImplementation>(new ConnectionBuilder<TestContractImplementation>());
            server.IsListening = true;
            Connection< TestContractImplementation, TestChannel> incomeContractConnection = null;
            server.AfterConnect += (sender, income) => incomeContractConnection = income;
            var clientChannel = new TestChannel();
            var proxyConnection = new ConnectionBuilder<ITestContract>().UseChannel(clientChannel).Buid();
            server.TestListener.ImmitateAccept(clientChannel);
            Assert.IsNotNull(incomeContractConnection);
        }
    }
}
