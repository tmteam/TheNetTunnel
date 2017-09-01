using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Presentation.Serializers
{
    public class ByteArraySerializer : SerializerBase<byte[]>
    {
        public ByteArraySerializer()
        {
            Size = null;
        }

        public override void SerializeT(byte[] obj, MemoryStream stream)
        {
            if (obj == null)
                return;
            stream.Write(obj,0, obj.Length);
        }
    }
}
