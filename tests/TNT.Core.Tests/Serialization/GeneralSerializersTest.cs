using System;
using System.IO;
using NUnit.Framework;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;

namespace TNT.Core.Tests.Serialization
{
    [TestFixture]
    public class GeneralSerializersTest
    {
        [Test]
        public void DateTimeOffset_SerializeAndBack_ValuesAreEqual()
        {
            var value = DateTimeOffset.Now;

            var deserialized =
                SerializeAndBack<UTCFileTimeAndOffsetSerializer, UTCFileTimeAndOffsetDeserializer, DateTimeOffset>(value);

            Assert.AreEqual(value.Offset,  deserialized.Offset);
            Assert.Less(Math.Abs((value.DateTime - deserialized.DateTime).TotalMilliseconds) , 1);
        }

        [Test]
        public void DateTime_SerializeAndBack_ValuesAreEqual()
        {
            var value = new DateTime(2017, 10, 13, 14, 23, 15);

            var deserialized =
                SerializeAndBack<UTCFileTimeSerializer, UTCFileTimeDeserializer, DateTime>(value);

            Assert.Less(Math.Abs((value - deserialized).TotalMilliseconds), 1);
        }

        [TestCase("Taram pam pam")]
        [TestCase("")]
        [TestCase(null)]
        public void Unicode_SerializeAndBack_ValuesAreEqual(string value)
        {
            var deserialized =
                SerializeAndBack<UnicodeSerializer, UnicodeDeserializer, string>(value);
            Assert.AreEqual(value, deserialized);
        }


        T SerializeAndBack<TSerializer, TDeserializer, T>(T value)
          where TSerializer : ISerializer<T>, new()
          where TDeserializer : IDeserializer<T>, new()
        {
            using (var result = new MemoryStream())
            {
                var serializer = new TSerializer();
                serializer.SerializeT(value, result);

                result.Position = 0;
                return new TDeserializer().DeserializeT(result, (int)result.Length);
            }
        }
    }
}
