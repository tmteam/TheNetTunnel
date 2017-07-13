using System;
using System.IO;

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
            if(from.Length- from.Position<2)
                throw new EndOfStreamException();
            byte[] arr = new byte[2];
            from.Read(arr, 0,2);

            return BitConverter.ToInt16(arr, 0);   //(short)(from.ReadByte() + (from.ReadByte()>>8));
        }
    }
}
