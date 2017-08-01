using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using NUnit.Framework;
using TNT.Tests;

namespace TNT.IntegrationTests.Serialization
{
   
    [TestFixture]
    public class ProtobuffBigSerializationTest
    {
        [Test]
        public void PacketOf500Kb_Serialization_deserializesSame()
        {
            Company company = IntegrationTestsHelper.CreateCompany(1000);
            using (var stream = new MemoryStream())
            {
                var serializer = new TNT.Presentation.Serializers.ProtoSerializer<Company>();
                serializer.SerializeT(company, stream);
                stream.Position = 0;
                var deserializer = new TNT.Presentation.Deserializers.ProtoDeserializer<Company>();
                var deserialized = deserializer.DeserializeT(stream, (int)stream.Length);
                company.AssertIsSameTo(deserialized);
            }
        }

        [Test]
        public void PacketOf2mb_Serialization_deserializesSame()
        {
            var company = IntegrationTestsHelper.CreateCompany(2000);
            using (var stream = new MemoryStream())
            {
                var serializer = new TNT.Presentation.Serializers.ProtoSerializer<Company>();
                serializer.SerializeT(company, stream);
                stream.Position = 0;
                var deserializer = new TNT.Presentation.Deserializers.ProtoDeserializer<Company>();
                var deserialized = deserializer.DeserializeT(stream, (int)stream.Length);
                company.AssertIsSameTo(deserialized);
            }
        }

        [Test]
        public void PacketOf10mb_Serialization_deserializesSame()
        {
            var company = IntegrationTestsHelper.CreateCompany(5000);
            using (var stream = new MemoryStream())
            {
                var serializer = new TNT.Presentation.Serializers.ProtoSerializer<Company>();
                serializer.SerializeT(company, stream);
                stream.Position = 0;
                var deserializer = new TNT.Presentation.Deserializers.ProtoDeserializer<Company>();
                var deserialized = deserializer.DeserializeT(stream, (int)stream.Length);
                company.AssertIsSameTo(deserialized);
            }
        }
        [Test]
        public void PacketOf50mb_Serialization_deserializesSame()
        {
            var company = IntegrationTestsHelper.CreateCompany(10000);
            using (var stream = new MemoryStream())
            {
                var serializer = new TNT.Presentation.Serializers.ProtoSerializer<Company>();
                serializer.SerializeT(company, stream);
                stream.Position = 0;
                var deserializer = new TNT.Presentation.Deserializers.ProtoDeserializer<Company>();
                var deserialized = deserializer.DeserializeT(stream, (int)stream.Length);
                company.AssertIsSameTo(deserialized);
            }
        }

        [Test]
        public void PacketOf500Kb_transmitsViaTcp()
        {
            using (var tcpPair = new TcpConnectionPair
                <ISingleMessageContract<Company>,
                ISingleMessageContract<Company>,
                SingleMessageContract<Company>>())
            {
                EventAwaiter<Company> callAwaiter = new EventAwaiter<Company>();
                tcpPair.OriginContract.SayCalled += callAwaiter.EventRaised;
                var company = IntegrationTestsHelper.CreateCompany(1000);
                tcpPair.ProxyConnection.Contract.Ask(company);
                var received = callAwaiter.WaitOneOrDefault(5000);
                Assert.IsNotNull(received);
                received.AssertIsSameTo(company);
            }
        }
        [Test]
        public void PacketOf2mb_transmitsViaTcp()
        {
            using (var tcpPair = new TcpConnectionPair
                <ISingleMessageContract<Company>,
                ISingleMessageContract<Company>,
                SingleMessageContract<Company>>())
            {
                EventAwaiter<Company> callAwaiter = new EventAwaiter<Company>();
                tcpPair.OriginContract.SayCalled += callAwaiter.EventRaised;
                var company = IntegrationTestsHelper.CreateCompany(2000);
                tcpPair.ProxyConnection.Contract.Ask(company);
                var received = callAwaiter.WaitOneOrDefault(5000);
                Assert.IsNotNull(received);
                received.AssertIsSameTo(company);
            }
        }
        [Test]
        public void PacketOf10mb_transmitsViaTcp()
        {
            using (var tcpPair = new TcpConnectionPair
                <ISingleMessageContract<Company>,
                ISingleMessageContract<Company>,
                SingleMessageContract<Company>>())
            {
                EventAwaiter<Company> callAwaiter = new EventAwaiter<Company>();
                tcpPair.OriginContract.SayCalled += callAwaiter.EventRaised;
                var company = IntegrationTestsHelper.CreateCompany(5000);
                tcpPair.ProxyConnection.Contract.Ask(company);
                var received = callAwaiter.WaitOneOrDefault(5000);
                Assert.IsNotNull(received);
                received.AssertIsSameTo(company);
            }
        }
        [Test]
        public void PacketOf50mb_transmitsViaTcp()
        {
            using (var tcpPair = new TcpConnectionPair
                <ISingleMessageContract<Company>,
                ISingleMessageContract<Company>,
                SingleMessageContract<Company>>())
            {
                EventAwaiter<Company> callAwaiter = new EventAwaiter<Company>();
                tcpPair.OriginContract.SayCalled += callAwaiter.EventRaised;
                var company = IntegrationTestsHelper.CreateCompany(10000);
                tcpPair.ProxyConnection.Contract.Ask(company);
                var received = callAwaiter.WaitOneOrDefault(5000);
                Assert.IsNotNull(received);
                received.AssertIsSameTo(company);
            }
        }
      

      
    }
}
