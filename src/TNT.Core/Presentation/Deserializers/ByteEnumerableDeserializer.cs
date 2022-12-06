using System;
using System.Collections.Generic;
using System.Linq;

namespace TNT.Presentation.Deserializers;

public class ByteEnumerableDeserializer : DeserializerBase<IEnumerable<byte>>
{
    const int minListSize = 2048;

    public override IEnumerable<byte> DeserializeT(System.IO.Stream stream, int size)
    {
        if (size == 0)
            return Array.Empty<byte>();
        if (size < minListSize)
        {
            //array
            var ans = new byte[size];
            stream.Read(ans, 0, size);
            return ans;
        }
        // list
        var lans = new List<byte>(size);
        var lasts = size;
        var buff = new byte[minListSize];
        while (lasts > minListSize)
        {
            stream.Read(buff, 0, minListSize);
            lans.AddRange(buff);
            lasts -= minListSize;
        }
        if (lasts < 1)
            return lans;

        stream.Read(buff, 0, lasts);
        lans.AddRange(buff.Take(lasts));

        return lans;
    }
}