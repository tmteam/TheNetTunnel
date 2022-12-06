using System;
using System.Reflection;

namespace TNT.Presentation.Serializers;

public class EnumSerializer<T> : SerializerBase<T> 
    where T: struct 
{
    private readonly ISerializer primitive;
        
    public EnumSerializer()
    {
        if(!(typeof(T).GetTypeInfo().IsEnum))
            throw  new InvalidOperationException("Type \""+typeof(T)+"\" must be enum type");
        var underLying = Enum.GetUnderlyingType(typeof(T));

        var serializerType = typeof(ValueTypeSerializer<>).MakeGenericType(underLying);
        primitive = (ISerializer)Activator.CreateInstance(serializerType);
        Size      = primitive.Size;
    }

    public override void SerializeT(T obj, System.IO.MemoryStream stream)
    {
        primitive.Serialize(obj, stream);
    }
}