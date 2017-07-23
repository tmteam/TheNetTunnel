using System;
using System.IO;
using NUnit.Framework;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;

namespace TNT.Tests.Cord.Serializers
{
    [TestFixture()]
    public class DateTimeSerializationTest
    {
        [Test]
        public void DateTimeOffset_SerializeAndBack_ValuesAreEqual()
        {
            var value = DateTimeOffset.Now;

            using (var result = new MemoryStream())
            {
                var primitiveSerializator = new UTCFileTimeAndOffsetSerializer();
                primitiveSerializator.SerializeT(value, result);

                result.Position = 0;

                var deserialized  = new UTCFileTimeAndOffsetDeserializer().DeserializeT(result, (int)primitiveSerializator.Size);

                Assert.AreEqual(value.Offset,  deserialized.Offset);
                Assert.Less(Math.Abs((value.DateTime - deserialized.DateTime).TotalMilliseconds) , 1);
            }

        }
    }
}
