using System;
using System.IO;
using NUnit.Framework;
using TNT.Exceptions.Remote;
using TNT.Presentation;
using TNT.Presentation.Deserializers;
using TNT.Presentation.Serializers;

namespace TNT.Tests.Serialization
{
    [TestFixture()]
    public class GeneralSerializersTest
    {
        [Test]
        public void DateTimeOffset_SerializeAndBack_ValuesAreEqual()
        {
            var value = DateTimeOffset.Now;

            var deserialized =
                SerializeTestHelper.SerializeAndBack<UTCFileTimeAndOffsetSerializer, UTCFileTimeAndOffsetDeserializer, DateTimeOffset>(value);

            Assert.AreEqual(value.Offset,  deserialized.Offset);
            Assert.Less(Math.Abs((value.DateTime - deserialized.DateTime).TotalMilliseconds) , 1);
        }

        [Test]
        public void DateTime_SerializeAndBack_ValuesAreEqual()
        {
            var value = new DateTime(2017, 10, 13, 14, 23, 15);
            var deserialized =
                SerializeTestHelper.SerializeAndBack<UTCFileTimeSerializer, UTCFileTimeDeserializer, DateTime>(value);
            Assert.Less(Math.Abs((value - deserialized).TotalMilliseconds), 1);
        }
        
        [TestCase("Taram pam pam")]
        [TestCase("")]
        [TestCase(null)]
        public void Unicode_SerializeAndBack_ValuesAreEqual(string value)
        {
            var deserialized =
                SerializeTestHelper.SerializeAndBack<UnicodeSerializer, UnicodeDeserializer, string>(value);
            Assert.AreEqual(value, deserialized);
        }
        [TestCase(null, null, ErrorType.ContractSignatureError, null)]
        [TestCase(null, null, ErrorType.SerializationError, null)]
        [TestCase(null, null, ErrorType.UnhandledUserExceptionError, "")]
        [TestCase(1, null, ErrorType.SerializationError, "")]
        [TestCase(101, null, ErrorType.ContractSignatureError, "")]
        [TestCase(null, 102, ErrorType.UnhandledUserExceptionError, null)]
        [TestCase(null, 102, ErrorType.ContractSignatureError, "hi")]
        [TestCase(null, null, ErrorType.SerializationError, "wooow")]
        [TestCase(111, 112, ErrorType.SerializationError, "mamaaa")]
        [TestCase(111, 112, ErrorType.ContractSignatureError, null)]
        [TestCase(111, 112, ErrorType.ContractSignatureError, "")]
        public void ErrorMessage_SerializeAndBack_ValuesAreEqual(short? messageId, short? askId, ErrorType type, string message)
        {
            var em = new ErrorMessage(messageId, askId, type, message);
            var deserialized = SerializeTestHelper.SerializeAndBack<ErrorMessageSerializer, ErrorMessageDeserializer, ErrorMessage>(em);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(em.AskId, deserialized.AskId);
                Assert.AreEqual(em.MessageId, deserialized.MessageId);
                Assert.AreEqual(em.ErrorType, deserialized.ErrorType);
                Assert.AreEqual(em.AdditionalExceptionInformation, deserialized.AdditionalExceptionInformation);
            });
        }
    }
}
