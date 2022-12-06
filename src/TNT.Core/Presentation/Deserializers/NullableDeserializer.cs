using System.IO;
using System.Runtime.InteropServices;

namespace TNT.Presentation.Deserializers;

public class NullableDeserializer<T> : DeserializerBase<T?> where T : struct
{
    public NullableDeserializer()
    {
        Size = Marshal.SizeOf(typeof(T))+1;
    }

    public override T? DeserializeT(Stream stream, int size)
    {
        var arr = new byte[Size.Value];
        stream.Read(arr, 0, Size.Value);
        if (arr[0] == 0)
            return null;
        else
        {
            return Tools.ToStruct<T>(arr, 1, Size.Value-1);
        }
    }
}