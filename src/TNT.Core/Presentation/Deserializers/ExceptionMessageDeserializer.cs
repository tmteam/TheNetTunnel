using System.IO;
using TNT.Exceptions.Remote;

namespace TNT.Presentation.Deserializers;

public class ErrorMessageDeserializer: DeserializerBase<ErrorMessage>
{
    private readonly SequenceDeserializer _deserializer;

    public ErrorMessageDeserializer()
    {
        this.Size = null;
        _deserializer = new SequenceDeserializer(new IDeserializer[]
        {
            new NullableDeserializer<short>(),
            new NullableDeserializer<short>(),
            new EnumDeserializer<ErrorType>(),
            new UnicodeDeserializer()
        });
    }
    public override ErrorMessage DeserializeT(Stream stream, int size)
    {
        var deserialized = _deserializer.DeserializeT(stream, size);
        return new ErrorMessage
        (
            messageId: (short?) deserialized[0],
            askId: (short?) deserialized[1],
            type: (ErrorType) deserialized[2],
            additionalExceptionInformation: (string) deserialized[3]
        );
    }
}