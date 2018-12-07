using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Presentation.Deserializers
{
    public class ByteArrayDeserializer : DeserializerBase<byte[]>
    {
        public override byte[] DeserializeT(System.IO.Stream stream, int size)
        {
            if (size == 0)
            return new byte[0];
            //array
            var ans = new byte[size];
            stream.Read(ans, 0, size);
            return ans;
        }
    }
}
