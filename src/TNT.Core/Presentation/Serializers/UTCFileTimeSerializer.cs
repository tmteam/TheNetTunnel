using System;

namespace TNT.Presentation.Serializers
{
    public class UTCFileTimeSerializer : SerializerBase<DateTime>
    {
        public UTCFileTimeSerializer()
        {
            Size = sizeof(long);
        }

        public override void SerializeT(DateTime obj, System.IO.MemoryStream stream)
        {
            var lng = obj.ToFileTimeUtc();
            lng.WriteToStream<long>(stream, Size.Value);
        }
    }
}