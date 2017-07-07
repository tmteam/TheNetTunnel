using System;

namespace TNT.Cord.Serializers
{
    public class UTCFileTimeAndOffsetSerializer : SerializerBase<DateTimeOffset>
    {
        static readonly ValueTypeSerializer<int> intSerializer = new ValueTypeSerializer<int>();
        public UTCFileTimeAndOffsetSerializer()
        {
            Size = sizeof(long) + intSerializer.Size;
        }

        public override void SerializeT(DateTimeOffset obj, System.IO.MemoryStream stream)
        {
            var lng = obj.DateTime.ToFileTimeUtc();
            lng.WriteToStream<long>(stream, sizeof(long));

            var offset = (int) obj.Offset.TotalSeconds;
            intSerializer.SerializeT(offset,stream);
        }
    }

  
}