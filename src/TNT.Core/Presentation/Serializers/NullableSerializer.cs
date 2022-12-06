using System.IO;
using System.Runtime.InteropServices;

namespace TNT.Presentation.Serializers;

public class NullableSerializer<T>: SerializerBase<T?> where T: struct
{
    public NullableSerializer()
    {
        Size = Marshal.SizeOf(typeof(T)) + 1;
    }

    public override void SerializeT(T? obj, MemoryStream stream)
    {
        if (obj == null)
            stream.Write(new byte[Size.Value], 0, Size.Value);
        else
        {
            stream.WriteByte(1);
            obj.Value.WriteToStream(stream, Size.Value -1);
        }
    }
}