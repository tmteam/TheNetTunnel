using NUnit.Framework;
using TNT.Exceptions;
using TNT.Exceptions.ContractImplementation;
using TNT.Presentation;
using TNT.Testing;
using TNT.Tests.Presentation.Proxy.ContractInterfaces;

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

            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseChannel(channel)
                .Build();

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
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseChannel(channel)
                .Build();
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
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseChannel(channel)
                .Build();
            Assert.AreEqual(channel, proxyConnection.Channel);
        }
        [Test]
        public void ProxyBuilderBuilds_ChannelAllowReceiveIsTrue()
        {
            var channel = new TestChannel();
            channel.ImmitateConnect();
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>()
                .UseChannel(channel)
                .Build();

            Assert.IsTrue(channel.AllowReceive);
        }
        [Test]

        public void ProxyBuilder_UseContractInitalization_CalledBeforeBuildDone()
        {
            var channel = new TestChannel();
            ITestContract initializationArgument = null;
            var proxyConnection = ConnectionBuilder.UseContract<ITestContract>()
                .UseContractInitalization((i,c)=> initializationArgument = i)
                .UseChannel(channel)
                .Build();
            Assert.IsNotNull(initializationArgument);
            Assert.AreEqual(proxyConnection.Contract, initializationArgument);
        }
        [Test]

        public void ConnectionDisposes_channelBecomesDisconnected()
        {
            var channel = new TestChannel();
            using (var proxyConnection = ConnectionBuilder.UseContract<ITestContract>()
                .UseChannel(new TestChannel())
                .Build())
            {
                proxyConnection.Channel.ImmitateConnect();    
            }
            Assert.IsFalse(channel.IsConnected);
        }
        [Test]
        public void OriginContract_CreatesByType_ContractCreated()
        {
            var channel = new TestChannel();
            
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
                .UseChannel(channel)
                .Build();
            Assert.IsNotNull(proxyConnection.Contract);
        }
        [Test]
        public void OriginContract_CreatesByFactory_ContractCreated()
        {
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
                .UseChannel(new TestChannel())
                .Build();

            Assert.IsNotNull(proxyConnection.Contract);
        }
        [Test]
        public void OriginContractAsInterface_CreatesByFactory_ContractCreated()
        {
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract, TestContractImplementation>()
                .UseChannel(new TestChannel())
                .Build();

            Assert.IsInstanceOf<ITestContract>(proxyConnection.Contract);
        }

        [Test]
        public void OriginContractAsSingleTone_CreatesByFactory_ContractCreated()
        {
            var contract = new TestContractImplementation();
            var proxyConnection = ConnectionBuilder
                .UseContract<ITestContract>(contract)
                .UseChannel(new TestChannel())
                .Build();
            Assert.AreEqual(contract, proxyConnection.Contract);
        }

        [Test]
        public void UnserializeableContract_CreateT_throwsException()
        {
            var builder = ConnectionBuilder
                .UseContract<IUnserializeableContract>()
                .UseChannel(new TestChannel());
         
            Assert.Throws<TypeCannotBeSerializedException>(()=>builder.Build());
        }
        [Test]
        public void UnDeserializeableContract_CreateT_throwsException()
        {
            var builder = ConnectionBuilder
                .UseContract<IUnDeserializeableContract>()
                .UseChannel(new TestChannel());

            Assert.Throws<TypeCannotBeDeserializedException>(() => builder.Build());
        }
    }
}