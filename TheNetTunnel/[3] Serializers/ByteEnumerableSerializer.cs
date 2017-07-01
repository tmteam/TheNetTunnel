using System.Collections.Generic;

namespace TNT.Serialization
{
    public class ByteEnumerableSerializer:SerializerBase<IEnumerable<byte>>
    {
        public ByteEnumerableSerializer() {
            Size = null;
        }

        public override void SerializeT(IEnumerable<byte> obj, System.IO.Stream stream)
        {
            if (obj == null)
                return;
            foreach (var b in obj)
                stream.WriteByte(b);
        }
    }
}
