using System;
using System.IO;

namespace TNT.Cord.Deserializers
{
    public class UTCFileTimeAndOffsetDeserializer : DeserializerBase<DateTimeOffset>
    {
        public UTCFileTimeAndOffsetDeserializer()
        {
            Size = longDeserializer.Size + intTypeDeserializer.Size;

        }

        private static readonly ValueTypeDeserializer<long> longDeserializer = new ValueTypeDeserializer<long>();
        private static readonly ValueTypeDeserializer<int> intTypeDeserializer = new ValueTypeDeserializer<int>();
        public override DateTimeOffset DeserializeT(Stream stream, int size)
        {
            var dateTimeUtc = longDeserializer.DeserializeT(stream, longDeserializer.Size.Value);
            var dateTime = DateTime.FromFileTimeUtc(dateTimeUtc);
            var offsetInSec = intTypeDeserializer.DeserializeT(stream, intTypeDeserializer.Size.Value);


            return new DateTimeOffset(
                dateTime: DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified),
                offset:   TimeSpan.FromSeconds(offsetInSec));
        }
    }
}