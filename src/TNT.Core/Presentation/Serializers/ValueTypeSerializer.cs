using System.IO;
using System.Runtime.InteropServices;

namespace TNT.Presentation.Serializers;

public class ValueTypeSerializer<T> : SerializerBase<T> where T: struct 
{
    public ValueTypeSerializer()
    {
        Size = Marshal.SizeOf(typeof(T));
    }

    public override void SerializeT(T obj, MemoryStream stream)
    {
        obj.WriteToStream(stream, Size.Value);
    }
}