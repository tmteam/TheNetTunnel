using System;
using System.IO;
using System.Text;
using TNT.Presentation.Deserializers;

namespace TNT.Presentation.Serializers
{
    public class UnicodeSerializer : SerializerBase<string>
    {
        public UnicodeSerializer()
        {
            Size = null;
        }

        public override void SerializeT(string obj, System.IO.MemoryStream stream)
        {
            //Null and empty string cases are different. 
            if(obj==null)
                return;
            if (obj == String.Empty)
            {
                //"" string can be seialized in different ways (zero length or U+FEFF. So we have to serialize it manualy 
                //U+0003 - EndOfStrinh marker.
                Tools.WriteShort(0x0003, stream);
                return;
            }
            var sw = new StreamWriter(stream, Encoding.Unicode);
            sw.Write(obj);
            sw.Flush();
        }
    }
}