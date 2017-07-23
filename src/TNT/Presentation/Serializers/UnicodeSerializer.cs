using System.IO;
using System.Text;

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
            var sw = new StreamWriter(stream, Encoding.Unicode);
            sw.Write(obj);
            sw.Flush();
        }
    }
}