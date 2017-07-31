using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TNT.Api;
using TNT.Exceptions.Local;
using TNT.Exceptions.Remote;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;
using TNT.Tests;
using TNT.Tests.Presentation.Contracts;

namespace TNT.IntegrationTests.TcpLocalhost
{
    [TestFixture]
    public class SerializationExceptionsTest
    {
        [Test]
        public void LocalProxySerializationFails_throws()
        {
            var proxyContractBuilder = GetProxyBuilder()
                .UseSerializer(GetThrowsSerializationRuleFor<string>());

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (GetOriginBuilder(), proxyContractBuilder))
            {
                //local string serializer throws.
                Assert.Multiple(() =>
                {
                    Assert.Throws<LocalSerializationException>(() => tcpPair.ProxyContract.Say("testString"));
                    tcpPair.AssertPairIsDisconnected();
                });
            }
        }

        [Test]
        public void LocalOriginSerializationFails_throws()
        {
            var originContractBuilder = GetOriginBuilder()
                .UseSerializer(GetThrowsSerializationRuleFor<string>());

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, GetProxyBuilder()))
            {
                //local origin serializer fails
                Assert.Multiple(() =>
                {
                    Assert.Throws<LocalSerializationException>(() => tcpPair.OriginContract.OnSayS("testString"));
                    tcpPair.AssertPairIsDisconnected();
                });
            }
        }

        [Test]
        public void RemoteProxySeserializationFails_throws()
        {
            var originContractBuilder = GetOriginBuilder().UseSerializer(GetThrowsSerializationRuleFor<string>());

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, GetProxyBuilder()))
            {
                //origin contract uses failed string serializer, when it returns answer
                Assert.Multiple(() =>
                {
                    Assert.Throws<RemoteSerializationException>(() => tcpPair.ProxyContract.Ask("testString"));
                    tcpPair.AssertPairIsDisconnected();
                });
            }
        }

        [Test]
        public void RemoteOriginSeserializationFails_throws()
        {
            var proxyContractBuilder = GetProxyBuilder().UseSerializer(GetThrowsSerializationRuleFor<string>());

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (GetOriginBuilder(), proxyContractBuilder))
            {
                //proxy contract uses failed string serializer, when it returns answer
                Assert.Multiple(() =>
                {
                    Assert.Throws<RemoteSerializationException>(() => tcpPair.OriginContract.OnAskS("testString"));
                    tcpPair.AssertPairIsDisconnected();
                });
            }
        }

        [Test]
        public void LocalProxyDeserializationFails_throws()
        {
            var proxyContractBuilder =GetProxyBuilder().UseDeserializer(GetThrowsDeserializationRuleFor<string>());

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (GetOriginBuilder(), proxyContractBuilder))
            {
                //proxy contract uses failed string deserializer, when it accepts answer
                Assert.Multiple(() =>
                {
                    Assert.Throws<LocalSerializationException>(() => tcpPair.ProxyContract.Ask("testString"));
                    tcpPair.AssertPairIsDisconnected();
                });
            }
        }

        [Test]
        public void LocalOriginDeserializationFails_throws()
        {
            var originContractBuilder = GetOriginBuilder()
                .UseDeserializer(GetThrowsDeserializationRuleFor<string>());

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, GetProxyBuilder()))
            {
                tcpPair.ProxyContract.OnAskS += (s) => s;
                //origin contract uses failed string deserializer, when it accepts answer
                Assert.Multiple(() =>
                {
                    Assert.Throws<LocalSerializationException>(() => tcpPair.OriginContract.OnAskS("testString"));
                    tcpPair.AssertPairIsDisconnected();
                });
            }
        }

        [Test]
        public void RemoteProxyDeseserializationFails_throws()
        {
            var originContractBuilder = GetOriginBuilder().UseDeserializer(GetThrowsDeserializationRuleFor<string>());
            
            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (originContractBuilder, GetProxyBuilder()))
            {
                //origin contract uses failed string deserializer, when it returns answer
                Assert.Multiple(() =>
                {
                    Assert.Throws<RemoteSerializationException>(() => tcpPair.ProxyContract.Ask("testString"));
                    tcpPair.AssertPairIsDisconnected();
                });
            }
        }

     
        [Test]
        public void RemoteOriginDeserializationFails_throws()
        {
            var proxyContractBuilder = GetProxyBuilder().UseDeserializer(GetThrowsDeserializationRuleFor<string>());

            using (var tcpPair = new TcpConnectionPair<ITestContract, ITestContract, TestContractMock>
                (GetOriginBuilder(), proxyContractBuilder))
            {
                //proxy contract uses failed string deserializer, when it returns answer
                Assert.Multiple(() =>
                {
                    Assert.Throws<RemoteSerializationException>(() => tcpPair.OriginContract.OnAskS("testString"));
                    tcpPair.AssertPairIsDisconnected();
                });
            }
        }

        [Test]
        public void SerializationThrowsRule_throws()
        {
            //Just check the Moq generator behaves as expected
            var rule = GetThrowsSerializationRuleFor<string>();
            var serializer = rule.GetSerializer(typeof(string), SerializerFactory.CreateDefault());
            Assert.Throws<Exception>(() => serializer.Serialize(new object(), new MemoryStream()));
        }

        [Test]
        public void DeserializationThrowsRule_throws()
        {
            //Just check the Moq generator behaves as expected
            var rule = GetThrowsDeserializationRuleFor<string>();
            var deserializer = rule.GetDeserializer(typeof(string), DeserializerFactory.CreateDefault());
            Assert.Throws<Exception>(() => deserializer.Deserialize(new MemoryStream(), 0));
        }

        private PresentationBuilder<ITestContract> GetOriginBuilder()
        {
            return TntBuilder
                .UseContract<ITestContract, TestContractMock>();
        }
        private PresentationBuilder<ITestContract> GetProxyBuilder()
        {
            return TntBuilder
                .UseContract<ITestContract>();
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

    }
}
