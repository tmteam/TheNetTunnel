using System.IO;
using TNT.Exceptions.Remote;

namespace TNT.Presentation.Serializers
{
    public class ExceptionMessageSerializer: SerializerBase<ExceptionMessage>
    {
        private  readonly SequenceSerializer _serializer;

        public ExceptionMessageSerializer()
        {
            this.Size = null;
            _serializer = new SequenceSerializer(
                   new ISerializer[]
                   {
                        new ValueTypeSerializer<short>(),
                        new ValueTypeSerializer<short>(),
                        new EnumSerializer<RemoteExceptionId>(),
                        new UnicodeSerializer()
                   });
        }
        public override void SerializeT(ExceptionMessage obj, MemoryStream stream)
        {
             _serializer.SerializeT(
                    new object[] {obj.MessageId, obj.AskId, obj.ExceptionType, obj.AdditionalExceptionInformation}, stream);

        }
    }
}
