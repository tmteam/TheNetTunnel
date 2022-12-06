using System.Collections.Generic;
using System.IO;

namespace TNT.Presentation.Serializers;

public class ByteEnumerableSerializer : SerializerBase<IEnumerable<byte>>
{
    public ByteEnumerableSerializer()
    {
        Size = null;
    }

    public override void SerializeT(IEnumerable<byte> obj, MemoryStream stream)
    {
        if (obj == null)
            return;
        foreach (var b in obj)
            stream.WriteByte(b);
    }
}