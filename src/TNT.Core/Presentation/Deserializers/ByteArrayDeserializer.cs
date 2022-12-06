using System;

namespace TNT.Presentation.Deserializers;

public class ByteArrayDeserializer : DeserializerBase<byte[]>
{
    public override byte[] DeserializeT(System.IO.Stream stream, int size)
    {
        if (size == 0)
            return Array.Empty<byte>();
        //array
        var ans = new byte[size];
        stream.Read(ans, 0, size);
        return ans;
    }
}