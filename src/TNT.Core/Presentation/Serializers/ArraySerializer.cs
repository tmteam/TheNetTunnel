using System;

namespace TNT.Presentation.Serializers;

public class ArraySerializer<TArray> : SerializerBase<TArray>
{
    private readonly ISerializer _memberSerializer;
    private readonly bool _isFix = false;


    public ArraySerializer(SerializerFactory serializerFactory)
    {
        Size = null;
        var memberType = typeof(TArray).GetElementType();
        _memberSerializer = serializerFactory.Create(memberType);
        if (_memberSerializer.Size.HasValue)
        {
            _isFix = true;
        }
    }

    public override void SerializeT(TArray obj, System.IO.MemoryStream stream)
    {
        if (_isFix)
            SerializeFix(obj, stream);
        else
            SerializeDyn(obj, stream);
    }

    public void SerializeFix(TArray obj, System.IO.MemoryStream stream)
    {
        var TArray = obj as Array;

        for (int i = 0; i < TArray.Length; i++)
            _memberSerializer.Serialize(TArray.GetValue(i), stream);
    }

    public void SerializeDyn(TArray obj, System.IO.MemoryStream stream)
    {
        var TArray = obj as Array;

        for (int i = 0; i < TArray.Length; i++)
        {
            var sPos = stream.Position;
            stream.Write(new byte[] {0, 0, 0, 0}, 0, 4);
            _memberSerializer.Serialize(TArray.GetValue(i), stream);

            var len = BitConverter.GetBytes((int) (stream.Position - sPos - 4));
            stream.Position = sPos;
            stream.Write(len, 0, 4);
            stream.Position = stream.Length;
        }
    }
}