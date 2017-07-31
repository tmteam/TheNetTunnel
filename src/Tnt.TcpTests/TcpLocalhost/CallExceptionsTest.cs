using System;
using NUnit.Framework;
using TNT.Api;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;
using TNT.Presentation.ReceiveDispatching;
using TNT.Tcp;
using TNT.Tests;
using TNT.Tests.Presentation.Contracts;
using System.Threading;
using TNT.Presentation.Serializers;
using Moq;
using System.IO;
using TNT.Presentation.Deserializers;

namespace TNT.IntegrationTests.TcpLocalhost
{
    [TestFixture]
    public class CallsExceptionsTest
    {
        [Test]
        public void ProxyConnectionIsNotEstablishedYet_SayCallThrows()
        {
            var proxyConnection = TntBuilder
                .UseContract<ITestContract>()
                .UseReceiveDispatcher<NotThreadDispatcher>()
                .UseChannel(() => new TcpChannel())
                .Build();

            TestTools.AssertThrowsAndNotBlocks<ConnectionIsNotEstablishedYet>(() => proxyConnection.Contract.Say());
        }


        [Test]
        public void ProxyConnectionIsNotEstablishedYet_AskCallThrows()
        {
            var proxyConnection = TntBuilder
               .UseContract<ITestContract>()
               .UseReceiveDispatcher<NotThreadDispatcher>()
               .UseChannel(() => new TcpChannel())
               .Build();

            TestTools.AssertThrowsAndNotBlocks<ConnectionIsNotEstablishedYet>(() => proxyConnection.Contract.Ask());
        }

        [Test]
        public void ProxyConnectionIsLost_SayCallThrows()
        {
            var tcpPair = new TcpConnectionPair();

            tcpPair.Disconnect();
            TestTools.AssertThrowsAndNotBlocks<ConnectionIsLostException>(() => tcpPair.ProxyConnection.Contract.Say());
        }

        [Test]
        public void ProxyConnectionIsLost_AskCallThrows()
        {
            var tcpPair = new TcpConnectionPair();
            tcpPair.Disconnect();
            TestTools.AssertThrowsAndNotBlocks<ConnectionIsLostException>(
                () => tcpPair.ProxyConnection.Contract.Ask());
        }



        [Test]
        public void Proxy_AskMissingCord_Throws()
        {
            using (var tcpPair = new TcpConnectionPair<ITestContract, IEmptyContract, EmptyContract>()) 
            {
                TestTools.AssertThrowsAndNotBlocks<RemoteContractImplementationException>(
                    () => tcpPair.ProxyConnection.Contract.Ask());
            }
        }
        [Test]
        public void Proxy_SayMissingCord_NotThrows()
        {
            using (var tcpPair = new TcpConnectionPair<ITestContract, IEmptyContract, EmptyContract>())
            {
                TestTools.AssertNotBlocks(
                    () => tcpPair.ProxyConnection.Contract.Say());
            }
        }
        [Test]
        public void Origin_SayMissingCord_NotThrows()
        {
            using (var tcpPair = new TcpConnectionPair<IEmptyContract, ITestContract, TestContractMock>())
            {
                TestTools.AssertNotBlocks(
                    () => tcpPair.OriginContract.OnSay());
            }
        }
        [Test]
        public void Proxy_SayWithException_CallNotThrows()
        {
            using (var tcpPair = new TcpConnectionPair<IExceptionalContract, IExceptionalContract, ExceptionalContract>())
            {
                TestTools.AssertNotBlocks(
                    () => tcpPair.ProxyConnection.Contract.Say());
            }
        }
        [Test]
        public void Proxy_AskWithException_Throws()
        {
            using (var tcpPair = new TcpConnectionPair<IExceptionalContract, IExceptionalContract, ExceptionalContract>())
            {
                TestTools.AssertThrowsAndNotBlocks<RemoteUnhandledException>(
                    () => tcpPair.ProxyConnection.Contract.Ask());
            }
        }

        [Test]
        public void Origin_SayExceptioanlCallback_NotThrows()
        {
            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>())
            {
               tcpPair.ProxyContract.OnSay += () => { throw new InvalidOperationException(); };
               TestTools.AssertNotBlocks(
                    () => tcpPair.OriginContract.OnSay());
            }
          
        }


