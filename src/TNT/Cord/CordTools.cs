using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNT.Cord
{
    public static  class CordTools
    {
        public static void WriteShort(short outputCordId, MemoryStream to)
        {
            //Write first byte
            to.WriteByte((byte)(outputCordId & 0xFF));
            //Write second byte
            to.WriteByte((byte)(outputCordId >> 8));
        }

        public static short ReadShort(MemoryStream from)
        {
            return (short)(from.ReadByte() + (from.ReadByte()>>8));
        }
    }
}
