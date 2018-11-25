using System.IO;
using CommonTestTools;
using NUnit.Framework;
using Tnt.LongTests.ContractMocks;

namespace Tnt.LongTests.Serialization
{
   
    [TestFixture]
    public class ProtobuffBigSerializationTest
    {
        [Test]
        public void PacketOf500Kb_Serialization_deserializesSame()
        {
            SerializeAndDeserializeProtobuffMessage(1000);
        }

        [Test]
        public void PacketOf2mb_Serialization_deserializesSame()
        {
            SerializeAndDeserializeProtobuffMessage(2000);
        }

        [Test]
        public void PacketOf10mb_Serialization_deserializesSame()
        {
            SerializeAndDeserializeProtobuffMessage(5000);
        }
        [Test]
        public void PacketOf50mb_Serialization_deserializesSame()
        {
            SerializeAndDeserializeProtobuffMessage(10000);
        }

       

        [Test]
        public void PacketOf500Kb_transmitsViaTcp() {
            CheckProtobuffEchoTransaction(1000);
        }
        [Test]
        public void PacketOf2mb_transmitsViaTcp() {
            CheckProtobuffEchoTransaction(2000);
        }
        [Test]
        public void PacketOf10mb_transmitsViaTcp() {
            CheckProtobuffEchoTransaction(5000);
        }
        [Test]
        public void PacketOf50mb_transmitsViaTcp() {
            CheckProtobuffEchoTransaction(10000);
        }

        private static void SerializeAndDeserializeProtobuffMessage(int companySize)
        {
            var company = IntegrationTestsHelper.CreateCompany(companySize);
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

        private static void CheckProtobuffEchoTransaction(int itemsSize)
        {
            using (var tcpPair = new TcpConnectionPair
                <ISingleMessageContract<Company>,
                ISingleMessageContract<Company>,
                SingleMessageContract<Company>>())
            {
                EventAwaiter<Company> callAwaiter = new EventAwaiter<Company>();
                tcpPair.OriginContract.SayCalled += callAwaiter.EventRaised;
                var company = IntegrationTestsHelper.CreateCompany(itemsSize);
                tcpPair.ProxyConnection.Contract.Ask(company);
                var received = callAwaiter.WaitOneOrDefault(5000);
                Assert.IsNotNull(received);
                received.AssertIsSameTo(company);
            }
        }

    }
}