        [Test]
        public void Origin_AskExceptioanlCallback_Throws()
        {
            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>())
            {
                tcpPair.ProxyContract .OnAsk += () => { throw new InvalidOperationException(); };
                TestTools.AssertThrowsAndNotBlocks<RemoteUnhandledException>(
                    () => tcpPair.OriginContract.OnAsk());
            }
        }
        [Test]
        public void Origin_AsksNotImplemented_returnsDefault()
        {
            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>())
            {
                var answer = TestTools.AssertNotBlocks(tcpPair.OriginContract.OnAsk).Result;
                Assert.AreEqual(default(int), answer);
            }
        }

        [Test]
        public void Disconnected_duringOriginAsk_throws()
        {
            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>())
            {
                tcpPair.ProxyConnection.Contract.OnAsk += () =>
                {
                    tcpPair.Disconnect();
                    return 0;
                };

                TestTools.AssertThrowsAndNotBlocks<ConnectionIsLostException>(
                    () => tcpPair.OriginContract.OnAsk());
            }
        }

        #region serialization

        [Test]
        public void LocalProxySerializationFails_throws()
        {
            var proxyContractBuilder = TntBuilder
                 .UseContract<ITestContract>()
                 .UseSerializer(GetThrowsSerializationRuleFor<string>());
            var originContractBuilder = TntBuilder
                 .UseContract<ITestContract, TestContractMock>();
            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, proxyContractBuilder))
            {
                //local string serializer throws.
                Assert.Throws<LocalSerializationException>(() => tcpPair.ProxyContract.Say("testString"));
            }
        }
        [Test]
        public void LocalOriginSerializationFails_throws()
        {
            var originContractBuilder = TntBuilder
                .UseContract<ITestContract, TestContractMock>()
                 .UseSerializer(GetThrowsSerializationRuleFor<string>());

            var proxyContractBuilder = TntBuilder
                .UseContract<ITestContract>();

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, proxyContractBuilder))
            {
                //local origin serializer fails
                Assert.Throws<LocalSerializationException>(() => tcpPair.OriginContract.OnSayS("testString"));
            }
        }
        [Test]
        public void RemoteProxySeserializationFails_throws()
        {
            var originContractBuilder = TntBuilder
                 .UseContract<ITestContract, TestContractMock>()
                 .UseSerializer(GetThrowsSerializationRuleFor<string>());

            var proxyContractBuilder = TntBuilder
                 .UseContract<ITestContract>();

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, proxyContractBuilder))
            {
                //origin contract uses failed string serializer, when it returns answer
                Assert.Throws<RemoteSerializationException>(() => tcpPair.ProxyContract.Ask("testString"));
            }
        }
        [Test]
        public void RemoteOriginSeserializationFails_throws()
        {
            var originContractBuilder = TntBuilder
                .UseContract<ITestContract, TestContractMock>();

            var proxyContractBuilder = TntBuilder
                 .UseContract<ITestContract>()
                 .UseSerializer(GetThrowsSerializationRuleFor<string>());

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, proxyContractBuilder))
            {
                //proxy contract uses failed string serializer, when it returns answer
                Assert.Throws<RemoteSerializationException>(() => tcpPair.OriginContract.OnAskS("testString"));
            }
        }


        [Test]
        public void LocalProxyDeserializationFails_throws()
        {
            var originContractBuilder = TntBuilder
                 .UseContract<ITestContract, TestContractMock>();

            var proxyContractBuilder = TntBuilder
                 .UseContract<ITestContract>()
                 .UseDeserializer(GetThrowsDeserializationRuleFor<string>());

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, proxyContractBuilder))
            {
                //proxy contract uses failed string deserializer, when it accepts answer
                Assert.Throws<LocalSerializationException>(() => tcpPair.ProxyContract.Ask("testString"));
            }
        }
        [Test]
        public void LocalOriginDeserializationFails_throws()
        {
            var originContractBuilder = TntBuilder
                 .UseContract<ITestContract, TestContractMock>()
                 .UseDeserializer(GetThrowsDeserializationRuleFor<string>());

            var proxyContractBuilder = TntBuilder
                .UseContract<ITestContract>();

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, proxyContractBuilder))
            {
                //origin contract uses failed string deserializer, when it accepts answer
                Assert.Throws<LocalSerializationException>(() => tcpPair.OriginContract.OnAskS("testString"));
            }
        }

        [Test]
        public void RemoteProxyDeseserializationFails_throws()
        {
            var originContractBuilder = TntBuilder
                 .UseContract<ITestContract, TestContractMock>()
                 .UseDeserializer(GetThrowsDeserializationRuleFor<string>());

            var proxyContractBuilder = TntBuilder
                 .UseContract<ITestContract>();

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, proxyContractBuilder))
            {
                //origin contract uses failed string deserializer, when it returns answer
                Assert.Throws<RemoteSerializationException>(() => tcpPair.ProxyContract.Ask("testString"));
            }
        }
     
        [Test]
        public void RemoteOriginDeserializationFails_throws()
        {
            var originContractBuilder = TntBuilder
                .UseContract<ITestContract, TestContractMock>();

            var proxyContractBuilder = TntBuilder
                 .UseContract<ITestContract>()
                 .UseDeserializer(GetThrowsDeserializationRuleFor<string>());

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, proxyContractBuilder))
            {
                //proxy contract uses failed string deserializer, when it returns answer
                Assert.Throws<RemoteSerializationException>(() => tcpPair.OriginContract.OnAskS("testString"));
            }
        }


        [Test]
        public void SerializationThrowsRule_throws()
        {
            //Just check the Moq generator behaves as expected
            var rule = GetThrowsSerializationRuleFor<string>();
            var serializer = rule.GetSerializer(typeof(string), SerializerFactory.CreateDefault());
             Assert.Throws<Exception>(()=> serializer.Serialize(new object(), new MemoryStream()));
        }
        private SerializationRule GetThrowsSerializationRuleFor<T>()
        {
            var fakeSerializer = new Mock<ISerializer>();

            fakeSerializer
                .Setup(s => s.Serialize(It.IsAny<object>(), It.IsAny<MemoryStream>()))
                .Callback(() => { throw new Exception("Fake exception"); });
            var throwsRule = new SerializationRule((t) => t == typeof(T), (t) => fakeSerializer.Object);
            return throwsRule;
        }
        private DeserializationRule GetThrowsDeserializationRuleFor<T>()
        {
            var fakeDeserializer = new Mock<IDeserializer>();

            fakeDeserializer
                .Setup(s => s.Deserialize(It.IsAny<Stream>(), It.IsAny<int>()))
                .Callback(() => { throw new Exception(); });
            var throwsRule = new DeserializationRule((t) => t == typeof(T), (t) => fakeDeserializer.Object);
            return throwsRule;
        }
        #endregion
    }

}
