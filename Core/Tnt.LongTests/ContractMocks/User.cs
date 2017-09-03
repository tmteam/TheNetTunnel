using NUnit.Framework;
using ProtoBuf;

namespace TNT.IntegrationTests.ContractMocks
{
    [ProtoContract]
    public class User
    {
        [ProtoMember(1)]
        public string Name;
        [ProtoMember(2)]
        public int Age;
        [ProtoMember(3)]
        public byte[] Payload;

        public void AssertIsSameTo(User user)
        {
            Assert.AreEqual(user.Name, Name);
            Assert.AreEqual(user.Age, Age);
            CollectionAssert.AreEqual(user.Payload, Payload);
        }
    }
}