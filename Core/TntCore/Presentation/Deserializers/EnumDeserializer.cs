using System;
using System.IO;

namespace TNT.Presentation.Deserializers
{
    public class EnumDeserializer<T> : DeserializerBase<T> 
        where T: struct 
    {
        private readonly IDeserializer primitive;
        
        public EnumDeserializer()
        {
            if(!(typeof(T).IsEnum))
                throw  new InvalidOperationException("Type \""+typeof(T)+"\" must be enum type");
            var underLying = Enum.GetUnderlyingType(typeof(T));

            var serializerType = typeof(ValueTypeDeserializer<>).MakeGenericType(underLying);
            primitive = (IDeserializer)Activator.CreateInstance(serializerType);
            Size      = primitive.Size;
        }


        public override T DeserializeT(Stream stream, int size)
        {
            return (T)primitive.Deserialize(stream, size);
        }
    }

}

