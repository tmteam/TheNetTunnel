using NUnit.Framework;
using TNT.Channel;
using TNT.Channel.Test;

namespace TNT.Tests.Presentation.FullStack
{
    [TestFixture]
    public class ConnectionBuilderTest
    {
        [Test]
        public void ProxyBuilder_ChannelConnectedBefore_SayCalled_DataSent()
        {
            var channel = new TestChannel();
            channel.ImmitateConnect();
            var proxyConnection = new ConnectionBuilder<ITestContract>().UseChannel(channel).Buid();
            proxyConnection.Channel.ImmitateConnect();
            byte[] sentMessage = null;
            proxyConnection.Channel.OnWrited += (s, msg) => sentMessage = msg;
            proxyConnection.Contract.Say();

            Assert.IsNotNull(sentMessage);
            Assert.IsNotEmpty(sentMessage);
        }
        [Test]
        public void ProxyBuilder_SayCalled_DataSent()
        {
            var channel = new TestChannel();
            var proxyConnection = new ConnectionBuilder<ITestContract>().UseChannel(channel).Buid();
            proxyConnection.Channel.ImmitateConnect();
            byte[] sentMessage = null;
            proxyConnection.Channel.OnWrited += (s, msg) => sentMessage = msg;
            proxyConnection.Contract.Say();

            Assert.IsNotNull(sentMessage);
            Assert.IsNotEmpty(sentMessage);
        }
        [Test]
        public void ProxyBuilderCreatesWithCorrectConnection()
        {
            var channel = new TestChannel();
            var proxyConnection = new ConnectionBuilder<ITestContract>().UseChannel(channel).Buid();
            Assert.AreEqual(channel, proxyConnection.Channel);
        }
        [Test]
        public void ProxyBuilderBuilds_ChannelAllowReceiveIsTrue()
        {
            var channel = new TestChannel();
            var proxyConnection = new ConnectionBuilder<ITestContract>()
                .UseChannel(channel)
                .Buid();
            Assert.IsTrue(channel.AllowReceive);
        }
        [Test]

        public void ProxyBuilder_UseContractInitalization_CalledBeforeBuildDone()
        {
            var channel = new TestChannel();
            ITestContract initializationArgument = null;
            var proxyConnection = new ConnectionBuilder<ITestContract>()
                .UseContractInitalization((i)=> initializationArgument = i)
                .UseChannel(channel)
                .Buid();
            Assert.IsNotNull(initializationArgument);
            Assert.AreEqual(proxyConnection.Contract, initializationArgument);
        }
        [Test]

        public void ConnectionDisposes_channelBecomesDisconnected()
        {
            var channel = new TestChannel();
            using (var proxyConnection = new ConnectionBuilder<ITestContract>()
                .UseChannel(new TestChannel())
                .Buid())
            {
                proxyConnection.Channel.ImmitateConnect();    
            }
            Assert.IsFalse(channel.IsConnected);
        }
        [Test]
        public void OriginContract_CreatesByType_ContractCreated()
        {
            var channel = new TestChannel();
            
            var proxyConnection = new ConnectionBuilder<TestContractImplementation>()
                .UseChannel(channel)
                .Buid();
            Assert.IsNotNull(proxyConnection.Contract);
        }
        [Test]
        public void OriginContract_CreatesByFactory_ContractCreated()
        {
            var proxyConnection = new ConnectionBuilder<TestContractImplementation>(()=>new TestContractImplementation())
                .UseChannel(new TestChannel())
                .Buid();

            Assert.IsNotNull(proxyConnection.Contract);
        }
        [Test]
        public void OriginContractAsInterface_CreatesByFactory_ContractCreated()
        {
            var proxyConnection = new ConnectionBuilder<ITestContract>(() => new TestContractImplementation())
                .UseChannel(new TestChannel())
                .Buid();

            Assert.IsInstanceOf<TestContractImplementation>(proxyConnection.Contract);
        }

        [Test]
        public void OriginContractAsSingleTone_CreatesByFactory_ContractCreated()
        {
            var contract = new TestContractImplementation();
            var proxyConnection = new ConnectionBuilder<ITestContract>(contract)
                .UseChannel(new TestChannel())
                .Buid();
            Assert.AreEqual(contract, proxyConnection.Contract);
        }
    }
}