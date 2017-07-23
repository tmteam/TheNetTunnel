using System.IO;
using TNT.Exceptions.Remote;

namespace TNT.Presentation.Deserializers
{
    public class ExceptionMessageDeserializer: DeserializerBase<ExceptionMessage>
    {
        private readonly SequenceDeserializer _deserializer;

        public ExceptionMessageDeserializer()
        {
            this.Size = null;
            _deserializer = new SequenceDeserializer(new IDeserializer[]
            {
                  new ValueTypeDeserializer<short>(),
                  new ValueTypeDeserializer<short>(),
                  new EnumDeserializer<RemoteExceptionId>(),
                  new UnicodeDeserializer()
            });
        }
        public override ExceptionMessage DeserializeT(Stream stream, int size)
        {
            var deserialized = _deserializer.DeserializeT(stream, size);
            return new ExceptionMessage
            (
                messageId: (short) deserialized[0],
                askId: (short) deserialized[1],
                type: (RemoteExceptionId) deserialized[2],
                additionalExceptionInformation: (string) deserialized[3]
            );
        }
    }
}
